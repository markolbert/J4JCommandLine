using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        OptionCollection Options { get; }
        bool IgnoreUnkeyedParameters { get; }
        bool IsConfigured { get; }
        bool HelpRequested { get; }
        CommandLineLogger Logger { get; }

        //// Utility method for adding logger to the error collection. These are keyed by whatever
        //// option key (e.g., the 'x' in '-x') is associated with the error.
        //void LogError( string? key, string error );

        bool Initialize();

        // Parses the command line arguments against the Option objects bound to 
        // targeted properties, or to NullOption objects to collect error information.
        bool Parse(string[] args);

        // allows retrieval of the TValue instance in a type-agnostic way
        object GetValue();
    }

    public interface IBindingTarget<TValue> : IBindingTarget
        where TValue : class
    {
        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        TValue Value { get; }

        // binds the selected property to a newly-created Option instance. If all goes
        // well that will be an Option object capable of being a valid parsing target. If
        // something goes wrong a NullOption object will be returned. These only serve
        // to capture error information about the binding and parsing efforts.
        //
        // There are a number of reasons why a selected property may not be able to be bound
        // to an Option object. Examples: the property is not publicly read- and write-able; 
        // the property has a null value and does not have a public parameterless constructor
        // to create an instance of it. Check the error output after parsing for details.
        Option Bind<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            params string[] keys );

        // binds the selected property to a newly-created Option instance which will enable
        // parsing of all the "non-option" text (i.e., command line parameters not associated with
        // any keyed option) to the selected property.
        //
        // If something goes wrong a NullOption object will be returned. These only serve
        // to capture error information about the binding and parsing efforts.
        //
        // There are a number of reasons why a selected property may not be able to be bound
        // to an Option object. Examples: the property is not publicly read- and write-able; 
        // the property has a null value and does not have a public parameterless constructor
        // to create an instance of it. Check the error output after parsing for details.
        Option BindUnkeyed<TProp>(Expression<Func<TValue, TProp>> propertySelector);
    }
}