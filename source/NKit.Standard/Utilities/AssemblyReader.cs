﻿namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public partial class AssemblyReader
    {
        #region Methods

        public static E CreateClassInstance<E>(Assembly assembly, string fullTypeName) where E : class
        {
            Type type = assembly.GetType(fullTypeName, false, true);
            if (type == null)
            {
                throw new NullReferenceException(
                    string.Format(
                    "Cannot find type {0} in assembly {1}.",
                    fullTypeName,
                    assembly.FullName));
            }
            object obj = Activator.CreateInstance(type);
            E result = obj as E;
            if (result == null)
            {
                throw new NullReferenceException(
                    string.Format(
                    "Type {0} in assembly {1} is not of type {2}.",
                    fullTypeName,
                    assembly.FullName,
                    typeof(E).FullName));
            }
            return result;
        }

        public static object CreateClassInstance(Assembly assembly, string fullTypeName)
        {
            Type type = assembly.GetType(fullTypeName, false, true);
            if (type == null)
            {
                throw new NullReferenceException(
                    string.Format(
                    "Cannot find type {0} in assembly {1}.",
                    fullTypeName,
                    assembly.FullName));
            }
            object result = Activator.CreateInstance(type);
            return result;
        }

        public static object CreateClassInstance(Type type)
        {
            object result = Activator.CreateInstance(type);
            return result;
        }

        public static Type FindType(
            string assemblyName, 
            string typeNamespace, 
            string typeName,
            bool throwExceptionOnError)
        {
            string executingDirectory = Information.GetExecutingDirectory();
            string assemblyFilePath = Path.Combine(executingDirectory, assemblyName);
            Assembly assembly = Assembly.LoadFrom(assemblyFilePath);
            return FindType(assembly, typeNamespace, typeName, throwExceptionOnError);
        }

        public static Type FindType(
            Assembly assembly,
            string typeNamespace,
            string typeName,
            bool throwExceptionOnError)
        {
            string assemblyName = assembly.GetName().CodeBase.Remove(0, 6);
            string fullTypeName = string.Format("{0}.{1}", typeNamespace, typeName);
            Type result = assembly.GetType(fullTypeName, false, true);
            if (result == null && throwExceptionOnError)
            {
                throw new NullReferenceException(string.Format("Could not find type {0} in assembly {1}.", fullTypeName, assemblyName));
            }
            return result;
        }

        /// <summary>
        /// Checks the entry assembly and collects all referenced projects' output XML document file paths.
        /// </summary>
        /// <returns>Returns all referenced projects' output XML document file paths.</returns>
        public static List<string> GetAllXMlDocumenationFilePaths()
        {
            //Collect all referenced projects output XML document file paths  
            Assembly currentAssembly = Assembly.GetEntryAssembly();
            List<string> result = currentAssembly.GetReferencedAssemblies()
            .Union(new AssemblyName[] { currentAssembly.GetName() })
            .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
            .Where(f => File.Exists(f)).ToList();
            return result;
        }

        #endregion //Methods
    }
}
