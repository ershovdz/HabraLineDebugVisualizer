using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Serialization;
using HelixToolkit.Wpf;
using System;

namespace VisualizerService
{
    using ID = String;
    using NAME = String;
    using System.Collections.ObjectModel;

    public class ObjectToVisualize
    {
        public Boolean IsVisible { get; set; }
        public Boolean IsValid { get; set; }

        public ObjectToVisualize( Boolean visible, Boolean valid )
        {
            IsVisible = visible;
            IsValid = valid;
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public Dictionary<NAME, ObjectToVisualize> _watchList { get; set; }
        public Model3DGroup _meshModel { get; set; }
        public LinesVisual3D _lineModel { get; set; }

        private double _gridStep;
        public double GridStep
        {
            get
            {
                return _gridStep;
            }
            set
            {
                _gridStep = value;
                RaisePropertyChanged( "GridStep" );
            }
        }

        private double _gridMinSize;

        private Rect3D _gridGabarit;
        public Rect3D GridGabarit
        {
            get
            {
                return _gridGabarit;
            }
            set
            {
                _gridGabarit = value;
                RaisePropertyChanged( "GridGabarit" );
            }
        }

        private Rect3D _gridSubstrate;
        public Rect3D GridSubstrate
        {
            get
            {
                return _gridSubstrate;
            }
            set
            {
                _gridSubstrate = value;
                RaisePropertyChanged( "GridSubstrate" );
            }
        }

        private Point3D _defaultCameraPosition;
        public Point3D DefaultCameraPosition
        {
            get
            {
                return _defaultCameraPosition;
            }
            set
            {
                _defaultCameraPosition = value;
                RaisePropertyChanged( "DefaultCameraPosition" );
            }
        }

        private Vector3D _defaultLookDirection;
        public Vector3D DefaultLookDirection
        {
            get
            {
                return _defaultLookDirection;
            }
            set
            {
                _defaultLookDirection = value;
                RaisePropertyChanged( "DefaultLookDirection" );
            }
        }

        // intermediate data
        static private Color _meshColor { get; set; }
        private Dictionary<String, Model3D> _modelToMesh { get; set; }
        private IWatchObjectsSource _watchObjectsSource;

        public ViewModel( IWatchObjectsSource source )
        {
            _watchObjectsSource = source;

            _meshColor = Color.FromArgb( 0x80, 10, 230, 10 );
            _meshModel = new Model3DGroup();
            _lineModel = new LinesVisual3D();
            _watchList = new Dictionary<NAME, ObjectToVisualize>();

            _gridMinSize = 24000;
            GridStep = 1000;
            GridGabarit = new Rect3D( 0, 0, 0, _gridMinSize, _gridMinSize, 0 );
            GridSubstrate = new Rect3D( 0, 0, 0, _gridMinSize - 2 * GridStep, _gridMinSize - 2 * GridStep, 0 );

            DefaultCameraPosition = new Point3D( _gridMinSize, 0, _gridMinSize );
            DefaultLookDirection = new Vector3D( -DefaultCameraPosition.X, 0, -DefaultCameraPosition.Z );

            UpdateData();

            _watchObjectsSource.WatchObjectsUpdated += OnWatchSourceUpdated;
        }

        private void OnWatchSourceUpdated( object sender, EventArgs e )
        {
            UpdateData();
        }

        public void UpdateData()
        {
            Dictionary<String, LineObject> lines = _watchObjectsSource.getLineObjects();

            UpdateListViewModel( lines );

            UpdateSceneModel( lines );

            UpdateGridSize();

            UpdateDefaultCameraPosition();
        }

        public void SetVisibility( String name, bool visible )
        {
            _watchList[name].IsVisible = visible;
            UpdateData();
        }

        public void ReloadModels()
        {
            _watchObjectsSource.reloadAllObjects();
            UpdateData();
        }

        public void RemoveAllModels()
        {
            _watchObjectsSource.removeAllObjects();
            UpdateData();
        }

        private void UpdateSceneModel( Dictionary<String, LineObject> lines )
        {
            _lineModel.Points.Clear();

            Point3DCollection points = new Point3DCollection();
            foreach( var line in lines )
            {
                if( _watchList.ContainsKey( line.Key ) && _watchList[line.Key].IsVisible )
                {
                    foreach( var point in line.Value._points )
                    {
                        points.Add( point );
                    }
                }
            }

            _lineModel.Points = points;
            _lineModel.Thickness = 3;
            _lineModel.Color = Color.FromRgb( 255, 0, 0 );

            RaisePropertyChanged("_lineModel");
        }

        private void UpdateGridSize()
        {
            double modelWidth = System.Math.Max( _meshModel.Bounds.SizeX, _lineModel.Content.Bounds.SizeX );
            double modelLength = System.Math.Max( _meshModel.Bounds.SizeY, _lineModel.Content.Bounds.SizeY );

            double gridWidth = modelWidth > _gridMinSize ? modelWidth : _gridMinSize;
            double gridLength = modelLength > _gridMinSize ? modelLength : _gridMinSize;

            double gridSubstrateWidth = gridWidth - 2 * GridStep;
            double gridSubstrateLength = gridLength - 2 * GridStep;

            GridGabarit = new Rect3D( 0, 0, 0, gridWidth, gridLength, 0 );
            GridSubstrate = new Rect3D( 0, 0, 0, gridSubstrateWidth, gridSubstrateLength, 0 );
        }

        private void UpdateDefaultCameraPosition()
        {
            double modelWidth = System.Math.Max( _meshModel.Bounds.SizeX, _lineModel.Content.Bounds.SizeX );
            double cameraXPosition = modelWidth > _gridMinSize ? modelWidth : _gridMinSize;

            double modelHeight = System.Math.Max( _meshModel.Bounds.SizeZ, _lineModel.Content.Bounds.SizeZ );
            double cameraZPosition = modelHeight > _gridMinSize ? modelHeight : _gridMinSize;

            DefaultCameraPosition = new Point3D( cameraXPosition, 0, cameraZPosition );
            DefaultLookDirection = new Vector3D( -DefaultCameraPosition.X, 0, -DefaultCameraPosition.Z );
        }

        private void UpdateListViewModel( Dictionary<String, LineObject> lines )
        {
            Dictionary<NAME, ObjectToVisualize> newObjectListToVisualize = new Dictionary<NAME, ObjectToVisualize>();
            
            foreach( var line in lines )
            {
                Boolean isVisible = true;

                if( _watchList.ContainsKey( line.Key ) )
                {
                    isVisible = _watchList[line.Key].IsVisible;
                }

                ObjectToVisualize obj = new ObjectToVisualize( isVisible, line.Value._isValid );
                newObjectListToVisualize[line.Key] = obj;
            }

            _watchList = newObjectListToVisualize;
            RaisePropertyChanged( "_watchList" );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged( string property )
        {
            var handler = PropertyChanged;
            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( property ) );
            }
        }
    }
}