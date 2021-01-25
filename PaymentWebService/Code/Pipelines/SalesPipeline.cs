using Authorizers.Common;
using CommonDTO;
using PaymentDTO.Payment;
using System;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    public class SalesPipeline : PipelineData<SaleRequestDTO>
    {
        public int? _scheduledTransactionId = null;
        public CommonDTO.SaleTransaction _transaction;
        public Authorizers.Common.Authorizer _authorizer;
        public AuthorizerTransaction _authorizerTransaction;
        public AuthorizerResponse _authorizerResponse;

        public override PipelineSteps[] GetPipelineSteps()
        {
            return new PipelineSteps[] {
                new PipelineSteps(ResolveVault),
                new PipelineSteps(ResolveCryptogram),
                new PipelineSteps(FigureoutAuthorizer),
                new PipelineSteps(GetAuthorizerSpecificFields),
                new PipelineSteps(ValidateInformation),
                new PipelineSteps(CreateTransaction),
                new PipelineSteps(SaveIfScheduled),
                new PipelineSteps(SaveTransactionToDb),
                new PipelineSteps(SendToAuthorizer),
                new PipelineSteps(SaveResultToDb),
                new PipelineSteps(GenerateEventForHooks),
            };
        }
        public async Task GenerateEventForHooks()
        {
            var ev = new CommonDTO.SaleEventData()
            {
                transactionId = _transaction.id??0,
                amount = _rq.amount
            };
            await EventRepository.SaveEvent(_accountId, (int)EVENTTYPE.TRANSACTION, (int)TRANSACTIONEVENTSUBTYPE.SALE, ev.transactionId.ToString(), ev);
        }
        public async Task SaveTransactionToDb()
        {
            await TransactionRepository.SaveTransaction(_accountId, TransactionType.SALE, -1, _rq.amount.amount, _transaction).ConfigureAwait(false);
        }

        public async Task ResolveVault()
        {
            if (_rq is not IPaymentInfo obj)
                return;
            if (obj.paymentInfo is not VaultPayment vaultPayment)
                return;

            string vaultInfo = await Vault.Vault2.GetVaultData(_accountId, vaultPayment.vaultInfo.vaultId, vaultPayment.vaultInfo.sequenceId).ConfigureAwait(false);
            var paymentInfo = Helpers.JsonHelper.GetPaymentInfo(vaultInfo);
            if (paymentInfo == null)
                AddError($"Invalid vault information vaultId:{vaultPayment.vaultInfo.vaultId}, sequence:{vaultPayment.vaultInfo.sequenceId}");
            else
                obj.paymentInfo = paymentInfo;
        }

        public async Task ResolveCryptogram()
        {
            if (_rq is not IPaymentInfo obj)
                return;
            if (obj.paymentInfo is not CryptogramPayment cryptogramPayment)
                return;

            string cryptogram = await Cryptogram.Cryptogram.GetString(_accountId, cryptogramPayment.cryptogram).ConfigureAwait(false); //may be we should use Peek here

            var paymentInfo = Helpers.JsonHelper.GetPaymentInfo(cryptogram);
            if (paymentInfo == null)
                AddError($"Invalid cryptogram:{cryptogramPayment.cryptogram}");
            obj.paymentInfo = paymentInfo;
        }

        public Task CreateTransaction()
        {
            _transaction = Global._mapper.Map<SaleTransaction>(_rq);
            return Task.CompletedTask;
        }

        public async Task SaveIfScheduled()
        {
            if (_rq is not ISchedulingInfo obj)
                return;
            if (obj.schedulingInfo == null)
                return;

            //validate
            if (obj.schedulingInfo.scheduleType != ScheduleType.ONETIME)
            {
                if (obj.schedulingInfo.endOn <= obj.schedulingInfo.startOn)
                {
                    AddError($"endOn {obj.schedulingInfo.endOn:MM/dd/yyyy} can not be before start date {obj.schedulingInfo.startOn:MM/dd/yyyy}");
                    return;
                }
            }
            DateTime today = DateTime.Today;
            if (obj.schedulingInfo.startOn < today)
            {
                AddError($"startOn {obj.schedulingInfo.startOn:MM/dd/yyyy} date must be today or in a future");
                return;
            }
            if (obj.schedulingInfo.startOn == today)
            {
                obj.schedulingInfo.startOn = Helpers.Scheduler.CalculateNextRun(today, obj.schedulingInfo.startOn, obj.schedulingInfo.endOn, obj.schedulingInfo.scheduleType);
                obj.schedulingInfo.runTransactionTodayAsWell = true;
            }

            _scheduledTransactionId = await ScheduledTransaction.ScheduleTransaction(_accountId, obj.schedulingInfo, _transaction).ConfigureAwait(false);
             
            if (!obj.schedulingInfo.runTransactionTodayAsWell)
                FinishPipeline();
        }

        public Task FigureoutAuthorizer()
        {
            //for now authorizer is based on payment
            PaymentType paymentType = PaymentType.CreditCard;
            if (_rq.paymentInfo is AchPaymentInfo)
                paymentType = PaymentType.Ach;
            if (_rq.paymentInfo is GiftCardPayment)
                paymentType = PaymentType.GiftCard;

            _authorizer = Authorizers.Common.Authorizer.GetAuthorizer(_accountId, paymentType);
            _authorizerTransaction = new AuthorizerSaleTransaction()
            {
                amount = _rq.amount,
                paymentInfo = _rq.paymentInfo
            };
            return Task.CompletedTask;
        }

        public Task GetAuthorizerSpecificFields()
        {
            //here is the chance for us to get authorizer specifc fields from _requestBody
            return Task.CompletedTask;
        }

        public Task ValidateInformation()
        {
            if (_rq.amount.amount <= 0)
                AddError("Amount can not be less than 0 or grater than 10,0000");
            return Task.CompletedTask;
        }

        public async Task SendToAuthorizer()
        {
            var rp = await _authorizer.SaleTransaction((AuthorizerSaleTransaction)_authorizerTransaction).ConfigureAwait(false);
            _authorizerResponse = rp;
            if (rp.success)
                _transaction.status = TransactionStatus.SUCCESS;
            else
            {
                _transaction.status = TransactionStatus.REJECTED;
                _transaction.statusMessage = rp.message;
                _success = false;
                AddError(rp.message);
            }
        }

        public async Task SaveResultToDb()
        {
            if (_authorizerResponse != null)
                await TransactionRepository.SaveAuthorizerResponse((int)_transaction.id, (int)_transaction.status, _transaction.statusMessage, _transaction.amount.amount, _transaction.amount.amount, _authorizerResponse).ConfigureAwait(false);
        }
    }
}
