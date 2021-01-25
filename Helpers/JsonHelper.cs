using CommonDTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public class KnownTypesBinder : ISerializationBinder
    {
        Dictionary<string, Type> _mapName2Type = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<Type, string> _mapType2Name = new Dictionary<Type, string>();

        public Type BindToType(string assemblyName, string typeName)
        {
            return _mapName2Type[typeName];
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {

            assemblyName = null;
            if (!_mapType2Name.TryGetValue(serializedType, out typeName))
                throw new Exception($"type {serializedType.Name} is not registered with KnownTypeBinder");
        }

        public void RegisterType(Type t)
        {
            string key = t.Name;
            _mapName2Type.Add(key, t);
            _mapType2Name.Add(t, key);
        }
    }


    public static class JsonHelper
    {
        static JsonSerializer _serializerWithEncryption;
        static JsonSerializer _serializerWithoutEncryption;

        static JsonSerializerSettings _deserializeSettings;
        static JsonSerializerSettings _serializeSettings;
        static JsonSerializerSettings _serializeSettingsWithEncryption;
        
        static KnownTypesBinder _knownTypesBinder;

        static JsonHelper()
        {
            _knownTypesBinder = new KnownTypesBinder();

            _deserializeSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = _knownTypesBinder,
            };
            _deserializeSettings.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = false });

            _serializeSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = _knownTypesBinder
            };
            _serializeSettings.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = false });

            _serializeSettingsWithEncryption = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = _knownTypesBinder
            };
            _serializeSettingsWithEncryption.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = true });

            _serializerWithEncryption = new JsonSerializer
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            _serializerWithEncryption.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = true });

            _serializerWithoutEncryption = new JsonSerializer
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            _serializerWithoutEncryption.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = false });
        }

        class internalclass
        {
            public PaymentInfo paymentInfo { get; set; }
        }
        public static PaymentInfo GetPaymentInfo(string s)
        {
            //let's check if we have object that has paymentInfo
            internalclass t =  GetObject<internalclass>(s);
            if (t == null)
                return null;
            return t.paymentInfo;
        }

        public static T GetObject<T>(string s)
        {
            return (T)GetObject(s, typeof(T));
        }

        public static object GetObject(string s)
        {
            return JsonConvert.DeserializeObject(s, _deserializeSettings);
        }

        public static object GetObject(string s, Type t)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            return JsonConvert.DeserializeObject(s, t, _deserializeSettings);
        }

        public static string GetJson(object o)
        {
            return JsonConvert.SerializeObject(o, typeof(object), _serializeSettings);
        }

        public static void RegisterType(Type t)
        {
            _knownTypesBinder.RegisterType(t);
        }

        public static string EncryptPaymentInfoObject(string payload)
        {
            return EncryptPaymentInfoObject(JObject.Parse(payload));
        }
        public static string EncryptPaymentInfoObject(JObject jObject)
        {
            JToken token = jObject["paymentInfo"];
            if (token != null)
            {
                PaymentInfo paymentInfo = token.ToObject<PaymentInfo>(_serializerWithoutEncryption);
                string paymentInfoJson = JsonConvert.SerializeObject(paymentInfo, Formatting.None, _serializeSettingsWithEncryption);
                token.Replace(JToken.Parse(paymentInfoJson));
                return jObject.ToString(Formatting.None);
            }
            else
                return jObject.ToString(Formatting.None);
        }

        public static string DecryptPaymentInfoObject(string payload)
        {
            JObject jObject = JObject.Parse(payload);
            JToken token = jObject["paymentInfo"];
            if (token != null)
            {
                PaymentInfo paymentInfo = token.ToObject<PaymentInfo>(_serializerWithEncryption);
                string paymentInfoJson = JsonConvert.SerializeObject(paymentInfo, Formatting.None, _serializeSettings);
                token.Replace(JToken.Parse(paymentInfoJson));
                payload = jObject.ToString(Formatting.None);
            }
            return payload;
        }
    }
}
