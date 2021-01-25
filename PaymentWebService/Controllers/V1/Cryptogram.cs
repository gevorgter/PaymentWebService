using CommonDTO;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaymentDTO;
using PaymentDTO.Cryptogram;
using PaymentWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PaymentWebService.Controllers.V1
{
    [ApiController]
    [Route("cryptogram/v1")]
    [ApiExceptionFilter]
    public class CryptogramController : ControllerBase
    {
        static JsonSerializer _serializer;
        static JsonSerializerSettings _jsonSerializerSettings;

        static CryptogramController()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };

            _jsonSerializerSettings.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = false });

            _serializer = new JsonSerializer
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            _serializer.Converters.Add(_jsonSerializerSettings.Converters[0]);
        }

        [HttpPost("PutString")]
        public async Task<PutObjectResponseDTO> PutString()
        {
            return await Execute<PutObjectRequestDTO, PutObjectResponseDTO>(async (accountId, rq, rp) =>
            {
                rp.cryptogram = await Cryptogram.Cryptogram.PutString(accountId, (JObject)rq.payload, rq.encrypted, rq.ttlInMinutes, rq.retriveCount).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [HttpPost("GetString")]
        public async Task<GetObjectResponseDTO> GetString()
        {
            return await Execute<GetObjectRequestDTO, GetObjectResponseDTO>(async (accountId, rq, rp) =>
            {
                string res = await Cryptogram.Cryptogram.GetString(accountId, rq.cryptogram).ConfigureAwait(false);
                if (res == null)
                {
                    rp.errorCode = (int)PaymentDTO.Cryptogram.ERRORCODE.CRYPTOGRAMNOTFOUND;
                    rp.errors = new List<string>() { "Cryptogram not found" };
                }
                else
                {
                    rp.payload = res;
                }
            }).ConfigureAwait(false);
        }

        async Task<TOut> Execute<TIn, TOut>(Func<int, TIn, TOut, Task> action)
           where TIn : AuthroizedRequestDTO
           where TOut : ResponseDTO, new()
        {
            var stream = new StreamReader(Request.Body);
            string requestBody = await stream.ReadToEndAsync();
            TIn rq = JsonConvert.DeserializeObject<TIn>(requestBody, _jsonSerializerSettings);
            TOut rp = new TOut();
            /*
            authentication
            */
            int accountId = 1;
            await action(accountId, rq, rp).ConfigureAwait(false);
            return rp;
        }
    }
}
