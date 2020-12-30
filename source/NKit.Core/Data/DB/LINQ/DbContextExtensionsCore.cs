namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Reflection;

    #endregion //Using Directives

    public static class DbContextExtensionsCore
    {
        #region Methods

        /// <summary>
        /// Gets an IQueryable Set for a dynamic Type.
        /// https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static IQueryable Set(this DbContext context, Type T)
        {
            MethodInfo method = typeof(DbContext).GetMethods().Where(p => p.Name == "Set" && p.ContainsGenericParameters).FirstOrDefault(); // Get the generic type definition 
            method = method.MakeGenericMethod(T); // Build a method with the specific type argument you're interested in
            return method.Invoke(context, null) as IQueryable;
        }

        /// <summary>
        /// Gets an IQueryable Set for the given generic Type.
        /// https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IQueryable<T> Set<T>(this DbContext context)
        {
            MethodInfo method = typeof(DbContext).GetMethods().Where(p => p.Name == "Set" && p.ContainsGenericParameters).FirstOrDefault(); // Get the generic type definition 
            method = method.MakeGenericMethod(typeof(T)); // Build a method with the specific type argument you're interested in 
            return method.Invoke(context, null) as IQueryable<T>;
        }

        #endregion //Methods
    }
}
