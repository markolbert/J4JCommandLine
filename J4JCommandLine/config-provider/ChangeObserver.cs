using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    // thanx to Peter van den Hout for this!
    // https://ofpinewood.com/blog/creating-a-custom-configurationprovider-for-a-entity-framework-core-source
    public class ChangeObserver
    {
        public event EventHandler<ConfigurationChangedEventArgs>? Changed;

        #region singleton

        private static readonly Lazy<ChangeObserver> lazy = new Lazy<ChangeObserver>( () => new ChangeObserver() );
        public static ChangeObserver Instance => lazy.Value;

        #endregion

        private ChangeObserver()
        {
        }

        public void OnChanged( string newCommandLine ) => ThreadPool.QueueUserWorkItem( ( _ ) =>
            Changed?.Invoke( this, new ConfigurationChangedEventArgs { NewCommandLine = newCommandLine } ) );

        public void OnChanged() => ThreadPool.QueueUserWorkItem( ( _ ) =>
            Changed?.Invoke( this, new ConfigurationChangedEventArgs { OptionsConfigurationChanged = true } ) );
    }
}
