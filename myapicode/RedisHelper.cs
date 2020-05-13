using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace myapicode
{
    public class RedisHelper
    {
        public static HashEntry[] ToHashEnties(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(proterty => new HashEntry(proterty.Name, proterty.GetValue(obj).ToString())).ToArray();
        }
        public static T ConvertFromRedis<T>(HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry()))
                {
                    continue;
                }
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }
            return (T)obj;
        }

    }
}
