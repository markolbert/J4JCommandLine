using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOptionCollection : IEnumerable<IOption>
    {
        Func<IJ4JLogger>? LoggerFactory { get; }
        CommandLineStyle CommandLineStyle { get; }
        MasterTextCollection MasterText { get; }
        ReadOnlyCollection<IOption> Options { get; }
        int Count { get; }
        bool TargetsMultipleTypes { get; }

        List<string> UnkeyedValues { get; }
        List<TokenEntry> UnknownKeys { get; }
        IOption? this[ string key ] { get; }
        void ClearValues();

        void SetTypePrefix<TTarget>( string prefix )
            where TTarget : class, new();

        string GetTypePrefix<TTarget>()
            where TTarget : class, new();

        Option<TProp>? Add<TProp>( string contextPath );
        IOption? Add( Type propType, string contextPath );

        Option<TProp>? Bind<TTarget, TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            params string[] cmdLineKeys )
            where TTarget : class, new();

        bool UsesCommandLineKey( string key );
        bool UsesContextPath( string contextPath );

        bool KeysSpecified( params string[] keys );
        void DisplayHelp( IDisplayHelp? displayHelp );
    }
}