namespace NKit.Data.ORM
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Reflection;
    using System.Text;
    using System.Data.Entity.Migrations.Model;
    using NKit.Utilities.Logging;
    using NKit.Utilities;

    #endregion //Using Directives

    public class OrmAssemblyCore
    {
        #region Constructors

        public OrmAssemblyCore(string assemblyName, AssemblyBuilderAccess assemblyBuilderAccess)
        {
            Initialize(assemblyName, null, assemblyBuilderAccess);
        }

        public OrmAssemblyCore(string assemblyName, string assemblyFileName, AssemblyBuilderAccess assemblyBuilderAccess)
        {
            Initialize(assemblyName, assemblyFileName, assemblyBuilderAccess);
        }

        #endregion //Constructors

        #region Fields

        protected string _assemblyName;
        protected string _assemblyFileName;
        protected string _assemblyFilePath;
        protected AssemblyBuilderAccess _assemblyBuilderAccess;
        protected EntityCacheGeneric<string, OrmTypeCore> _ormTypes;
        protected AssemblyBuilder _assemblyBuilder;
        protected ModuleBuilder _moduleBuilder;

        #endregion //Fields

        #region Properties

        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        public string AssemblyFileName
        {
            get { return _assemblyFileName; }
        }

        public string AssemblyFilePath
        {
            get { return _assemblyFilePath; }
        }

        public AssemblyBuilderAccess AssemblyBuilderAccess
        {
            get { return _assemblyBuilderAccess; }
        }

        public AssemblyBuilder AssemblyBuilder
        {
            get { return _assemblyBuilder; }
        }

        public ModuleBuilder ModuleBuilder
        {
            get { return _moduleBuilder; }
        }

        public EntityCacheGeneric<string, OrmTypeCore> OrmTypes
        {
            get { return _ormTypes; }
        }

        #endregion //Properties

        #region Methods

        public void Clean()
        {
            _ormTypes.ToList().ForEach(p => p.Clean());
            _ormTypes.Clear();
            _ormTypes = null;
            _moduleBuilder = null;
            _assemblyBuilder = null;
        }

        protected void Initialize(
            string assemblyName,
            string assemblyFileName,
            AssemblyBuilderAccess assemblyBuilderAccess)
        {
            if (assemblyName != null)
            {
                assemblyName = assemblyName.Trim();
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be null when constructing a {1}.",
                    EntityReaderGeneric<OrmAssemblyCore>.GetPropertyName(p => p.AssemblyName, false),
                    this.GetType().FullName));
            }
            _assemblyName = assemblyName;
            _assemblyFileName = assemblyFileName;
            _assemblyBuilderAccess = assemblyBuilderAccess;
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_assemblyName), _assemblyBuilderAccess);
            string domainName = AppDomain.CurrentDomain.FriendlyName;
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName);
            //_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName, _assemblyFileName); //Supplying the assembly file name is not supported on .NET Core, only on .NET Framework.
            _ormTypes = new EntityCacheGeneric<string, OrmTypeCore>();
        }

        public OrmTypeCore CreateOrmType(string typeName, bool prefixWithAssemblyNamespace)
        {
            if (_ormTypes.Exists(typeName))
            {
                throw new ArgumentException(string.Format(
                    "{0} with {1} {2} already created on {3}.",
                    typeof(OrmTypeCore).FullName,
                    EntityReaderGeneric<OrmTypeCore>.GetPropertyName(p => p.TypeName, false),
                    typeName,
                    this.GetType().FullName));
            }
            if (prefixWithAssemblyNamespace)
            {
                typeName = string.Format("{0}.{1}", _assemblyName, typeName);
            }
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public);
            ConstructorBuilder constructorBuilder = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            OrmTypeCore result = new OrmTypeCore(typeName, typeBuilder);
            _ormTypes.Add(result.TypeName, result);
            return result;
        }

        /// <summary>
        /// Gets the default assembly file path which is the assembly file name in the current executing directory.
        /// </summary>
        /// <returns></returns>
        private string GetAssemblyFilePathToExecutingDirectory()
        {
            return Path.Combine(Information.GetExecutingDirectory(), _assemblyFileName);
        }

        private void DeleteAssemblyFromCurrentDirectory()
        {
            if (File.Exists(_assemblyFileName))
            {
                File.Delete(_assemblyFileName);
            }
        }

        /// <summary>
        /// Saving dynamic assemblies is not supported on .NET Core. It's only supported on .NET Framework: https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.save?view=netframework-4.8.1
        /// Hence this method will throw a NotSupportedException.
        /// 
        /// We can only save the assembly in the current directory when we call save i.e. we're not allowed to set a path.
        /// However, the current directory may not always be equal to current directory. 
        /// If that's the case we need to save it to the current directory, then copy it to the executing directory and thereafter delete it from the current directory.
        /// </summary>
        private void SaveOrmAssemblyToExecutingDirectory()
        {
            throw new NotSupportedException("Saving dynamic assemblies is not supported on .NET Core. It's only supported on .NET Framework.");

            //DeleteAssemblyFromCurrentDirectory(); //Delete the old assembly from the current directory.
            //_assemblyBuilder.Save(_assemblyFileName); //Saves it to the current directory.
            //if (Environment.CurrentDirectory == Information.GetExecutingDirectory())
            //{
            //    return; //If the current and executing directories are the same, then we can leave the assembly where it is.
            //}
            //string executingDirectoryAssemblyFilePath = GetAssemblyFilePathToExecutingDirectory(); //Path to the executing directory where we need to actually save it to.
            //if (File.Exists(executingDirectoryAssemblyFilePath))
            //{
            //    File.Delete(executingDirectoryAssemblyFilePath); //Delete the old assembly from the executing directory.
            //}
            //File.Copy(_assemblyFileName, executingDirectoryAssemblyFilePath); //Copy from the current directory to the executing directory.
            //DeleteAssemblyFromCurrentDirectory(); //Delete the assembly from the current directory.
            //_assemblyFilePath = executingDirectoryAssemblyFilePath;
        }

        /// <summary>
        /// Saving dynamic assemblies is not supported on .NET Core. It's only supported on .NET Framework: https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.save?view=netframework-4.8.1
        /// Hence this method will throw a NotSupportedException.
        /// 
        /// Saves the assembly in the executing directory and then copies it to the specified output directory if the output directory is different from the executing directory.
        /// Throws an exception of the specified output directory does not exist.
        /// </summary>
        public void Save(string ormAssemblyOutputDirectory)
        {
            throw new NotSupportedException("Saving dynamic assemblies is not supported on .NET Core. It's only supported on .NET Framework.");

            //SaveOrmAssemblyToExecutingDirectory();
            //if (string.IsNullOrEmpty(ormAssemblyOutputDirectory) || (ormAssemblyOutputDirectory.ToLower().Trim() == Information.GetExecutingDirectory().ToLower().Trim()))
            //{
            //    return; //Output directory not specified or the specified output directory is in fact the executing directory.
            //}
            //if (!Directory.Exists(ormAssemblyOutputDirectory))
            //{
            //    throw new UserThrownException(
            //        string.Format("Could not find ORM output directory {0}.", ormAssemblyOutputDirectory),
            //        LoggingLevel.Minimum);
            //}
            //string newOutputAssemblyFilePath = Path.Combine(ormAssemblyOutputDirectory, _assemblyFileName);
            //if (File.Exists(newOutputAssemblyFilePath))
            //{
            //    File.Delete(newOutputAssemblyFilePath); //Delete the old assembly from the output directory.
            //}
            //File.Copy(_assemblyFilePath, newOutputAssemblyFilePath); //Copy from the executing directory to the output directory specified by the user in settings file.
            //_assemblyFilePath = newOutputAssemblyFilePath;
        }

        /// <summary>
        /// Gets the assembly file name.
        /// </summary>
        public override string ToString()
        {
            return _assemblyName;
        }

        #endregion //Methods

        #region Destructors

        //~OrmAssembly()
        //{
        //    int test = 0;
        //}

        #endregion //Destructors
    }
}
