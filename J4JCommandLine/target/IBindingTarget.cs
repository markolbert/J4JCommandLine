﻿using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        // the properties targeted by this binding operation (i.e., ones tied to particular OptionBase objects)
        ReadOnlyCollection<TargetedProperty> TargetedProperties { get; }

        // the IOption objects created by binding properties to TValue
        IOptionCollection Options { get; }

        // Errors encountered during the binding or parsing operations
        CommandLineErrors Errors { get; }

        // Utility method for adding errors to the error collection. These are keyed by whatever
        // option key (e.g., the 'x' in '-x') is associated with the error.
        void AddError( string key, string error );

        // Parses the command line arguments against the Option objects bound to 
        // targeted properties, or to NullOption objects to collect error information.
        MappingResults Parse(string[] args);

        // allows retrieval of the TValue instance in a type-agnostic way
        object GetValue();
    }

    public interface IBindingTarget<TValue> : IBindingTarget
        where TValue : class
    {
        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        TValue Value { get; }

        // binds the selected property to a newly-created OptionBase instance. If all goes
        // well that will be an Option object capable of being a valid parsing target. If
        // something goes wrong a NullOption object will be returned. These only serve
        // to capture error information about the binding and parsing efforts.
        //
        // There are a number of reasons why a selected property may not be able to be bound
        // to an Option object. Examples: the property is not publicly read- and write-able; 
        // the property has a null value and does not have a public parameterless constructor
        // to create an instance of it. Check the error output after parsing for details.
        OptionBase Bind<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            params string[] keys );
    }
}