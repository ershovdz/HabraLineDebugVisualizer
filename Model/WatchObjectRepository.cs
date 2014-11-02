using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerService
{
    public abstract class IWatchObjectsSource
    {
        public abstract Dictionary<String, LineObject> getLineObjects();
        public abstract void removeAllObjects();
        public abstract void reloadAllObjects();

        public event EventHandler<EventArgs> WatchObjectsUpdated;

        protected virtual void OnWatchObjectsUpdated()
        {
            EventHandler<EventArgs> handler = WatchObjectsUpdated;
            if( handler != null )
            {
                handler( this, new EventArgs() );
            }
        }
    }


    public class WatchObjectRepository : IWatchObjectsSource
    {
        public Dictionary<String, LineObject> _lineObjects = new Dictionary<String, LineObject>();

        public override Dictionary<String, LineObject> getLineObjects()
        {
            return new Dictionary<String, LineObject>( _lineObjects );
        }

        #region implementation

        public override void removeAllObjects()
        {
            _lineObjects.Clear();
            OnWatchObjectsUpdated();
        }

        public override void reloadAllObjects()
        {
            foreach( var lineObj in _lineObjects )
            {
                lineObj.Value.Reload();
            }

            OnWatchObjectsUpdated();
        }

        public void AddLine( String name, String serializedDataPath )
        {
            bool hasObject = _lineObjects.ContainsKey( name );
            if( !hasObject )
            {
                _lineObjects[name] = new LineObject( serializedDataPath );
            }
            else
            {
                _lineObjects[name].Init( serializedDataPath );
            }

            OnWatchObjectsUpdated();
        }       

        #endregion
    }
}
