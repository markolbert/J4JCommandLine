﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

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

        private bool _preAssigned;

        public TargetedProperty( 
            PropertyInfo propertyInfo, 
            object? rootContainer,
            TargetedProperty? parent,
            ITargetableTypeFactory targetableTypeFactory, 
            StringComparison keyComp,
            IJ4JLogger? logger)
        {
            _keyComp = keyComp;

            PropertyInfo = propertyInfo;
            Parent = parent;
            TargetableType = targetableTypeFactory.Create( PropertyInfo.PropertyType );
            IsPubliclyReadWrite = PropertyInfo.IsPublicReadWrite( logger );

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
        public Multiplicity Multiplicity => TargetableType.Multiplicity;

        public bool IsTargetable => ( IsCreateable || IsPreAssigned )
                                    && IsPubliclyReadWrite
                                    && Multiplicity != Multiplicity.Unsupported;

        // The IOption bound to this TargetedProperty. This should never be null for 
        // a configured TargetedProperty but assignment of the Option being bound to is
        // done after the TargetedProperty instance is created so it has to be nullable.
        //
        // An unsupported TargetedProperty (i.e., one which is not targetable) will always
        // be bound to an instance of NullOption, which is used to capture error information
        // during parsing.
        public IOption? BoundOption { get; set; }

        // When getting or setting the value of the property described by this TargetedProperty
        // you have to have access to the container object which owns the property. GetContainer()
        // retrieves this by starting at the root, bound object and walking the TargetedProperty tree.
        public object? GetContainer( IBindingTarget bindingTarget ) => GetContainer( bindingTarget.GetValue() );

        protected object? GetContainer( object? container )
        {
            if( container == null )
                return null;

            object? retVal = container;

            // walk through the parent containers grabbing values,
            // creating the ones that are undefined
            foreach (var targeted in PropertyPath.SkipLast(1))
            {
                var newContainer = targeted.PropertyInfo.GetValue(retVal);

                // if the property doesn't have a value, create it
                // if possible
                if( newContainer == null )
                {
                    newContainer = targeted.TargetableType.GetDefaultValue();

                    targeted.PropertyInfo.SetValue( retVal, newContainer );
                }

                retVal = newContainer;

                if (retVal == null)
                    return null;
            }

            return retVal;
        }

        // an informational value containing the names of all the properties between the root,
        // bound object and this TargetedProperty (including this TargetedProperty's name itself).
        public string FullPath => Parent == null ? PropertyInfo.Name : $"{Parent.FullPath}.{Name}";

        // maps the parsed information contained in parsedResults to Option bound to this TargetedProperty by matching
        // keys. Performs various checks to ensure the conversion and binding process is valid, storing
        // errors when it's not.
        public MappingResults MapParseResult(
            IBindingTarget bindingTarget,
            ParseResults parseResults,
            IJ4JLogger? logger = null )
        {
            // validate parameters and state
            if( BoundOption == null )
            {
                logger?.Error<string>( "Trying to map parsing results to unbound property {0}", PropertyInfo.Name );
                bindingTarget.AddError( "?", $"Property '{PropertyInfo.Name}' is unbound" );

                return MappingResults.Unbound;
            }

            // see if our BoundOption's keys match a key in the parse results so we can retrieve a
            // specific IParseResult
            var parseResult = parseResults
                .FirstOrDefault( pr =>
                    BoundOption.Keys.Any( k => string.Equals( k, pr.Key, _keyComp ) ) );

            // store the option key that we matched on for later use in displaying context-sensitive help
            var optionKey = parseResult == null ? BoundOption.Keys.First() : parseResult.Key;

            if( Multiplicity == Multiplicity.Unsupported )
            {
                logger?.Error<string>( "Property {0} has an unsupported Multiplicity", PropertyInfo.Name );
                bindingTarget.AddError( optionKey, $"Property '{PropertyInfo.Name}' has an unsupported Multiplicity" );

                return MappingResults.UnsupportedMultiplicity;
            }

            if( !IsPreAssigned && !IsCreateable )
            {
                logger?.Error<string>( "Property {0} was not pre-assigned and is not creatable", PropertyInfo.Name );
                bindingTarget.AddError( optionKey,
                    $"Property '{PropertyInfo.Name}' was not pre-assigned and is not creatable" );

                return MappingResults.NotDefinedOrCreatable;
            }

            if( !IsPubliclyReadWrite )
            {
                logger?.Error<string>( "Property {0} is not publicly readable/writeable", PropertyInfo.Name );
                bindingTarget.AddError( optionKey,
                    $"Property '{PropertyInfo.Name}' is not publicly readable/writeable" );

                return MappingResults.NotPublicReadWrite;
            }

            var retVal = MappingResults.Success;

            // start by setting the value we're going to set on our bound property to 
            // whatever default was specified for our BoundOption.
            var propValue = BoundOption.DefaultValue;

            if( parseResult == null )
            {
                // set a return flag if there's no matching IParseResult
                logger?.Error<string>( "No matching argument keys for property {0}", PropertyInfo.Name );

                // if the option isn't required we'll just use the previously-determined default value
                if( BoundOption.IsRequired )
                {
                    bindingTarget.AddError( optionKey, $"Missing required option '{optionKey}'" );
                    retVal |= MappingResults.MissingRequired;
                }
            }
            else
            {
                retVal = BoundOption.Convert( bindingTarget, parseResult, TargetableType, out var convResult );

                if( retVal == MappingResults.Success )
                    propValue = convResult;
            }

            if( propValue != null && !BoundOption.Validate( bindingTarget, optionKey, propValue ) )
            {
                // revert to our default value (which we presume is valid but don't actually know
                // or care)
                propValue = BoundOption.DefaultValue;

                // set a flag to record the validation failure
                retVal |= MappingResults.ValidationFailed;
            }

            // finally, set the target property's value
            PropertyInfo.SetValue( GetContainer( bindingTarget ), propValue );

            return retVal;
        }
    }
}