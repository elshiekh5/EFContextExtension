using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace EFContextExtension
{
    //--------------------------------------------------
    public class CustomSqlParameterList
    {
        public CustomSqlParameterList()
        {
            Parameters = new List<SqlParameter>();
        }
        public List<SqlParameter> Parameters { get; set; }
        public SqlParameter Add(string parameterName, object value)
        {
            return this.Add(new SqlParameter(parameterName, value));
        }
        public SqlParameter AddWithValue(string parameterName, object value)
        {
            return this.Add(new SqlParameter(parameterName, value));
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
    }
}