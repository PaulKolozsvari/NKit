namespace NKit.Core.Data.DB.LINQ
{
    #region Using Directives

    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Reflection;

    #endregion //Using Directives

    public static class EntityFrameworkExtensionsCore
    {
        #region Methods

        /// <summary>
        /// Gets an IQueryable Set for a dynamic Type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static IQueryable Set(this DbContext context, Type T)
        {
            // Get the generic type definition
            MethodInfo method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance);
            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(T);
            return method.Invoke(context, null) as IQueryable;
        }

        /// <summary>
        /// Gets an IQueryable Set for the given generic Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IQueryable<T> Set<T>(this DbContext context)
        {
            // Get the generic type definition 
            MethodInfo method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance);
            // Build a method with the specific type argument you're interested in 
            method = method.MakeGenericMethod(typeof(T));
            return method.Invoke(context, null) as IQueryable<T>;
        }

        #endregion //Methods
    }
}
