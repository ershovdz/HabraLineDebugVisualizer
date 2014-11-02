using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerService
{
    public class LineObject
    {
        public String _sharedMemoryMarker;
        public Point3DCollection _points;
        public Boolean _isValid;

        public LineObject( String marker )
        {
            Init(marker);
        }

        public void Init( String marker )
        {
            _sharedMemoryMarker = marker;
            Reload();
        }

        public void Reload()
        {
            if( _sharedMemoryMarker == null || _sharedMemoryMarker.Equals( "" ) )
            {
                _isValid = false;
                return;
            }

            CLIBridge cliBridge = new CLIBridge();
            Vertex[] vertices;
            bool isSuccess = cliBridge.getLineData( _sharedMemoryMarker, out vertices );
            if( isSuccess )
            {
                _points = new Point3DCollection( vertices.Count() );
                for( int i = 0; i < vertices.Count(); ++i )
                    _points.Add( new Point3D( vertices[i]._x, vertices[i]._y, vertices[i]._z ) );

                _isValid = true;
            }
            else
            {
                _isValid = false;
            }
        }
    }
}
