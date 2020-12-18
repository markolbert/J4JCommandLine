using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionsBase : IEnumerable<Option>
    {
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
        protected List<Option> Options { get; } = new();

        public int Count => Options.Count;

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

            return Options.Any( x =>
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

                return Options.FirstOrDefault( opt =>
                    opt.IsInitialized
                    && opt.Keys.Any( k => string.Equals( k, key, MasterText.TextComparison!.Value ) )
                );
            }
        }

        public IEnumerator<Option> GetEnumerator()
        {
            foreach( var option in Options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}