using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility.Helpers
{
    public static class MetaActions
    {
        public static object SetProperty(object obj, string propertyName, object value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Handle ExpandoObject and IDictionary<string, object>
            if (obj is IDictionary<string, object> expandoDict)
            {
                expandoDict[propertyName] = value;
                return expandoDict;
            }

            // Handle dynamic objects with runtime binding
            var objType = obj.GetType();

            // Check if the object has the property
            var property = objType.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
                return obj;
            }

            // Handle cases where the object doesn't have the property (dynamic add)
            if (obj is System.Dynamic.ExpandoObject dynamicObj)
            {
                ((IDictionary<string, object>)dynamicObj)[propertyName] = value;
                return dynamicObj;
            }

            // Handle dynamic objects that aren't ExpandoObject (e.g., JObject or custom classes)
            if (obj is object dyn)
            {
                try
                {
                    var expando = new ExpandoObject() as IDictionary<string, object>;
                    // Copy properties from original object
                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        expando.Add(prop.Name, prop.GetValue(obj));
                    }

                    expando.Add(propertyName, value);
                    return expando;
                }
                catch
                {
                    throw new InvalidOperationException($"Property '{propertyName}' could not be set on the given object.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Property '{propertyName}' could not be set on the given object.");
            }
        }
        public static object GetPrimaryKeyValue<T>(this T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();
            var keyProperty = type.GetProperty("Id") ?? type.GetProperty(type.Name + "Id");

        //    keyProperty = type.GetProperties()
        //.FirstOrDefault(p => p.Name.IndexOf("Id", StringComparison.OrdinalIgnoreCase) >= 0);
            if (keyProperty != null)
                return keyProperty.GetValue(obj);

            if (keyProperty == null)
                throw new InvalidOperationException("Primary key property not found.");

            return keyProperty.GetValue(obj);
        }

    }

}
