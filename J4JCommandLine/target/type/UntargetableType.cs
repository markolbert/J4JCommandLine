namespace J4JSoftware.CommandLine
{
    // describes a "SupportedType" which is not targetable, for any of several
    // reasons
    public class UntargetableType : TargetableType
    {
        internal UntargetableType()
            : base( typeof(object), Multiplicity.Unsupported )
        {
            IsCreatable = false;
        }

        public override object? GetDefaultValue() => null;
    }
}