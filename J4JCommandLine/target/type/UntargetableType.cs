namespace J4JSoftware.CommandLine
{
    public class UntargetableType : TargetableType
    {
        internal UntargetableType()
            : base( typeof(object), PropertyMultiplicity.Unsupported )
        {
            IsCreatable = false;
        }

        public override object? GetDefaultValue() => null;
    }
}