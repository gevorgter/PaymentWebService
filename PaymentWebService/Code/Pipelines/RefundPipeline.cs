﻿using Authorizers.Common;
using CommonDTO;
using PaymentDTO.Payment;
using System;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    public class RefundPipeline : PipelineData<RefundRequestDTO>
    {
        public CommonDTO.Transaction _transaction;
        public Authorizers.Common.Authorizer _authorizer;
        public AuthorizerTransaction _authorizerTransaction;
        public AuthorizerResponse _authorizerResponse;

        public decimal _originalAmount = 0;
        public decimal _amountLeft = 0;
        public int _lockId = 0;
        public int _originalTransactionId;
        public Transaction originalTransaction;
        public AuthorizerResponse authorizerOriginalResponse;

        public override PipelineSteps[] GetPipelineSteps()
        {
            return new PipelineSteps[] {
                new PipelineSteps(Validate),
                new PipelineSteps(GetAndLockOriginalTransaction),
                new PipelineSteps(FigureoutAuthorizer),
                new PipelineSteps(CreateTransaction),
                new PipelineSteps(SaveTransactionToDb),
                new PipelineSteps(SendToAuthorizer),
                new PipelineSteps(SaveResultAndUnlock, PipelineSteps.RUNCONDITIONS.ALL),
                new PipelineSteps(GenerateEventForHooks),
             };
        }
   
        public Task Validate()
        {
            if (!Int32.TryParse(_rq.originalTrId, out int trId))
            {
                AddError($"trId:{_rq.originalTrId} must be number");
                return Task.CompletedTask;
            }
            _originalTransactionId = trId;
            return Task.CompletedTask;
        }
        public async Task GenerateEventForHooks()
        {
            var ev = new CommonDTO.RefundEventData()
            {
                originalTransactionId = _originalTransactionId.ToString(),
                refundTransactionId = _transaction.id.ToString(),
                amount = _rq.amount
            };
            await EventRepository.SaveEvent(_accountId, (int)EVENTTYPE.TRANSACTION, (int)TRANSACTIONEVENTSUBTYPE.REFUND, ev.originalTransactionId, ev);
        }
       
        public async Task GetAndLockOriginalTransaction()
        {
            int lockId = Global._rnd.Next(1, int.MaxValue);
            var trDBO = await TransactionRepository.GetTransactionDBDTO(_originalTransactionId, lockId).ConfigureAwait(false);
            if (trDBO == null)
            {
                AddError("Transaction either does not exist or locked and can not be refunded");
                return;
            }
            _lockId = lockId;

            if (trDBO.status != (int)TransactionStatus.SUCCESS)
            {
                AddError($"You can only refund successful transaction");
                return;
            }
            var obj = TransactionRepository.GetTransaction(trDBO);
            Transaction transaction = obj as SaleTransaction;
            if (transaction == null)
            {
                //let's check if it is auth
                transaction = obj as AuthTransaction;
                if (transaction == null)
                {
                    AddError($"You can only issue refund against sale or auth transaction");
                    return;
                }
                else
                    transaction.id = trDBO.id;
            }
            else
                transaction.id = trDBO.id;
           
            var authorizerResponse = Helpers.JsonHelper.GetObject(trDBO.authorizerResponseJson) as AuthorizerResponse;
            if (authorizerResponse == null)
            {
                AddError($"Your transaction has not been sent to authorizer, can not refund");
                return;
            }
            originalTransaction = transaction;
            _originalAmount = trDBO.originalAmount;
            _amountLeft = trDBO.amountLeft;
            if (_amountLeft < _rq.amount.amount)
            {
                AddError($"You can not refund {_rq.amount.amount:0.00} more than amount left {_amountLeft:0.00} on this transaction");
                return;
            }
            authorizerOriginalResponse = authorizerResponse;
        }

        public async Task SaveTransactionToDb()
        {
            await TransactionRepository.SaveTransaction(_accountId, TransactionType.REFUND, (int)originalTransaction.id, 0M, _transaction).ConfigureAwait(false);
        }

        public Task CreateTransaction()
        {
            _transaction = new VoidTransaction()
            {
                originalTrId = Int32.Parse(_rq.originalTrId)
            };
            return Task.CompletedTask;
        }

        public Task FigureoutAuthorizer()
        {
            //for now authorizer is based on payment
            var tr = originalTransaction as IPaymentInfo;
            PaymentType paymentType = PaymentType.CreditCard;
            if (tr.paymentInfo is AchPaymentInfo)
                paymentType = PaymentType.Ach;
            if (tr.paymentInfo is GiftCardPayment)
                paymentType = PaymentType.GiftCard;

            _authorizer = Authorizers.Common.Authorizer.GetAuthorizer(_accountId, paymentType);
            return Task.CompletedTask;
        }

        public async Task SendToAuthorizer()
        {
            AuthorizerRefundTransaction rq = new AuthorizerRefundTransaction();
            rq.amount = (originalTransaction as IAmount).amount;
            rq.amount.amount = _amountLeft;
            rq.originalPaymentInfo = (originalTransaction as IPaymentInfo).paymentInfo;
            rq.originalTransactionResponse = authorizerOriginalResponse;

            var rp = await _authorizer.RefundTransaction(rq).ConfigureAwait(false);
            _authorizerResponse = rp;
            if (rp.success)
            {
                _transaction.status = TransactionStatus.SUCCESS;
                _amountLeft -= rq.amount.amount;
            }
            else
            {
                _transaction.status = TransactionStatus.REJECTED;
                _transaction.statusMessage = rp.message;
                _success = false;
                AddError(rp.message);
            }
        }

        public async Task SaveResultAndUnlock()
        {
            if (_authorizerResponse != null)
                await TransactionRepository.SaveAuthorizerResponse((int)_transaction.id, (int)_transaction.status, _transaction.statusMessage, 0, 0, _authorizerResponse).ConfigureAwait(false);
            //update orininal transaction only if successful
            if ((originalTransaction != null) && (_transaction.status == TransactionStatus.SUCCESS))
            {
                await TransactionRepository.SaveResultAndUnlock(_originalTransactionId, (int)TransactionStatus.REFUNDED, originalTransaction.statusMessage, _originalAmount, _amountLeft).ConfigureAwait(false);
                _lockId = 0;
            }
            if (_lockId > 0)
                await TransactionRepository.UnlockTransaction(_originalTransactionId, _lockId).ConfigureAwait(false);

            _lockId = 0;
        }

    }
}
