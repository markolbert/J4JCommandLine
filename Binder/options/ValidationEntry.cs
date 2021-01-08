using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public class ValidationEntry<T> : IValidationEntry
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();
        private readonly string? _hint;

        internal ValidationEntry( T value, IValidationEntry? parent, ValidationContext context, string? hint = null )
        {
            Value = value;
            Parent = parent;
            Context = context;
            _hint = hint;
        }

        public ValidationContext Context { get; }
        public IValidationEntry? Parent { get; }
        public T Value { get; }

        public string EntryPath => Parent == null ? string.Empty : $"{Parent.EntryPath}:{Value!.ToString()!}";

        public ReadOnlyCollection<ValidationError> Errors => _errors.AsReadOnly();

        public void AddError( string error )
        {
            var item = new ValidationError( this, string.IsNullOrEmpty( _hint ) 
                    ? $"{EntryPath} -- {error}" 
                    : $"{EntryPath} ({_hint}) -- {error}" );

            _errors.Add( item );
        }

        public bool IsValid => Errors.Count == 0;

        public ValidationEntry<TEntry> CreateChild<TEntry>( TEntry toValidate, string? hint = null )
        {
            var retVal = new ValidationEntry<TEntry>( toValidate, this, Context, hint );
            Context.Entries.Push( retVal );

            return retVal;
        }
    }
}