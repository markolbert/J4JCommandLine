using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public abstract class DisplayHelpBase : IDisplayHelp
    {
        protected DisplayHelpBase( IJ4JLogger? logger )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public abstract void ProcessOptions( IOptionCollection options );

        protected List<string> GetKeys( IOption option )
        {
            var retVal = new List<string>();

            foreach (var prefix in option.Container.MasterText[TextUsageType.Prefix])
            {
                foreach (var key in option.Keys)
                {
                    retVal.Add($"{prefix}{key}");
                }
            }

            return retVal;
        }

        protected string GetStyleText( IOption option )
        {
                var reqdText = option.Required ? "must" : "can";

                switch ( option.Style )
                {
                    case OptionStyle.Collection:
                        var sb = new StringBuilder();

                        sb.Append( option.MaxValues == int.MaxValue ? $"one to {option.MaxValues:n0}" : "one or more" );
                        sb.Append( $" values {reqdText} be specified" );

                        return sb.ToString();

                    case OptionStyle.ConcatenatedSingleValue:
                        return $"one or more related values (e.g., flagged enums) {reqdText} be specified";

                    case OptionStyle.SingleValued:
                        return $"a single value {reqdText} be specified";

                    case OptionStyle.Switch:
                        return "any value specified will be ignored";
                }

                throw new InvalidEnumArgumentException( $"Unsupported {typeof(OptionStyle)} '{option.Style}'" );
        }
    }
}