using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    // describes a user-selected property intended to be a target of a command line option
    // the targeted property may or may not actually be targetable unless it meets
    // the following conditions:
    // - is pre-defined (has a value when bound to)
    // - has a public parameterless constructor (so if it's not defined when bound a value can be created)
    // - is publicly readable and writeable
    // - can be converted via a known ITextConverter
    public class TargetedProperty
    {
        private readonly StringComparison _keyComp;
        private readonly CommandLineLogger _logger;

        private bool _preAssigned;

        public TargetedProperty( 
            PropertyInfo propertyInfo, 
            object? rootContainer,
            TargetedProperty? parent,
            ITargetableTypeFactory targetableTypeFactory, 
            StringComparison keyComp,
            CommandLineLogger logger )
        {
            _keyComp = keyComp;
            _logger = logger;

            PropertyInfo = propertyInfo;
            Parent = parent;
            TargetableType = targetableTypeFactory.Create( PropertyInfo.PropertyType );
            IsPubliclyReadWrite = PropertyInfo.IsPublicReadWrite();

            var targetContainer = GetContainer( rootContainer );

            IsPreAssigned = PropertyInfo.PropertyType.IsValueType
                            || ( targetContainer != null && PropertyInfo.GetValue( targetContainer ) != null );
        }

        // the TargetedProperty which owns this TargetedProperty, or null if
        // this TargetedProperty is owned by the object being bound to by the framework.
        public TargetedProperty? Parent { get; }

        // the list of TargetedProperty values describing the "property path" from the
        // object being bound by the framework to this TargetedProperty instance
        public List<TargetedProperty> PropertyPath
        {
            get
            {
                if( Parent == null )
                    return new List<TargetedProperty> { this };

                var retVal = Parent.PropertyPath;
                retVal.Add( this );

                return retVal;
            }
        }

        // the PropertyInfo information for the property described by this TargetedProperty
        public PropertyInfo PropertyInfo { get; }

        // information describing whether or not the Type of the property described by this
        // TargetedProperty. This includes information about whether the Type is creatable
        public ITargetableType TargetableType { get; }

        public string Name => PropertyInfo.Name;

        // a property is only creatable if every parent property back to the 
        // root container is also creatable. That's because if a property needs to
        // be created all of its predecessor owners must be creatable as well.
        public bool IsCreateable
        {
            get
            {
                if( Parent == null )
                    return TargetableType.IsCreatable;

                return TargetableType.IsCreatable && Parent.IsCreateable;
            }
        }

        // a property is only preassigned if every parent property back to the
        // root container is also preassigned. This means that the property described
        // by this TargetedProperty instance can be set without having to create
        // instances of any parent/owner property.
        public bool IsPreAssigned
        {
            get
            {
                if( Parent == null )
                    return _preAssigned;

                return _preAssigned && Parent.IsPreAssigned;
            }

            set => _preAssigned = value;
        }

        public bool IsPubliclyReadWrite { get; private set; }
        public PropertyMultiplicity Multiplicity => TargetableType.Multiplicity;

        // The IOption bound to this TargetedProperty. This should never be null for 
        // a configured TargetedProperty but assignment of the Option being bound to is
        // done after the TargetedProperty instance is created so it has to be nullable.
        //
        // An unsupported TargetedProperty (i.e., one which is not targetable) will always
        // be bound to an instance of NullOption, which is used to capture error information
        // during parsing.
        public Option? BoundOption { get; set; }

        // an informational value containing the names of all the properties between the root,
        // bound object and this TargetedProperty (including this TargetedProperty's name itself).
        public string FullPath => Parent == null ? PropertyInfo.Name : $"{Parent.FullPath}.{Name}";

        public bool IsUpdateable()
        {
            // validate parameters and state
            if (BoundOption == null)
            {
                _logger.LogError(ProcessingPhase.Parsing, $"Property '{PropertyInfo.Name}' is unbound");
                return false;
            }

            if (Multiplicity == PropertyMultiplicity.Unsupported)
            {
                _logger.LogError(
                    ProcessingPhase.Parsing,
                    $"Property '{PropertyInfo.Name}' has an unsupported Multiplicity",
                    option: BoundOption);

                return false;
            }

            if (!IsPreAssigned && !IsCreateable)
            {
                _logger.LogError(
                    ProcessingPhase.Parsing,
                    $"Property '{PropertyInfo.Name}' was not pre-assigned and is not creatable",
                    option: BoundOption);

                return false;
            }

            if (!IsPubliclyReadWrite)
            {
                _logger.LogError(
                    ProcessingPhase.Parsing,
                    $"Property '{PropertyInfo.Name}' is not publicly readable/writeable",
                    option: BoundOption);

                return false;
            }

            return true;
        }

        public bool GetDefaultValue( out object? result )
        {
            result = null;

            if( !IsUpdateable() )
                return false;

            result = BoundOption!.DefaultValue;

            return true;
        }

        // maps the information contained in allocations to the Option bound to this TargetedProperty by matching
        // keys. Performs various checks to ensure the conversion and binding process is valid, storing
        // logger when it's not.
        public bool GetValueFromAllocation( IAllocation allocation, out object? result )
        {
            result = null;

            if( !IsUpdateable() || !GetDefaultValue( out var defaultValue ) )
                return false;

            result = BoundOption!.Convert( allocation, TargetableType );

            if( result != null && BoundOption.Validate( result ) ) 
                return true;
            
            // revert to our default value (which we presume is valid but don't actually know
            // or care)
            result = defaultValue;

            return false;
        }

        public void SetValue( IBindingTarget bindingTarget, object? value )
        {
            var container = GetContainer( bindingTarget.GetValue() );

            PropertyInfo.SetValue( container, value );
        }

        private object? GetContainer(object? container)
        {
            if (container == null)
                return null;

            object? retVal = container;

            // walk through the parent containers grabbing values,
            // creating the ones that are undefined
            foreach (var targeted in PropertyPath.SkipLast(1))
            {
                var newContainer = targeted.PropertyInfo.GetValue(retVal);

                // if the property doesn't have a value, create it
                // if possible
                if (newContainer == null)
                {
                    newContainer = targeted.TargetableType.GetDefaultValue();

                    targeted.PropertyInfo.SetValue(retVal, newContainer);
                }

                retVal = newContainer;

                if (retVal == null)
                    return null;
            }

            return retVal;
        }
    }
}