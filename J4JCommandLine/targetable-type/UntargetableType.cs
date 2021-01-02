namespace J4JSoftware.CommandLine.Deprecated
{
    // describes a "SupportedType" which is not targetable, for any of several
    // reasons
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