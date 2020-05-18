using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class TextConverter
    {
        private readonly Dictionary<Type, Func<string, object?>> _converters =
            new Dictionary<Type, Func<string, object?>>();

        public TextConverter()
        {
            Case<int>( System.Convert.ToInt32 );
            Case<double>( System.Convert.ToDouble );
            Case<bool>( System.Convert.ToBoolean );
            Case<string>( x => x );
            Case<decimal>( System.Convert.ToDecimal );
        }

        public TextConverter Case<T>( Func<string, T> converter )
        {
            var type = typeof(T);

            if( _converters.ContainsKey( type ) )
                _converters[ type ] = x => converter( x );
            else _converters.Add( typeof(T), x => converter( x ) );

            return this;
        }

        public T Convert<T>( string x )
        {
            return (T) Convert( typeof(T), x );
        }

        public object Convert( Type targetType, string x )
        {
            if( !_converters.ContainsKey( targetType ) )
                return default!;

            return ( _converters[ targetType ]( x )! );
        }
    }

    public class BindingTest
    {
        private readonly StringWriter _consoleWriter = new StringWriter();
        private readonly TextConverter _textConv = new TextConverter();

        public BindingTest()
        {
            Console.SetOut( _consoleWriter );
        }

        [ Theory ]
        [ InlineData( "x", "32", "IntProperty", "-1", MappingResults.Success ) ]
        [InlineData("z", "32", "IntProperty", "-1", MappingResults.NoKeyFound, "-1")]
        [InlineData("x", "123.456", "DecProperty", "0", MappingResults.Success, "123.456")]
        public void Bind_root_properties( 
            string key, 
            string arg, 
            string propToTest, 
            string defaultValue, 
            MappingResults result, 
            string? propValue = null )
        {
            propValue ??= arg;
            propToTest.Should().NotBeNullOrEmpty();

            var context = TestServiceProvider.Instance.GetRequiredService<CommandLineContext>();

            var target = context.AddBindingTarget( new RootProperties(), "test" );

            target.TargetableProperties.Should()
                .Contain( x => string.Equals( x.PropertyInfo.Name, propToTest, StringComparison.OrdinalIgnoreCase ) );

            var boundProp = target.TargetableProperties
                .First( x => string.Equals( x.PropertyInfo.Name, propToTest, StringComparison.OrdinalIgnoreCase ) );

            var desiredValue = _textConv.Convert( boundProp.PropertyInfo.PropertyType, propValue );
            var defValue = _textConv.Convert( boundProp.PropertyInfo.PropertyType, defaultValue );

            target.BindProperty( propToTest, defValue, "x" );

            var parseResult = context.Parse( new string[] { $"-{key}", arg } );

            var consoleText = _consoleWriter.ToString();

            parseResult.Should().Be( result );

            var boundValue = boundProp!.PropertyInfo!.GetValue( target.Value );

            boundValue.Should().NotBeNull();
            boundValue.Should().Be( desiredValue );
        }
    }
}
