using Authorizers.Common;
using CommonDTO;
using Microsoft.AspNetCore.Mvc;
using PaymentDTO;
using PaymentDTO.Payment;
using PaymentWebService;
using System;
using PaymentWebService.Code;
using System.Linq;
using System.Threading.Tasks;
using PaymentWebService.Code.Pipelines;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Helpers;

namespace PaymentWebService.Controllers.V1
{
    [ApiController]
    [Route("payment/v1")]
    [ApiExceptionFilter]
    public class PaymentController : ControllerBase
    {
        static readonly JsonSerializerSettings _jsonSerializerSettings;

        static PaymentController()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            _jsonSerializerSettings.Converters.Add(new PaymentInfoJsonConverter() { _withEncryption = false});
        }

        [HttpPost("Sale")]
        public async Task<SaleResponseDTO> Sale()
        {
            return await Execute<SaleRequestDTO, SaleResponseDTO, SalesPipeline>((rq, rp, pipelineData) =>
            {
                rp.requestId = pipelineData._rq.requestId;
                rp.trId = pipelineData._transaction?.id;
                rp.scheduledTrId = pipelineData._scheduledTransactionId;
            });
        }

        [HttpPost("Auth")]
        public async Task<AuthResponseDTO> Auth()
        {
            return await Execute<AuthRequestDTO, AuthResponseDTO, AuthPipeline>((rq, rp, pipelineData) =>
            {
                rp.requestId = pipelineData._rq.requestId;
                rp.trId = pipelineData._transaction?.id;
                rp.scheduledTrId = pipelineData._scheduledTransactionId;
            });
        }

        [HttpPost("Void")]
        public async Task<VoidResponseDTO> Void()
        {
            return await Execute<VoidRequestDTO, VoidResponseDTO, VoidPipeline>((rq, rp, pipelineData) =>
            {
                rp.requestId = pipelineData._rq.requestId;
                rp.trId = pipelineData._transaction?.id;
            });
        }

        [HttpPost("Refund")]
        public async Task<RefundResponseDTO> Refund()
        {
            return await Execute<RefundRequestDTO, RefundResponseDTO, RefundPipeline>( (rq, rp, pipelineData) =>
            {
                rp.requestId = pipelineData._rq.requestId;
                rp.trId = pipelineData._transaction?.id;
            });
        }

        [HttpPost("SubscribeToEvent")]
        public async Task<SubscribeToEventResponseDTO> SubscribeToEvent()
        {
            return await Execute<SubscribeToEventRequestDTO, SubscribeToEventResponseDTO, SubscribeToEventPipeline>((rq, rp, pipelineData) =>
            {
                rp.subscriptionId = pipelineData._subscriptionId;
                return;
            });
        }


        /*
        [HttpPost("ReSale")]
        public SaleResponseDTO ReSale(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }

        [HttpPost("Refund")]
        public SaleResponseDTO Refund(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }

        [HttpPost("StandAloneRefund")]
        public SaleResponseDTO StandAloneRefund(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }

        [HttpPost("Authorize")]
        public SaleResponseDTO Authorize(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }
        [HttpPost("ReAuthorize")]
        public SaleResponseDTO ReAuthorize(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }

        [HttpPost("Capture")]
        public SaleResponseDTO Capture(AuthroizedRequestDTO r)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(r, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }

        [HttpPost("Void")]
        public SaleResponseDTO Void(AuthroizedRequestDTO rq)
        {
            return Execute<SaleRequestDTO, SaleResponseDTO>(rq, new SaleResponseDTO(), (rq, rp) =>
            {
            });
        }
        */

        async Task<TOut> Execute<TIn, TOut, TPipeline>(Action<TIn, TOut, TPipeline> action)
            where TIn : AuthroizedRequestDTO
            where TOut : ResponseDTO, new()
            where TPipeline : PipelineData<TIn>, new()
        {
            var stream = new StreamReader(Request.Body);
            string requestBody = await stream.ReadToEndAsync();
            var rq = JsonConvert.DeserializeObject<TIn>(requestBody, _jsonSerializerSettings);
            TOut rp = new TOut();
            TPipeline pipeline = new TPipeline()
            {
                _requestBody = requestBody,
                _accountId = rq.accountId,
                _rq = rq
            };

            await pipeline.ExecutePipeline().ConfigureAwait(false);
            rp.errorCode = (int)(pipeline._success ? PaymentDTO.Payment.ERRORCODE.NONE : PaymentDTO.Payment.ERRORCODE.FAIL);
            rp.errors = pipeline._errors;

            action(rq, rp, pipeline);
            return rp;
        }
       
    }
}
