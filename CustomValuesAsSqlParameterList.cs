using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace EFContextExtension
{
    //--------------------------------------------------
    public class CustomValuesAsSqlParameterList
    {

        public CustomValuesAsSqlParameterList()
        {
            Parameters = new List<SqlParameter>();
        }
        public List<Object> ParametersValues { get; set; }

        public List<SqlParameter> Parameters { get; set; }


        public SqlParameter Add(string parameterName, object value)
        {
            var p = new SqlParameter(parameterName, value);
            Type typeOfParameter = value.GetType();
            if (typeOfParameter == typeof(string))
            {
                p.Size = ((string)value).Length;
            }
            return this.Add(p);
        }
       

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
        {
            return this.Add(new SqlParameter(parameterName, sqlDbType));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size)
        {
            return this.Add(new SqlParameter(parameterName, sqlDbType, size));
        }
        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
        {
            return this.Add(new SqlParameter(parameterName, sqlDbType, size, sourceColumn));
        }
        public SqlParameter Add(SqlParameter parameter)
        {
            Parameters.Add(parameter);
            return parameter;
        }


        public SqlParameter AddOutputParameter(string parameterName, SqlDbType sqltype)
        {
            var p = new SqlParameter();
            p.ParameterName = parameterName;
            p.SqlDbType = sqltype;
            p.Direction = ParameterDirection.Output;
            this.Add(p);

            return p;
        }

        public SqlParameter AddOutputParameter_Integer(string parameterName)
        {
            return AddOutputParameter(parameterName, SqlDbType.Int);
        }
        public SqlParameter AddOutputParameter_Boolean(string parameterName)
        {
            return AddOutputParameter(parameterName, SqlDbType.Bit);
        }
    }
}