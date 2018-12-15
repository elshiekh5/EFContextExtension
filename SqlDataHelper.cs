using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;


namespace EFContextExtension
{
  
    public class SqlDataHelper
    {



        #region GetColumnsName
        public StringDictionary GetColumnsSchema(IDataReader reader)
        {
            StringDictionary columnsNames = new StringDictionary();
            DataTable dt = reader.GetSchemaTable();
            //---------------------------------
            foreach (DataColumn c in dt.Columns)
            {
                columnsNames.Add(c.ColumnName, null);
            }
            //---------------------------------
            return columnsNames;
        }
        #endregion

        #region --------------GetSqlConnection--------------
        public SqlConnection GetSqlConnection()
        {
            //return new SqlConnection(ConfigurationManager.ConnectionStrings["Connectionstring"].ToString());
            return new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());

        }
        //------------------------------------------
        #endregion

        #region --------------GetEntity--------------
        //---------------------------------------------------------------------
        //GetEntity
        //---------------------------------------------------------------------
        /// <summary>
        /// conver datareader object to an entity object
        /// </summary>
        /// <param name="reader">data reader </param>
        /// <param name="t">type of object we need to convert to</param>
        /// <returns></returns>
        public T GetEntity<T>(IDataReader reader)
        {
            Type t = typeof(T);
            return (T)GetEntity(reader, t);
        }
        //---------------------------------------------------------------------
        public object GetEntity(IDataReader reader, Type t)
        {

