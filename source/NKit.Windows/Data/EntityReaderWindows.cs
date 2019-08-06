namespace NKit.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using System.Reflection;
    using System.Data.Linq.Mapping;

    #endregion //Using Directives

    public class EntityReaderWindows : EntityReader
    {
        #region Methods

        /// <summary>
        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
        public static PropertyInfo GetLinqToSqlEntitySurrogateKey(Type entityType)
        {
            PropertyInfo[] properties = entityType.GetProperties();
            PropertyInfo surrogateKey = null;
            foreach (PropertyInfo p in properties)
            {
                object[] attributes = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                ColumnAttribute columnAttribute = attributes.Length < 1 ? null : (ColumnAttribute)attributes[0];
                if ((columnAttribute == null) || (!columnAttribute.IsPrimaryKey))
                {
                    continue;
                }
                if (surrogateKey != null)
                {
                    throw new Exception(
                        string.Format("{0} has more than one primary key. A surrogate key has to be a single field.",
                        entityType.Name));
                }
                surrogateKey = p;
            }
            if (surrogateKey == null)
            {
                throw new NullReferenceException(string.Format("{0} does not have surrogate key.", entityType.Name));
            }
            return surrogateKey;
        }

        /// <summary>
        /// Determines whether a property is an identity column.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Returns true if the property is an identity column.</returns>
        public static bool IsLinqToSqlEntityPropertyIdentityColumn(PropertyInfo p)
        {
            object[] attributes = p.GetCustomAttributes(typeof(ColumnAttribute), false);
            ColumnAttribute columnAttribute = attributes.Length < 1 ? null : (ColumnAttribute)attributes[0];
            if (columnAttribute == null)
            {
                return false;
            }
            return columnAttribute.DbType.Contains("IDENTITY");
        }

        /// <summary>
        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
        public static PropertyInfo GetLinqToSqlEntitySurrogateKey<E>()
        {
            return GetLinqToSqlEntitySurrogateKey(typeof(E));
        }

        #endregion //Methods
    }
}