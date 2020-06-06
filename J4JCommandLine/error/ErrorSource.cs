using System;

namespace J4JSoftware.CommandLine
{
    public class ErrorSource
    {
        private string _key = string.Empty;

        public IBindingTarget BindingTarget { get; set; }

        public string Key
        {
            get => _key;

            set
            {
                if( string.IsNullOrEmpty( value ) )
                    _key = string.Empty;

                _key = value;
            }
        }
    }
}