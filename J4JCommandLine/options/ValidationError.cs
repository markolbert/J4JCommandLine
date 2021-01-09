namespace J4JSoftware.Configuration.CommandLine
{
    public class ValidationError
    {
        public ValidationError( IValidationEntry entry, string error )
        {
            Entry = entry;
            Error = error;
        }

        public IValidationEntry Entry { get; }
        public string Error { get; }
    }
}