using System.Linq;
#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public partial class BindingTargetBuilder
    {
        private class Configuration
        {
            public bool Initialize<TValue>( BindingTargetBuilder builder, TValue? value = null )
                where TValue : class
            {
                Logger = new CommandLineLogger( builder._textComp );

                MasterText = new MasterTextCollection(builder._textComp);
                MasterText.AddRange(TextUsageType.Prefix, builder._prefixes);
                MasterText.AddRange(TextUsageType.ValueEncloser, builder._enclosers);

                if( builder._quotes != null )
                    MasterText.AddRange(TextUsageType.Quote, builder._quotes.Select(q => q.ToString()));

                if ( builder._helpKeys == null || builder._helpKeys.Length == 0 )
                {
                    Logger.LogError( ProcessingPhase.Initializing, $"No help keys defined" );
                    return false;
                }

                if (value == null && !typeof(TValue).HasPublicParameterlessConstructor())
                {
                    Logger.LogError(ProcessingPhase.Initializing,
                        $"{typeof(TValue)} does not have a public parameterless constructor");

                    return false;
                }

                MasterText.AddRange( TextUsageType.HelpOptionKey, builder._helpKeys );

                return true;
            }

            public CommandLineLogger Logger { get; private set; }
            public MasterTextCollection MasterText { get; private set; }
        }
    }
}