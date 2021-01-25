using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Crypto;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


public class EncryptedStringPropertyResolver : DefaultContractResolver
{
    public EncryptedStringPropertyResolver()
    {
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

        // Find all string properties that have a [JsonEncrypt] attribute applied
        // and attach an EncryptedStringValueProvider instance to them
        foreach (JsonProperty prop in props.Where(p => p.PropertyType == typeof(string)))
        {
            PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
            if (pi != null && pi.GetCustomAttribute(typeof(CommonDTO.JsonEncryptAttribute), true) != null)
            {
                prop.ValueProvider =
                    new EncryptedStringValueProvider(pi);
            }
        }

        return props;
    }

    class EncryptedStringValueProvider : IValueProvider
    {
        PropertyInfo targetProperty;

        public EncryptedStringValueProvider(PropertyInfo targetProperty)
        {
            this.targetProperty = targetProperty;
        }

        // GetValue is called by Json.Net during serialization.
        // The target parameter has the object from which to read the unencrypted string;
        // the return value is an encrypted string that gets written to the JSON
        public object GetValue(object target)
        {
            string value = (string)targetProperty.GetValue(target);
            return value.Encrypt();
        }

        // SetValue gets called by Json.Net during deserialization.
        // The value parameter has the encrypted value read from the JSON;
        // target is the object on which to set the decrypted value.
        public void SetValue(object target, object value)
        {
                targetProperty.SetValue(target, ((string)value).Decrypt());
        }
    }
}