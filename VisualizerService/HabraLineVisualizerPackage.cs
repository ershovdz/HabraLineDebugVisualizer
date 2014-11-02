//********************************************************* 
// Copyright (c) Microsoft. All rights reserved. 
// This code is licensed under the Microsoft Public License. 
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
//********************************************************* 

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

using System.Diagnostics;
using System.Globalization;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace VisualizerService
{
    /// <summary>
    /// Vector visualizer service exposed by the package
    /// </summary>
    [Guid( "889EB3E4-70E5-4F4F-B13A-C61D08F2E2F5" )]
    public interface IHabraLineVisualizerService { }

    [PackageRegistration( UseManagedResourcesOnly = true )]
    [ProvideService(typeof(IHabraLineVisualizerService), ServiceName = "HabraLineVisualizerService")]
    [InstalledProductRegistration("HabraLine Debug Visualizer", "HabraLine Debug Visualizer", "1.0")]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource( "Menus.ctmenu", 1 )]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow( typeof( VisualizerToolWindow ) )]
    [Guid("C37A4CFC-670F-454A-B40E-AC08578ABABD")]
    public sealed class HabraLineVisualizerPackage : Package
    {
        public const string guidVisualizerCmdSetString = "7EEF3875-C826-48A0-8FF2-B874774CE6FF";
        public WatchObjectRepository _watchObjects = new WatchObjectRepository();

        
        /// <summary>
        /// Initialization of the package; register vector visualizer service
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            IServiceContainer serviceContainer = (IServiceContainer)this;

            if (serviceContainer != null)
            {
                serviceContainer.AddService( typeof( IHabraLineVisualizerService ), new HabraLineVisualizerService( _watchObjects ), true );
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService( typeof( IMenuCommandService ) ) as OleMenuCommandService;
            if( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID( new Guid( guidVisualizerCmdSetString ), ( int )0x0101 );
                MenuCommand menuToolWin = new MenuCommand( ShowToolWindow, toolwndCommandID );
                mcs.AddCommand( menuToolWin );
            }

            ToolWindowPane window = this.FindToolWindow( typeof( VisualizerToolWindow ), 0, true );
            if( window != null )
            {
                ( window as VisualizerToolWindow ).Init( _watchObjects );
            }
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow( object sender, EventArgs e )
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow( typeof( VisualizerToolWindow ), 0, true );

            if( ( null == window ) || ( null == window.Frame ) )
            {
                throw new NotSupportedException( "Couldn't create visualizer tool window" );
            }
            ( window as VisualizerToolWindow ).Init( _watchObjects );

            IVsWindowFrame windowFrame = ( IVsWindowFrame )window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( windowFrame.Show() );
        }
    }
}
