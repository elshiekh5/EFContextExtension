using System;
using System.Linq;
using System.Linq.Expressions;

namespace EFContextExtension
{
    public class DataProvider//<TEntity>  where TEntity : class
    {
        public static Func<TEntity, TEntity> CreateNewStatement<TEntity>(string fields)
        {
            if (string.IsNullOrEmpty(fields))
            {
                return null;
            }
                // input parameter "o"
                var xParameter = Expression.Parameter(typeof(TEntity), "o");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(TEntity));

            // create initializers
            var bindings = fields.Split(',').Select(o => o.Trim())
                .Select(o =>
                {

                    // property "Field1"
                    var mi = typeof(TEntity).GetProperty(o);

                    // original value "o.Field1"
                    var xOriginal = Expression.Property(xParameter, mi);

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(mi, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<TEntity, TEntity>>(xInit, xParameter);

            // compile to Func<Data, Data>
            return lambda.Compile();
        }
    }
}
