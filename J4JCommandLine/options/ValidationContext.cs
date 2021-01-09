using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine
{
    public class ValidationContext
    {
        public ValidationContext(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;
            Entries.Push( new ValidationEntry<PropertyInfo>( propInfo, null, this ) );
        }

        public Stack<IValidationEntry> Entries { get; } = new Stack<IValidationEntry>();
        public IValidationEntry Current => Entries.Peek();

        public PropertyInfo PropertyInfo { get; }

        public List<ValidationError> Errors => Entries.SelectMany( x => x.Errors ).ToList();
        public bool IsValid => !Errors.Any();
    }
}