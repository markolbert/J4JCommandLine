#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'AutofacCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public static class Extensions
    {
        #region TextToValue

        public static ContainerBuilder RegisterTextToValueAssemblies(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies ) =>
            builder.RegisterTypeAssemblies<ITextToValue>(
                assemblies,
                false,
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );


        public static ContainerBuilder RegisterTextToValueAssemblies(
            this ContainerBuilder builder,
            params Type[] typesInAssemblies)
            => builder.RegisterTextToValueAssemblies(typesInAssemblies.Select(x => x.Assembly));

        #endregion

        #region AvailableTokens

        public static ContainerBuilder RegisterTokenAssemblies(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies ) =>
            builder.RegisterTypeAssemblies<IAvailableTokens>(
                assemblies,
                false,
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

        public static ContainerBuilder RegisterTokenAssemblies(
            this ContainerBuilder builder,
            params Type[] typesInAssemblies)
            => builder.RegisterTokenAssemblies(typesInAssemblies.Select(x => x.Assembly).ToList());

        #endregion

        #region MasterTextCollections

        public static ContainerBuilder RegisterMasterTextCollectionAssemblies(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies ) =>
            builder.RegisterTypeAssemblies<IMasterTextCollection>(
                assemblies,
                false,
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

        public static ContainerBuilder RegisterMasterTextCollectionAssemblies(
            this ContainerBuilder builder,
            params Type[] typesInAssemblies)
            => builder.RegisterMasterTextCollectionAssemblies(typesInAssemblies.Select(x => x.Assembly).ToList());

        #endregion

        #region BindabilityValidators

        public static ContainerBuilder RegisterBindabilityValidatorAssemblies(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies ) =>
            builder.RegisterTypeAssemblies<IBindabilityValidator>(
                assemblies,
                false,
                TypeTester.NonAbstract,
                new ConstructorTesterPermuted<IBindabilityValidator>( typeof(IJ4JLogger), typeof(IEnumerable<ITextToValue>) ) );

        public static ContainerBuilder RegisterBindabilityValidatorAssemblies(
            this ContainerBuilder builder,
            params Type[] typesInAssemblies)
            => builder.RegisterBindabilityValidatorAssemblies(typesInAssemblies.Select(x => x.Assembly).ToList());

        #endregion

        #region DisplayHelp

        public static ContainerBuilder RegisterDisplayHelpAssemblies(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies ) =>
            builder.RegisterTypeAssemblies<IDisplayHelp>(
                assemblies,
                false,
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

        public static ContainerBuilder RegisterDisplayHelpAssemblies(
            this ContainerBuilder builder,
            params Type[] typesInAssemblies)
            => builder.RegisterDisplayHelpAssemblies(typesInAssemblies.Select(x => x.Assembly).ToList());

        #endregion

        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies,
            bool registerAsSelf = false,
            params ITypeTester[] tests )
            where T : class
        {
            var assemblyList = assemblies.ToList();

            // add default
            assemblyList.Add( typeof(T).Assembly );

            var temp = builder.RegisterAssemblyTypes( assemblyList.Distinct().ToArray() )
                .Where( t => tests.All( x =>
                {
                    var retVal = x.MeetsRequirements( t );
                    return retVal;
                } ) )
                .AsImplementedInterfaces();

            if( registerAsSelf )
                temp.AsSelf();

            return builder;
        }

        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies,
            bool registerAsSelf = false,
            params PredefinedTypeTests[] typeTests )
            where T : class
        {
            var assemblyList = assemblies.ToList();

            // add default
            assemblyList.Add( typeof(T).Assembly );

            var tests = new List<ITypeTester>();

            foreach( var test in typeTests.Distinct() )
            {
                switch( test )
                {
                    case PredefinedTypeTests.ParameterlessConstructor:
                        tests.Add( new ConstructorTester<T>() );
                        break;

                    case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                        tests.Add( new ConstructorTester<T>( typeof(IJ4JLogger) ) );
                        break;

                    case PredefinedTypeTests.OnlyJ4JLoggerFactoryRequired:
                        tests.Add( new ConstructorTester<T>( typeof(IJ4JLoggerFactory) ) );
                        break;

                    case PredefinedTypeTests.NonAbstract:
                        tests.Add( TypeTester.NonAbstract );
                        break;

                    default:
                        throw new InvalidEnumArgumentException(
                            $"Unsupported {nameof(PredefinedTypeTests)} value '{test}'" );
                }
            }

            var temp = builder.RegisterAssemblyTypes( assemblyList.Distinct().ToArray() )
                .Where( t => tests.All( x => x.MeetsRequirements( t ) ) )
                .AsImplementedInterfaces();

            if( registerAsSelf )
                temp.AsSelf();

            return builder;
        }

        #region Type checks

        //private static bool TypeMeetsRequirements( Type toCheck, params Func<Type, bool>[] tests )
        //    => tests.All( x => x( toCheck ) );

        //private static bool TypeIsNonAbstract( Type toCheck ) => !toCheck.IsAbstract;

        //private static bool TypeCanBeAssignedTo<TTarget>( Type toCheck ) =>
        //    !typeof(TTarget).IsAssignableFrom( toCheck );

        //private static bool TypeCanBeAssignedTo(Type targetType, Type toCheck) =>
        //    !targetType.IsAssignableFrom(toCheck);

        //private static bool PublicConstructorHasRequiredParameters( Type toCheck, params Type[] ctorParamTypes )
        //{
        //    var ctors = toCheck.GetConstructors();
        //    if( !ctors.Any() )
        //        return false;

        //    return ctors.Any( c =>
        //    {
        //        var parameters = c.GetParameters();

        //        if( parameters.Length != ctorParamTypes.Length )
        //            return false;

        //        for( var idx = 0; idx < ctorParamTypes.Length; idx++)
        //        {
        //            if( !parameters[ idx ].ParameterType.IsAssignableFrom( ctorParamTypes[ idx ] ) )
        //                return false;
        //        }

        //        return true;
        //    } );
        //}

        #endregion
    }
}