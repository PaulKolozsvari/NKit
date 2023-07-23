namespace NKit.Data
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Reflection;
    using System.Collections;
    using NKit.Utilities.Serialization;
    using NKit.Data.CSV;
    using System.Diagnostics;
    using System.Data.SqlClient;
    using System.Data.Common;
    using NKit.Utilities;

    #endregion //Using Directives

    public class DataHelperWindows : DataHelper
    {
        #region Methods

        public static List<object> ParseReaderToEntities(DbDataReader reader, Type entityType, string propertyNameFilter)
        {
            propertyNameFilter = propertyNameFilter ?? string.Empty;
            List<object> result = new List<object>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    object e = Activator.CreateInstance(entityType);
                    foreach (PropertyInfo p in entityType.GetProperties())
                    {
                        if (!p.Name.Contains(propertyNameFilter) ||
                            (p.PropertyType != typeof(string) &&
                            p.PropertyType != typeof(byte) &&
                            p.PropertyType != typeof(byte[])) &&
                            (p.PropertyType.IsClass ||
                            p.PropertyType.IsEnum ||
                            p.PropertyType.IsInterface ||
                            p.PropertyType.IsNotPublic ||
                            p.PropertyType.IsPointer))
                        {
                            continue;
                        }
                        object value = null;
                        try
                        {
                            //value = reader[p.Name];
                            int columnIndex = reader.GetOrdinal(p.Name);
                            if (columnIndex < 0)
                            {
                                throw new NullReferenceException($"Could not find column {p.Name} for entity {entityType.Name} on reader.");
                            }
                            Type readerFieldType = reader.GetFieldType(columnIndex);
                            if (reader.IsDBNull(columnIndex))
                            {
                                value = null;
                            }
                            else if (readerFieldType == typeof(DateTime))
                            {
                                string dateStringValue = reader.GetString(columnIndex);
                                if (string.IsNullOrEmpty(dateStringValue))
                                {
                                    value = null;
                                }
                                value = DateTimeHelper.Parse(dateStringValue);
                            }
                            else
                            {
                                value = reader[p.Name];
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format(
                                "Could not find column {0} on {1}.",
                                p.Name,
                                reader.GetType().FullName),
                                ex);
                        }
                        p.SetValue(e, ChangeType(value, p.PropertyType), null);
                    }
                    result.Add(e);
                }
            }
            return result;
        }

        public static List<E> ParseReaderToEntities<E>(DbDataReader reader, string propertyNameFilter) where E : class
        {
            List<object> objects = ParseReaderToEntities(reader, typeof(E), propertyNameFilter);
            List<E> result = new List<E>();
            objects.ForEach(o => result.Add((E)o));
            return result;
        }

        public static DataTable GetDataTableFromEntities(List<object> entities, Type entityType)
        {
            DataTable result = EntityReader.GetDataTable(false, entityType);
            foreach (object o in entities)
            {
                DataRow row = EntityReader.PopulateDataRow(o, result.NewRow(), false, entityType);
                result.Rows.Add(row);
            }
            return result;
        }

        public static DataTable GetDataTableFromEntities<E>(List<E> entities) where E : class
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            return GetDataTableFromEntities(objects, typeof(E));
        }

        public static List<object> GetEntitiesFromDataTable(DataTable table, Type entityType)
        {
            List<object> result = new List<object>();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    object e = Activator.CreateInstance(entityType);
                    EntityReader.PopulateFromDataRow(e, row);
                    result.Add(e);
                }
            }
            return result;
        }

        public static List<E> GetEntitiesFromDataTable<E>(DataTable table) where E : class
        {
            List<object> objects = GetEntitiesFromDataTable(table, typeof(E));
            List<E> result = new List<E>();
            objects.ForEach(o => result.Add((E)o));
            return result;
        }

        public static Type GetFirstParameterTypeOfGenericType(Type type)
        {
            if (!type.IsGenericType)
            {
                return type;
            }
            return type.GetGenericArguments()[0];
        }

        public static Type GetGenericCollectionItemType(Type type)
        {
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1 &&
                    typeof(ICollection<>).MakeGenericType(args).IsAssignableFrom(type))
                {
                    return args[0];
                }
            }
            return null;
        }

        public static string GetSqlDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static void ReplaceTableNullsWithDbNulls(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row[column.ColumnName] == null)
                    {
                        row[column.ColumnName] = DBNull.Value;
                    }
                }
            }
        }

        public static List<object> ReverseListOrder(List<object> objects)
        {
            List<object> result = new List<object>();
            for (int i = objects.Count; i > -1; i--)
            {
                result.Add(objects[i]);
            }
            return result;
        }

        public static List<E> ReverseListOrder<E>(List<E> list)
        {
            List<E> result = new List<E>();
            for (int i = list.Count - 1; i > -1; i--)
            {
                result.Add(list[i]);
            }
            return result;
        }

        public static Dictionary<K,E> ReverseDictionaryOrder<K,E>(Dictionary<K,E> dictionary)
        {
            Dictionary<K, E> result = new Dictionary<K, E>();
            List<K> keys = dictionary.Keys.ToList();
            for (int i = keys.Count - 1; i > -1; i--)
            {
                K key = keys[i];
                result.Add(key, dictionary[key]);
            }
            return result;
        }

        public static DataTable FilterDataTable(DataTable inputTable, string filterText)
        {
            string filterTextLower = filterText.ToLower();
            DataTable result = inputTable.Clone();
            result.Rows.Clear();
            for (int i = 0; i < inputTable.Rows.Count; i++)
            {
                DataRow row = inputTable.Rows[i];
                bool includeRowInSearchResult = false;
                for (int j = 0; j < inputTable.Columns.Count; j++)
                {
                    object cellValue = row[j];
                    if (cellValue != null && cellValue.ToString().ToLower().Contains(filterTextLower))
                    {
                        includeRowInSearchResult = true;
                        break;
                    }
                }
                if (includeRowInSearchResult)
                {
                    result.ImportRow(row);
                }
            }
            return result;
        }

        #endregion //Methods
    }
}