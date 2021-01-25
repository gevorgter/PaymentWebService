using Newtonsoft.Json;
using PaymentDTO;
using PaymentDTO.Cryptogram;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PaymentWebServiceClient
{
    

    public class Client
    {
        const string paymentServiceUrl = "https://localhost:44386";
        static HttpClient _httpClient = new HttpClient();

        static async Task<(string, HttpStatusCode)> SendPostRequest(string url, string data, string contentType = "application/json")
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", contentType);
                request.Content = new StringContent(data, Encoding.UTF8, contentType);

                var rp = await _httpClient.SendAsync(request).ConfigureAwait(false);
                string result = await rp.Content.ReadAsStringAsync().ConfigureAwait(false);
                return (result, rp.StatusCode);
            }
            catch (Exception e)
            {
                return (e.Message, HttpStatusCode.ServiceUnavailable);
            }
        }

        public static async Task<T> CallApi<T>(string url, object data)
            where T: PaymentDTO.ResponseDTO, new ()
        {
            string r = "";
            HttpStatusCode statusCode;
            try
            {
                string d = JsonConvert.SerializeObject(data);
                (r, statusCode) = await SendPostRequest(paymentServiceUrl + url, d).ConfigureAwait(false);
                if (statusCode != HttpStatusCode.OK)
                {
                    T rp = new T
                    {
                        errorCode = -1,
                        errors = new List<string>() { $"Bad status code:{statusCode} returned, message:{r}" }
                    };
                    return rp;
                }
                return JsonConvert.DeserializeObject<T>(r);
            }
            catch (Exception e)
            {
                T rp = new T
                {
                    errorCode = -2,
                    errors = new List<string>() { $"Exception happened:{e.Message} returned, message:{r}" }
                };
                return rp;
            }
        }

        public static async Task<PutObjectResponseDTO> CreateCryptogram(PutObjectRequestDTO rq, object payload)
        {
            if (payload != null)
                rq.payload = JsonConvert.SerializeObject(payload);
            return await CallApi<PutObjectResponseDTO>("/cryptogram/v1/PutString", rq).ConfigureAwait(false);
        }
        public static async Task<GetObjectResponseDTO> GetObjectFromCryptogram<T>(GetObjectRequestDTO rq)
        {
            return await CallApi<GetObjectResponseDTO>("/cryptogram/v1/GetString", rq).ConfigureAwait(false);
        }
    }
}