            object obj = Activator.CreateInstance(t);
            //object obj = new t();
            StringDictionary columnsNames = new StringDictionary();
            DataTable dt = reader.GetSchemaTable();
            Type nullableType;
            object value;
            object safeValue;
            //---------------------------------
            string columnname;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnname = reader.GetName(i);
                if (!columnsNames.ContainsKey(columnname))
                {
                    columnsNames.Add(columnname, null);
                    PropertyInfo myPropInfo;
                    myPropInfo = t.GetProperty(columnname);
                    value = reader[columnname];

                    if (value != DBNull.Value && myPropInfo != null)
                    {
                        //myPropInfo.SetValue(obj, Convert.ChangeType(value, myPropInfo.PropertyType), null);

                        if (myPropInfo.PropertyType.BaseType == typeof(System.Enum))
                        {
                            //int intVal = Convert.ToInt32(attr.Value);
                            myPropInfo.SetValue(obj, Enum.Parse(myPropInfo.PropertyType, value.ToString()), null);
                            //Enum.Parse(typeof(myPropInfo.), "FirstName");   
                        }
                        /*
                        else if (value.GetType() == typeof(Byte[]))
                        {
                            byte[] buf = (byte[])value;
                            myPropInfo.SetValue(obj, Convert.ChangeType(OurSerializer.Deserialize(buf), myPropInfo.PropertyType), null);
                        }
                        */
                        else if (Nullable.GetUnderlyingType(myPropInfo.PropertyType) != null)
                        {
                            nullableType = Nullable.GetUnderlyingType(myPropInfo.PropertyType) ?? myPropInfo.PropertyType;
                            safeValue = (value == null) ? null : Convert.ChangeType(value, nullableType);
                            myPropInfo.SetValue(obj, safeValue);

                        }
                        else
                        {
                            myPropInfo.SetValue(obj, Convert.ChangeType(value, myPropInfo.PropertyType), null);
                        }
                    }
                }
            }
            //---------------------------------
            return obj;
        }

        private object GetDynamic(IDataReader reader)
        {
            return null;
        }
        #endregion

        #region --------------ExecuteStoredProcedure--------------
        public int ExecuteStoredProcedure(string procedureName, CustomValuesAsSqlParameterList customParameters)
        {
            return ExecuteStoredProcedure(procedureName, customParameters.Parameters);
        }
        public int ExecuteStoredProcedure(string procedureName, CustomSqlParameterList customParameters)
        {
            return ExecuteStoredProcedure(procedureName, customParameters.Parameters);

        }
        public int ExecuteStoredProcedure(string procedureName, List<SqlParameter> parameters)
        {
            int resultCount = 0;
            using (SqlConnection myConnection = this.GetSqlConnection())
            {

                SqlCommand myCommand = new SqlCommand(procedureName, myConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                // Set the parameters
                foreach (SqlParameter p in parameters)
                {
                    myCommand.Parameters.Add(p);
                }
                //---------------------------------------------------------------------
                // Execute the command
                myConnection.Open();
                resultCount = myCommand.ExecuteNonQuery();
                myConnection.Close();
                //----------------------------------------------------------------
                return resultCount;
            }
        }
        #endregion

        #region --------------RetrieveDataFromStordProcedure--------------
        public List<T> RetrieveDataFromStordProcedure<T>(string procedureName, CustomValuesAsSqlParameterList customParameters)
        {
            return RetrieveDataFromStordProcedure<T>(procedureName, customParameters.Parameters);
        }
        public List<T> RetrieveDataFromStordProcedure<T>(string procedureName, CustomSqlParameterList parameters)
        {
            return RetrieveDataFromStordProcedure<T>(procedureName, parameters.Parameters);
        }
        public List<T> RetrieveDataFromStordProcedure<T>(string procedureName, List<SqlParameter> parameters)
        {
            List<T> itemsList = new List<T>();
            using (SqlConnection myConnection = this.GetSqlConnection())
            {

                SqlCommand myCommand = new SqlCommand(procedureName, myConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                // Set the parameters
                foreach (SqlParameter p in parameters)
                {
                    myCommand.Parameters.Add(p);
                }

                // Execute the command
                SqlDataReader dr;
                myConnection.Open();
                dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    var item = this.GetEntity<T>(dr);
                    if (item != null)
                    {
                        itemsList.Add(item);
                    }
                }
                dr.Close();
                myConnection.Close();
                //----------------------------------------------------------------
                return itemsList;
            }
        }
        #endregion

        #region --------------RetrieveMultiRecordSet--------------
        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, CustomValuesAsSqlParameterList customParameters, params Type[] types)
        {
            return RetrieveMultiRecordSet(procedureName, customParameters.Parameters, types);
        }
        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, CustomSqlParameterList parameters, params Type[] types)
        {
            return RetrieveMultiRecordSet(procedureName, parameters.Parameters, types);
        }
        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, List<SqlParameter> parameters, params Type[] types)
        {
            var recordSetDefinitions = GenerateRecordSetDefinition(types);
            return RetrieveMultiRecordSet(procedureName, parameters, recordSetDefinitions);

        }
        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, List<SqlParameter> parameters, Type[] types, List<string> names)
        {
            var rsDefinitionManager = new RecordSetDefinitionManager();
            var recordSetDefinitions = rsDefinitionManager.GenerateRecordSetDefinition(types, names);
            return RetrieveMultiRecordSet(procedureName, parameters, recordSetDefinitions);
        }
        #region oldCode of RetrieveMultiRecordSet
        /*
        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, List<SqlParameter> parameters, List<Type> types)
        {
            Dictionary<string, object> resultSet = new Dictionary<string, object>();
            using (SqlConnection myConnection = this.GetSqlConnection())
            {

                SqlCommand myCommand = new SqlCommand(procedureName, myConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                // Set the parameters
                foreach (SqlParameter p in parameters)
                {
                    myCommand.Parameters.Add(p);
                }
                //---------------------------------
                // Execute the command
                int index = 0;
                Type itemType = null;
                SqlDataReader dr;
                myConnection.Open();
                dr = myCommand.ExecuteReader();
                foreach (var t in types)
                {

                    var listType = typeof(List<>);
                    var constructedListType = listType.MakeGenericType(t);

                    var itemsList = (IList) Activator.CreateInstance(constructedListType);

                    if (index++ > 0) { dr.NextResult(); }

                    while (dr.Read())
                    {
                        var item = this.GetEntity(dr,t);
                        if (item != null)
                        {
                            itemsList.Add(item);
                        }
                    }
                    resultSet.Add(t.Name, itemsList);


                }
                dr.Close();
                myConnection.Close();
                //----------------------------------------------------------------
                return resultSet;
            }
        }
        */
        #endregion

        public Dictionary<string, object> RetrieveMultiRecordSet(string procedureName, List<SqlParameter> parameters, List<RecordSetDefinition> tepesDefinition)
        {
            Dictionary<string, object> resultSet = new Dictionary<string, object>();
            using (SqlConnection myConnection = this.GetSqlConnection())
            {

                SqlCommand myCommand = new SqlCommand(procedureName, myConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                // Set the parameters
                foreach (SqlParameter p in parameters)
                {
                    myCommand.Parameters.Add(p);
                }
                //---------------------------------
                // Execute the command
                int index = 0;
                SqlDataReader dr;
                myConnection.Open();
                dr = myCommand.ExecuteReader();
                foreach (var t in tepesDefinition)
                {


                    if (index > 0) { dr.NextResult(); }
                    if (t.IsGenericType)
                    {
                        IList itemsList = GetListOfDataFromDataReader(dr, t);
                        resultSet.Add(t.Name, itemsList);
                    }
                    else
                    {
                        var item = GetSingleobjectFromDataReader(dr, t);
                        resultSet.Add(t.Name, item);
                    }
                    ++index;
                }
                dr.Close();
                myConnection.Close();
                //----------------------------------------------------------------
                return resultSet;
            }
        }

        #endregion
        private List<RecordSetDefinition> GenerateRecordSetDefinition(Type[] types)
        {
            var rsDefinitionManager = new RecordSetDefinitionManager();
            return rsDefinitionManager.GenerateRecordSetDefinition(types, null);

        }

        private IList GetListOfDataFromDataReader(IDataReader dr, RecordSetDefinition t)
        {

            var itemsList = (IList)Activator.CreateInstance(t.Type);
            while (dr.Read())
            {
                var item = this.GetEntity(dr, t.GenericObjectType);
                if (item != null)
                {
                    itemsList.Add(item);
                }
            }
            return itemsList;
        }

        private object GetSingleobjectFromDataReader(IDataReader dr, RecordSetDefinition t)
        {
            object item = null;
            while (dr.Read())
            {
                item = this.GetEntity(dr, t.Type);
                break;
            }
            return item;

        }

        public bool Createko(Type myType, object obj, string tableName)
        {
            //Type myType = typeof(t);
            PropertyInfo[] piT = myType.GetProperties();
            object PropValue;
            DCAttributes[] dcattr;
            DCNonInsertable[] dcNonInsertable;
            //object defaultValue;
            /*string strQry =
                    " Count(*) FROM Users WHERE UserName=@username " +
                    "AND Password=@password";*/

            using (SqlConnection myConnection = this.GetSqlConnection())
            {
                string columns = "";
                string parameters = "";
                SqlParameter prm;

                SqlCommand myCommand = new SqlCommand("", myConnection);
                myCommand.CommandType = CommandType.Text;
                // Set the parameters
                myCommand.Parameters.Add("@DCID", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                foreach (PropertyInfo myPropInfo in piT)
                {
                    if (myPropInfo.CanWrite)
                    {
                        PropValue = myPropInfo.GetValue(obj, null);
                        dcattr = (DCAttributes[])myPropInfo.GetCustomAttributes(typeof(DCAttributes), false);
                        dcNonInsertable = (DCNonInsertable[])myPropInfo.GetCustomAttributes(typeof(DCNonInsertable), false);
                        if (dcNonInsertable.Length == 0)
                        {
                            if (columns.Length > 0)
                            {
                                columns += ",";
                                parameters += ",";
                            }
                            columns += "[" + myPropInfo.Name + "]";
                            parameters += "@" + myPropInfo.Name;

                            if (dcattr.Length > 0)
                            {
                                prm = new SqlParameter("@" + myPropInfo.Name, dcattr[0].PropType, dcattr[0].PropLength);
                                prm.Value = PropValue;
                            }
                            else
                            {
                                prm = new SqlParameter("@" + myPropInfo.Name, PropValue);

                            }
                            myCommand.Parameters.Add(prm);
                        }
                    }
                }
                prm = new SqlParameter();
                prm.ParameterName = "@DCID";
                prm.Direction = ParameterDirection.Output;
                string InsertQuery = "INSERT INTO [" + tableName + "] (" + columns + ")VALUES (" + parameters + ")";
                InsertQuery += "SET @DCID = @@Identity";
                myCommand.CommandText = InsertQuery;

                // Execute the command
                bool status = false;
                myConnection.Open();
                if (myCommand.ExecuteNonQuery() > 0)
                {
                    status = true;
                    //Get ID value from database and set it in object
                    int dcID = (int)myCommand.Parameters["@DCID"].Value;
                }
                myConnection.Close();
                return status;
            }
        }


    }
}