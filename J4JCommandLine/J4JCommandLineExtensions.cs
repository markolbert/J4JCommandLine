using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static bool IsPublicReadWrite( this PropertyInfo propInfo, IJ4JLogger? logger )
        {
            var getAccessor = propInfo.GetGetMethod();
            var setAccessor = propInfo.GetSetMethod();

            if ( getAccessor == null )
            {
                logger?.Verbose<string>("Property {0} is not readable", propInfo.Name);
                return false;
            }

            if (setAccessor == null)
            {
                logger?.Verbose<string>("Property {0} is not writable", propInfo.Name);
                return false;
            }

            if (!getAccessor.IsPublic )
            {
                logger?.Verbose<string>("Property {0} is not publicly readable", propInfo.Name);
                return false;
            }

            if (getAccessor.IsAbstract || setAccessor.IsAbstract)
            {
                logger?.Verbose<string>("Property {0} is abstract", propInfo.Name);
                return false;
            }

            if (!setAccessor.IsPublic )
            {
                logger?.Verbose<string>("Property {0} is not publicly writable", propInfo.Name);
                return false;
            }

            if (setAccessor.IsAbstract)
            {
                logger?.Verbose<string>("Property {0} is not writable", propInfo.Name);
                return false;
            }

            return true;
        }
    }
}
