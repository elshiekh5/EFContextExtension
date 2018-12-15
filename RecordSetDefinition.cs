using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFContextExtension
{
   public  class RecordSetDefinition
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsGenericType  { get; set; }
        public Type GenericObjectType { get; set; }

    }

    public class RecordSetDefinitionManager
    {
        public List<RecordSetDefinition> GenerateRecordSetDefinition(Type[] types, List<string> names)
        {
            List<RecordSetDefinition> recordSetDefinitions = new List<RecordSetDefinition>();
            RecordSetDefinition item = null;
            int index = 0;
            foreach (var t in types)
            {
                item = new RecordSetDefinition();
                item.Type = t;
                item.IsGenericType = t.IsGenericType;
                if (t.IsGenericType)
                {
                    item.GenericObjectType = t.GetGenericArguments()[0];
                }
                if (names == null)
                {

                    item.Name = (t.IsGenericType) ? item.GenericObjectType.Name : item.Type.Name;
                }
                else
                {
                    item.Name = names[index];
                }
                recordSetDefinitions.Add(item);
                ++index;
            }
            return recordSetDefinitions;

        }

    }
}
