using CommonDTO;
using PaymentDTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentWebService.Code.Pipelines
{
    public class SubscribeToEventPipeline : PipelineData<SubscribeToEventRequestDTO>
    {
        public int _subscriptionId = -1;
        public override PipelineSteps[] GetPipelineSteps()
        {
            return new PipelineSteps[] {
                new PipelineSteps(SaveToDb, PipelineSteps.RUNCONDITIONS.ALL),
            };
        }
        public async Task SaveToDb()
        {
            HookData hookData = null;
            switch (_rq.hookType)
            {
                case HOOKTYPE.URL:
                    var urLHookData = Helpers.JsonHelper.GetObject<UrlHookData>(_rq.hookDataJson);
                    if( String.IsNullOrEmpty(urLHookData.url))
                        AddError($"You must provide url in your hookData");
                    hookData = urLHookData;
                    break;
                default:
                    AddError($"Hook type {_rq.hookType} is not supported");
                    break;
            }
            if(_success)
                _subscriptionId = await EventRepository.CreateHook(_accountId, (int)_rq.eventType, _rq.eventSubType, _rq.eventId, (int)_rq.hookType, hookData);
        }
    }
}
