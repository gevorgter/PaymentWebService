using CommonDTO;
using Dapper;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Authorizers.Common
{
    public abstract class Authorizer
    {
        HttpClient _httpClient;

        public Authorizer()
        {
            _httpClient = new HttpClient();
        }

        static ClientAuthorizerConfig GetConfig(int accountId, PaymentType paymentType)
        {
            using var con = Global.Connection;
            var configString = con.QueryFirstOrDefault<string>("SELECT configJson FROM tblClientAuthorizerConfig WHERE accountId=@accountId AND paymentType=@paymentType", new
            {
                accountId,
                paymentType = (int)paymentType,
            });
            if (configString == null)
                throw new AuthorizerException($"No config file for client {accountId}, paymentType {paymentType}");

            return Helpers.JsonHelper.GetObject(configString) as ClientAuthorizerConfig;
        }

        public static Authorizer GetAuthorizer(int accountId, PaymentType paymentType)
        {
            var authorizerConfid = GetConfig(accountId, paymentType);
            if (authorizerConfid == null)
                throw new Exception($"No authorizer for accountId:{accountId} and paymentType:{paymentType}");
            return authorizerConfid.GetAuthorizer();
        }

        public abstract Task<AuthorizerResponse> SaleTransaction(AuthorizerSaleTransaction transaction);
        public abstract Task<AuthorizerResponse> AuthTransaction(AuthorizerAuthTransaction transaction);
        public abstract Task<AuthorizerResponse> RefundTransaction(AuthorizerRefundTransaction transaction);
        public abstract Task<AuthorizerResponse> StandaloneRefundTransaction(AuthorizerStandaloneRefundTransaction transaction);
        public abstract Task<AuthorizerResponse> VoidTransaction(AuthorizerVoidTransaction transaction);



        //public abstract bool ValidateTransaction(AuthorizerTransaction transaction);

        public async Task<(string,HttpStatusCode)> SendPostRequest(string url, string data, string contentType = "application/json")
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", contentType);
                request.Content = new StringContent(data, Encoding.UTF8, contentType);

                var rp = await _httpClient.SendAsync(request).ConfigureAwait(false);
                if (!rp.IsSuccessStatusCode)
                {
                    var tmp = await rp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Log.Error("Authorizer returned unsuccessful code {StatusCode}, conent: {content}, url:{url}, data: {data}", rp.StatusCode, tmp, url, data);
                    return (tmp, rp.StatusCode); ;
                }
                return (await rp.Content.ReadAsStringAsync().ConfigureAwait(false), HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Log.Error(e, "We could not connect to authorizer, url:{url}, data: {data}", url, data);
                return (null, HttpStatusCode.ServiceUnavailable);
            }
        }

        public async Task<(T, HttpStatusCode)> SendPostRequest<T>(string url, object data)
        {
            string r = "";
            string d = "";
            HttpStatusCode statusCode;
            try
            {
                d = JsonConvert.SerializeObject(data);
                (r, statusCode) = await SendPostRequest(url, d).ConfigureAwait(false);
                if (r == null)
                    return (default, statusCode);
                return (JsonConvert.DeserializeObject<T>(r), statusCode);
            }
            catch (Exception e)
            {
                Log.Error(e, "We could not serialize/deserialize data. In: {in}, Out: {out}", d, r);
                return default;
            }
        }
    }
}
