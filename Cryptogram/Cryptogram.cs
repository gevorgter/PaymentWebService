using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Security.Cryptography;
using Crypto;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CommonDTO;
using Helpers;
using System.Globalization;

namespace Cryptogram
{
    public class Cryptogram
    {
        static int _counter;
        const int CLEANUPINTERVAL = 20;

        static async Task DoCleanup(SqlConnection con)
        {
            _counter = 0;
            await con.ExecuteAsync("DELETE FROM tblCryptogram WHERE retrivalCount<=0 OR expirationDateTime<GETDATE()").ConfigureAwait(false);
        }

        public static async Task<string> PutString(int accountId, JObject jLoad, bool encrypted = false, int ttlInMinutes = 20, int retrivalCount = 1)
        {
            string payload;
            if (encrypted)
                payload = jLoad.ToString();
            else
                payload = JsonHelper.EncryptPaymentInfoObject(jLoad);

            return await SaveToDb(accountId, payload, encrypted, ttlInMinutes, retrivalCount);
        }

        public static async Task<string> PutString(int accountId, string payload, bool encrypted = false, int ttlInMinutes = 20, int retrivalCount = 1)
        {
            if (encrypted)
                payload = payload.Encrypt();
            else
                payload = JsonHelper.EncryptPaymentInfoObject(payload);

            return await SaveToDb(accountId, payload, encrypted, ttlInMinutes, retrivalCount);
        }

        static async Task<string> SaveToDb(int accountId, string payload, bool encrypted = false, int ttlInMinutes = 20, int retrivalCount = 1)
        {
            Guid crtyptogram = Guid.NewGuid();

            using SqlConnection con = Global.Connection;
            await con.ExecuteAsync("INSERT tblCryptogram([id],[accountId], [expirationDateTime],[retrivalCount],[encrypted],[o]) VALUES(@id,@accountId,@expDate,@retrivalCount,@encrypted,@o)", new
            {
                id = crtyptogram,
                accountId,
                expDate = DateTime.Now.AddMinutes(ttlInMinutes),
                retrivalCount = retrivalCount,
                encrypted = encrypted,
                o = payload
            }).ConfigureAwait(false);
            return crtyptogram.ToString("N");
        }
        public static async Task<string> GetString(int accountId, string cryptogram)
        {
            Guid cr = Guid.ParseExact(cryptogram, "N");
            using SqlConnection con = Global.Connection;
            con.Open();
            if (Interlocked.Increment(ref _counter) > CLEANUPINTERVAL)
                await DoCleanup(con).ConfigureAwait(false);

            var obj = await con.QuerySingleOrDefaultAsync<CryptogramDBDTO>("SELECT [id],[accountId], [expirationDateTime],[retrivalCount],[encrypted],[o] FROM tblCryptogram " +
                "WHERE Id=@id AND accountId=@accountId AND [expirationDateTime]>@expDate AND [retrivalCount]>0; UPDATE tblCryptogram SET retrivalCount = retrivalCount-1 WHERE Id=@id AND accountId=@accountId", new
            {
                id = cr,
                accountId,
                expDate = DateTime.Now
            }).ConfigureAwait(false);
            con.Close();
            if (obj == null)
                return null;
            if (obj.encrypted)
                obj.o = obj.o.Decrypt();
            else
                obj.o = JsonHelper.DecryptPaymentInfoObject(obj.o);

            return obj.o;
        }

        public static async Task<string> PeekString(int accountId, string cryptogram)
        {
            Guid cr = Guid.ParseExact(cryptogram, "N");
            using SqlConnection con = Global.Connection;
            con.Open();
            if (Interlocked.Increment(ref _counter) > CLEANUPINTERVAL)
                await DoCleanup(con);

            var obj = await con.QuerySingleOrDefaultAsync<CryptogramDBDTO>("SELECT [id],[accountId], [expirationDateTime],[retrivalCount],[encrypted],[o] FROM tblCryptogram " +
                "WHERE Id=@id AND accountId=@accountId AND [expirationDateTime]>@expDate AND [retrivalCount]>0", new
                {
                    id = cr,
                    accountId,
                    expDate = DateTime.Now
                }).ConfigureAwait(false);
            con.Close();
            if (obj == null)
                return null;
            if (obj.encrypted)
                obj.o = obj.o.Decrypt();
            else
                obj.o = JsonHelper.DecryptPaymentInfoObject(obj.o);

            return obj.o;
        }


        public static async Task<string> PutObject(int accountId, object o, bool encrypted = false, int ttlInMinutes = 20, int retrivalCount = 1)
        {
            string tmp = JsonConvert.SerializeObject(o);
            return await PutString(accountId, tmp, encrypted, ttlInMinutes, retrivalCount).ConfigureAwait(false);
        }
        public static async Task<object> GetObject(int accountId, string crypto, Type t, JsonSerializerSettings settings = null)
        {
            string json = await GetString(accountId, crypto).ConfigureAwait(false);
            if (json == null)
                return null;
            return JsonConvert.DeserializeObject(json, t, settings);
        }
        public static async Task<T> GetObject<T>(int accountId, string crypto, JsonSerializerSettings settings = null)
        {
            string tmp = await GetString(accountId, crypto).ConfigureAwait(false);
            if (tmp == null)
                return default(T);
            return (T)JsonConvert.DeserializeObject<T>(tmp, settings);
        }

        public static async Task<string> PutUknownObject(int accountId, object o, bool encrypted = false, int ttlInMinutes = 20, int retrivalCount = 1)
        {
            Type t = o.GetType();
            string typeString = t.AssemblyQualifiedName;
            string tmp = typeString + "|" + JsonConvert.SerializeObject(o);
            
            return await PutString(accountId, tmp, encrypted, ttlInMinutes, retrivalCount).ConfigureAwait(false);
        }
        public static async Task<object> GetUnknowObject(int accountId, string crypto, JsonSerializerSettings settings = null)
        {
            string tmp = await GetString(accountId, crypto).ConfigureAwait(false);
            if (tmp == null)
                return null;
            int i1 = tmp.IndexOf('|');
            if (i1 < 2)
                return null;
            string assemblyQualifiedName = tmp.Substring(0, i1);
            string json = tmp.Substring(i1);
            Type t = Type.GetType(assemblyQualifiedName);
            return JsonConvert.DeserializeObject(json, t, settings);
        }
    }
}
