using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class PropertyValidatorBase : IPropertyValidator
    {
        protected PropertyValidatorBase( IJ4JLogger? logger )
        {
            Logger = logger;
        }

        protected IJ4JLogger? Logger { get; }
        protected bool IsBindable { get; private set; }
        protected string? PropertyPath { get; private set; }
        protected bool IsOuterMostLeaf { get; private set; }

        public virtual bool IsPropertyBindable( Stack<PropertyInfo> propertyStack )
        {
            IsBindable = true;

            PropertyPath = propertyStack.Aggregate(
                new StringBuilder(),
                (sb, pi) =>
                {
                    if (sb.Length > 0)
                        sb.Append(":");

                    sb.Append(pi.Name);

                    return sb;
                },
                sb => sb.ToString()
            );

            IsOuterMostLeaf = propertyStack.Count == 1;

            return true;
        }

        protected void LogError(string error, string? hint = null)
        {
            if (Logger == null)
                return;

            IsBindable = false;

            Logger.Error<string, string, string>("PropertyValidation error -- {0}{1} - {2}",
                PropertyPath!,
                hint == null ? string.Empty : $" ({hint})",
                error);
        }
    }
}