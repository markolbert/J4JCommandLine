using System;

namespace J4JSoftware.CommandLine
{
    public class ErrorSource
    {
        private readonly StringComparison _keyComp;

        private string _key = string.Empty;

        public ErrorSource( StringComparison keyComp )
        {
            _keyComp = keyComp;
        }

        public IBindingTarget BindingTarget { get; set; }

        public string Key
        {
            get => _key;

            set
            {
                if( string.IsNullOrEmpty( value ) )
                    throw new NullReferenceException( nameof(Key) );

                _key = value;
            }
        }
    }
}