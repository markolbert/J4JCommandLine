using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class BindingTest
    {
        private readonly StringWriter _consoleWriter = new StringWriter();

        public BindingTest()
        {
            Console.SetOut( _consoleWriter );
        }

        [ Theory ]
        [ InlineData( "x", "32", "intproperty", -1, MappingResults.Success, 32 ) ]
        [InlineData("z", "32", "intproperty", -1, MappingResults.NoKeyFound, -1)]
        [InlineData("x", "123.456", "decproperty", 0, MappingResults.Success, 123.456)]
        public void Bind_root_properties( 
            string key, 
            string arg, 
            string propToTest, 
            object defaultValue, 
            MappingResults result, 
            object value )
        {
            var context = TestServiceProvider.Instance.GetRequiredService<CommandLineContext>();

            var target = context.AddBindingTarget( new RootProperties(), "test" );

            propToTest = propToTest.ToLower();

            switch( propToTest )
            {
                case "intproperty":
                    target.BindProperty( x => x.IntProperty, (int) defaultValue, "x" );
                    break;

                case "boolproperty":
                    target.BindProperty( x => x.BoolProperty, (bool) defaultValue, "x" );
                    break;

                case "textproperty":
                    target.BindProperty( x => x.TextProperty, (string) defaultValue, "x" );
                    break;

                case "decproperty":
                    var decDefault = Convert.ToDecimal( defaultValue );
                    target.BindProperty( x => x.DecimalProperty, decDefault, "x" );
                    break;
            }

            var parseResult = context.Parse( new string[] { $"-{key}", arg } );

            var consoleText = _consoleWriter.ToString();

            parseResult.Should().Be( result );

            switch( propToTest )
            {
                case "intproperty":
                    target.Value.IntProperty.Should().Be( (int) value );
                    break;

                case "boolproperty":
                    target.Value.BoolProperty.Should().Be( (bool) value );
                    break;

                case "textproperty":
                    target.Value.TextProperty.Should().Be( (string) value );
                    break;

                case "decproperty":
                    var decValue = Convert.ToDecimal( value );
                    target.Value.DecimalProperty.Should().Be( decValue );
                    break;
            }
        }
    }
}
