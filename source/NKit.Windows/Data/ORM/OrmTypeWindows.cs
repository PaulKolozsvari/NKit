﻿namespace NKit.Data.ORM
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection.Emit;
    using System.Reflection;
    using NKit.Data;

    #endregion //Using Directives

    /// <summary>
    /// http://olondono.blogspot.com/2008/02/creating-code-at-runtime.html
    /// </summary>
    public class OrmTypeWindows
    {
        #region Constructors

        public OrmTypeWindows(string typeName, TypeBuilder typeBuilder)
        {
            Initialize(typeName, typeBuilder);
        }

        #endregion //Constructors

        #region Fields

        protected string _typeName;
        protected TypeBuilder _typeBuilder;
        protected EntityCacheGeneric<string, OrmPropertyWindows> _properties;
        protected Type _dotNetType;

        #endregion //Fields

        #region Properties

        public string TypeName
        {
            get { return _typeName; }
        }

        public TypeBuilder TypeBuilder
        {
            get { return _typeBuilder; }
        }

        public EntityCacheGeneric<string, OrmPropertyWindows> Properties
        {
            get { return _properties; }
        }

        public Type DotNetType
        {
            get { return _dotNetType; }
        }

        #endregion //Properties

        #region Methods

        public void Clean()
        {
            _properties.ToList().ForEach(p => p.Clean());
            _properties.Clear();
            _properties = null;
            _typeName = null;
            _typeBuilder = null;
            _dotNetType = null;
        }

        protected void Initialize(string typeName, TypeBuilder typeBuilder)
        {
            if (typeName != null)
            {
                typeName = typeName.Trim();
            }
            if (string.IsNullOrEmpty(typeName))
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be null when constructing a {1}.",
                    EntityReaderGeneric<OrmTypeWindows>.GetPropertyName(p => p.TypeName, false),
                    this.GetType().FullName));
            }
            if (typeBuilder == null)
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be null when constructing a {1}.",
                    EntityReaderGeneric<OrmTypeWindows>.GetPropertyName(p => p.TypeBuilder, false),
                    this.GetType().FullName));
            }
            _typeName = typeName;
            _typeBuilder = typeBuilder;
            _properties = new EntityCacheGeneric<string, OrmPropertyWindows>();
        }

        public OrmPropertyWindows CreateOrmProperty(string propertyName, Type propertyType)
        {
            return CreateOrmProperty(propertyName, propertyType, PropertyAttributes.HasDefault);
        }

        public OrmPropertyWindows CreateOrmProperty(string propertyName, Type propertyType, PropertyAttributes propertyAttributes)
        {
            if (_properties.Exists(propertyName))
            {
                throw new ArgumentException(string.Format(
                    "{0} with {1} {2} already added to {3}.",
                    typeof(OrmPropertyWindows).FullName,
                    EntityReaderGeneric<OrmPropertyWindows>.GetPropertyName(p => p.PropertyName, false),
                    propertyName,
                    this.GetType().FullName));
            }

            OrmPropertyWindows result = new OrmPropertyWindows(propertyName, propertyAttributes, propertyType);
            FieldBuilder fieldBuilder = _typeBuilder.DefineField(result.FieldName, result.PropertyType, result.FieldAttributes);
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(result.PropertyName, result.PropertyAttributes, result.PropertyType, null);
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod(result.GetMethodName, methodAttributes, result.PropertyType, Type.EmptyTypes);
            MethodBuilder setMethodBuilder = _typeBuilder.DefineMethod(result.SetMethodName, methodAttributes, null, new Type[] { result.PropertyType });

            ILGenerator ilGenerator = getMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder); //Load field.
            ilGenerator.Emit(OpCodes.Ret); //Return the field.

            ilGenerator = setMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1); //Load argument.
            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder); //Set field.
            ilGenerator.Emit(OpCodes.Ret);

            //Set the methods on the properties.
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            _properties.Add(result.PropertyName, result);
            return result;
        }

        public Type CreateType()
        {
            _dotNetType = _typeBuilder.CreateType();
            return _dotNetType;
        }

        public override string ToString()
        {
            return _typeName;
        }

        #endregion //Methods

        #region Destructors

        //~OrmType()
        //{
        //    int i = 0;
        //}

        #endregion //Destructors
    }
}