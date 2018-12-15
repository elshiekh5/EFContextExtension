using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFContextExtension
{
    public class CustomDbContext : DbContext
    {
        public CustomDbContext(string entityConnectionString)
            : base(entityConnectionString)
        {

        }
        private SqlDataHelper _Extension;

        public SqlDataHelper Extension
        {
            get {
                if (_Extension == null)
                {
                    _Extension = new SqlDataHelper();
                }
                return _Extension; }
        }

    }
}
