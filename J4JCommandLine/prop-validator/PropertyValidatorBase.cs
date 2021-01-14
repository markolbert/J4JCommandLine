using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class PropertyValidatorBase : IPropertyValidator
    {
        private readonly IConverters _converters;

        protected PropertyValidatorBase( 
            IConverters converters,
            IJ4JLogger? logger )
        {
            _converters = converters;
            Logger = logger;
        }

        protected IJ4JLogger? Logger { get; }
        protected bool IsBindable { get; private set; }
        protected string? PropertyPath { get; private set; }
        protected bool IsOuterMostLeaf { get; private set; }

        protected bool CanConvert(Type toCheck) => _converters.CanConvert(toCheck);

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

            CheckBasicBindability( propertyStack.Peek().PropertyType );

            return IsBindable;
        }

        public virtual bool IsPropertyBindable( Type propType )
        {
            IsBindable = true;
            PropertyPath = propType.Name;
            IsOuterMostLeaf = true;

            CheckBasicBindability( propType );

            return IsBindable;
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

        private void CheckBasicBindability( Type toCheck )
        {
            var bindableInfo = BindableTypeInfo.Create(toCheck);

            if( bindableInfo.BindableType == BindableType.Unsupported )
                LogError( $"{toCheck} is neither a simple type, nor an array/list of simple types" );
        }
    }
}