//********************************************************* 
// Copyright (c) Microsoft. All rights reserved. 
// This code is licensed under the Microsoft Public License. 
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
//********************************************************* 

namespace VisualizerService
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Debugger.Interop;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Forms.DataVisualization.Charting;
    using System.Windows.Interop;
    using System.Collections.Generic;
    
    internal class HabraLineVisualizerService : IVsCppDebugUIVisualizer, IHabraLineVisualizerService
    {
        public WatchObjectRepository _watchObjects;

        public HabraLineVisualizerService( WatchObjectRepository watchObjects )
        {
            _watchObjects = watchObjects;
        }

        public int DisplayValue( uint ownerHwnd, uint visualizerId, Microsoft.VisualStudio.Debugger.Interop.IDebugProperty3 debugProperty )
        {

            String objectName;
            String sharedMemoryMarker;
            int hr = GetHabraLineDebugData( debugProperty, out objectName, out sharedMemoryMarker );
            _watchObjects.AddLine( objectName, sharedMemoryMarker );
            return hr;
        }

        private int GetHabraLineDebugData( Microsoft.VisualStudio.Debugger.Interop.IDebugProperty3 debugProperty, out String objName, out String marker )
        {
            int hr = VSConstants.S_OK;

            DEBUG_PROPERTY_INFO[] propertyInfo = new DEBUG_PROPERTY_INFO[1];
            hr = debugProperty.GetPropertyInfo(
                enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
                10 /* Radix */,
                10000 /* Eval Timeout */,
                new IDebugReference2[] { },
                0,
                propertyInfo );

            Debug.Assert( hr == VSConstants.S_OK, "IDebugProperty3.GetPropertyInfo failed" );

            objName = propertyInfo[0].bstrFullName;

            DEBUG_PROPERTY_INFO vectorMemberPropertyInfo;
            
            vectorMemberPropertyInfo = GetHabraLineMarkerMember( propertyInfo[0] );
            
            hr = GetSharedMemoryMarker( vectorMemberPropertyInfo, out marker );
            return hr;
        }

        DEBUG_PROPERTY_INFO GetHabraLineMarkerMember(DEBUG_PROPERTY_INFO lineProperty)
        {
            return GetChildPropertyAt(2, GetChildPropertyAt(2, lineProperty));
        }

        private int GetSharedMemoryMarker(DEBUG_PROPERTY_INFO vectorMemberPropertyInfo, out String marker )
        {
            int hr = VSConstants.S_OK;
            uint elementSize = 1;

            // std::vector internally keeps pointers to the first and last elements of the dynamic array
            // First get the values of those members. We are going to use them later for reading vector elements.
            // An std::vector<int> variable has the following nodes in raw view:
            // myVector
            //      + std::_Vector_alloc<0,std::_Vec_base_types<int,std::allocator<int> > >		
            //          + std::_Vector_val<std::_Simple_types<int> >	
            //              + std::_Container_base12	
            //              + _Myfirst
            //              + _Mylast
            //              + _Myend

            // This is the underlying base class of std::vector (std::_Vector_val<std::_Simple_types<int> > node above)

            DEBUG_PROPERTY_INFO vectorBaseClassNode = GetChildPropertyAt( 0, GetChildPropertyAt( 0, vectorMemberPropertyInfo ) );

            // myFirstInfo member points to the first element
            DEBUG_PROPERTY_INFO myFirstInfo = GetChildPropertyAt( 1, vectorBaseClassNode );

            // myLastInfo member points to the last element
            DEBUG_PROPERTY_INFO myLastInfo = GetChildPropertyAt( 2, vectorBaseClassNode );

            // Vector length can be calculated by the difference between myFirstInfo and myLastInfo pointers
            ulong startAddress = ulong.Parse( myFirstInfo.bstrValue.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
            ulong endAddress = ulong.Parse( myLastInfo.bstrValue.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
            uint vectorLength = ( uint )( endAddress - startAddress ) / elementSize;

            // Now that we have the address of the first element and the length of the vector,
            // we can read the vector elements from the debuggee memory. 
            IDebugMemoryContext2 memoryContext;
            hr = myFirstInfo.pProperty.GetMemoryContext( out memoryContext );
            Debug.Assert( hr == VSConstants.S_OK, "IDebugProperty.GetMemoryContext failed" );

            IDebugMemoryBytes2 memoryBytes;
            hr = myFirstInfo.pProperty.GetMemoryBytes( out memoryBytes );
            Debug.Assert( hr == VSConstants.S_OK, "IDebugProperty.GetMemoryBytes failed" );

            // Allocate buffer on our side for copied vector elements
            byte[] vectorBytes = new byte[elementSize * vectorLength];
            uint read = 0;
            uint unreadable = 0;

            hr = memoryBytes.ReadAt( memoryContext, elementSize * vectorLength, vectorBytes, out read, ref unreadable );
            Debug.Assert( hr == VSConstants.S_OK, "IDebugMemoryBytes.ReadAt failed" );

            marker = System.Text.Encoding.UTF8.GetString( vectorBytes );
            return hr;
        }

        /// <summary>
        /// Helper method to return the child property at the given index
        /// </summary>
        /// <param name="index">The index of the child property</param>
        /// <param name="debugPropertyInfo">The parent property</param>
        /// <returns>Child property at index</returns>
        public DEBUG_PROPERTY_INFO GetChildPropertyAt(int index, DEBUG_PROPERTY_INFO debugPropertyInfo)
        {
            int hr = VSConstants.S_OK;
            DEBUG_PROPERTY_INFO[] childInfo = new DEBUG_PROPERTY_INFO[1];
            IEnumDebugPropertyInfo2 enumDebugPropertyInfo;
            Guid guid = Guid.Empty;

            hr = debugPropertyInfo.pProperty.EnumChildren(
                enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE | enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP | enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE_RAW,
                10, /* Radix */
                ref guid,
                enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_CHILD_ALL,
                null,
                10000, /* Eval Timeout */
                out enumDebugPropertyInfo);

            Debug.Assert(hr == VSConstants.S_OK, "GetChildPropertyAt: EnumChildren failed");

            if (enumDebugPropertyInfo != null)
            {
                uint childCount;
                hr = enumDebugPropertyInfo.GetCount(out childCount);
                Debug.Assert(hr == VSConstants.S_OK, "GetChildPropertyAt: IEnumDebugPropertyInfo2.GetCount failed");
                Debug.Assert(childCount > index, "Given child index out of bounds");

                hr = enumDebugPropertyInfo.Skip((uint)index);
                Debug.Assert(hr == VSConstants.S_OK, "GetChildPropertyAt: IEnumDebugPropertyInfo2.Skip failed");

                uint fetched;
                hr = enumDebugPropertyInfo.Next(1, childInfo, out fetched);
                Debug.Assert(hr == VSConstants.S_OK, "GetChildPropertyAt: IEnumDebugPropertyInfo2.Next failed");
            }

            return childInfo[0];
        }
    }
}
