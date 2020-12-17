using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionsBase : IEnumerable<Option>
    {
        private readonly List<Option> _options = new List<Option>();

        protected OptionsBase( 
            MasterTextCollection masterText,
            IJ4JLogger logger
        )
        {
            MasterText = masterText;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }
        protected MasterTextCollection MasterText { get; }

        public int Count => _options.Count;

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool UsesCommandLineKey( string key ) => MasterText.Contains( key, TextUsageType.OptionKey );

        public bool UsesContextPath( string contextPath )
        {
            if( !MasterText.IsValid )
            {
                Logger.Error( "MasterTextCollection is not initialized" );
                return false;
            }

            return _options.Any( x =>
                x.ContextPath?.Equals( contextPath, MasterText.TextComparison!.Value ) ?? false );
        }

        public Option? this[ string key ]
        {
            get
            {
                if (!MasterText.IsValid)
                {
                    Logger.Error("MasterTextCollection is not initialized");
                    return null;
                }

                return _options.FirstOrDefault( opt =>
                    opt.IsInitialized
                    && opt.Keys.Any( k => string.Equals( k, key, MasterText.TextComparison!.Value ) )
                );
            }
        }

        protected Option AddInternal(string contextPath )
        {
            var retVal = new Option( this, contextPath, MasterText );

            _options.Add(retVal);

            return retVal;
        }

        public IEnumerator<Option> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}