using Authorizers.Common;
using CommonDTO;
using GiftCardDto;
using System;
using System.Threading.Tasks;

namespace Authorizers
{
    public class GifCardAuthroizerResponse : AuthorizerResponse
    {
        public int transactionId { get; set; }
    }
    public class GiftCardAuthorizer : Authorizer
    {
        static string _endpoint = "https://localhost:44368/gifcard";

        GiftCardClientConfig _clientConfig;

        public GiftCardAuthorizer(GiftCardClientConfig clientConfig)
        {
            _clientConfig = clientConfig;
        }

        public bool IsError(GiftCardDto.BaseResponse rp, AuthorizerResponse response)
        {
            if (rp == null)
            {
                //something broke on internet
                response.success = false;
                response.message = "Network problem";
                return true;
            }
            if (rp.errorCode != 0)
            {
                response.success = false;
                response.message = rp.errorMessage;
                return true;
            }
            return false;
        }
        public override async Task<AuthorizerResponse> SaleTransaction(AuthorizerSaleTransaction transaction)
        {
            GiftCardDto.AmountRequest rq = new GiftCardDto.AmountRequest();
            GiftCardPayment paymentInfo = transaction.paymentInfo as GiftCardPayment;
            rq.accountId = _clientConfig.accountId;
            rq.password = _clientConfig.password;
            rq.amount = -transaction.amount.amount;
            rq.card = paymentInfo.giftCard.giftCardNum;
            rq.pinCode = paymentInfo.giftCard.pinCode;
            (var rp, _) = await SendPostRequest<GiftCardDto.TransactionResponse>(_endpoint + "/v1/TransferMoney", rq).ConfigureAwait(false);
            GifCardAuthroizerResponse response = new GifCardAuthroizerResponse();
            if (!IsError(rp, response))
            {
                response.success = true;
                response.transactionId = rp.data.trId;
            }
            return response;
        }

        public async override Task<AuthorizerResponse> AuthTransaction(AuthorizerAuthTransaction transaction)
        {
            GiftCardDto.AmountRequest rq = new GiftCardDto.AmountRequest();
            GiftCardPayment paymentInfo = transaction.paymentInfo as GiftCardPayment;
            rq.accountId = _clientConfig.accountId;
            rq.password = _clientConfig.password;
            rq.amount = -transaction.amount.amount;
            rq.card = paymentInfo.giftCard.giftCardNum;
            rq.pinCode = paymentInfo.giftCard.pinCode;
            (var rp, _) = await SendPostRequest<GiftCardDto.TransactionResponse>(_endpoint + "/v1/TransferMoney", rq).ConfigureAwait(false);
            GifCardAuthroizerResponse response = new GifCardAuthroizerResponse();
            if (!IsError(rp, response))
            {
                response.success = true;
                response.transactionId = rp.data.trId;
            }
            return response;
        }

        public async override Task<AuthorizerResponse> RefundTransaction(AuthorizerRefundTransaction transaction)
        {
            GiftCardDto.AmountRequest rq = new GiftCardDto.AmountRequest();
            GiftCardPayment paymentInfo = transaction.originalPaymentInfo as GiftCardPayment;
            rq.accountId = _clientConfig.accountId;
            rq.password = _clientConfig.password;
            rq.amount = transaction.amount.amount;
            rq.card = paymentInfo.giftCard.giftCardNum;
            rq.pinCode = paymentInfo.giftCard.pinCode;

            (var rp, _) = await SendPostRequest<GiftCardDto.TransactionResponse>(_endpoint + "/v1/TransferMoney", rq).ConfigureAwait(false);
            GifCardAuthroizerResponse response = new GifCardAuthroizerResponse();
            if (!IsError(rp, response))
            {
                response.success = true;
                response.transactionId = rp.data.trId;
            }
            return response;
        }

        public async override Task<AuthorizerResponse> StandaloneRefundTransaction(AuthorizerStandaloneRefundTransaction transaction)
        {
            GiftCardDto.AmountRequest rq = new GiftCardDto.AmountRequest();
            GiftCardPayment paymentInfo = transaction.paymentInfo as GiftCardPayment;
            rq.accountId = _clientConfig.accountId;
            rq.password = _clientConfig.password;
            rq.amount = transaction.amount.amount;
            rq.card = paymentInfo.giftCard.giftCardNum;
            rq.pinCode = paymentInfo.giftCard.pinCode;

            (var rp, _) = await SendPostRequest<GiftCardDto.TransactionResponse>(_endpoint + "/v1/TransferMoney", rq).ConfigureAwait(false);
            GifCardAuthroizerResponse response = new GifCardAuthroizerResponse();
            if (!IsError(rp, response))
            {
                response.success = true;
                response.transactionId = rp.data.trId;
            }
            return response;
        }

        public async override Task<AuthorizerResponse> VoidTransaction(AuthorizerVoidTransaction transaction)
        {
            GiftCardDto.AmountRequest rq = new GiftCardDto.AmountRequest();
            GiftCardPayment paymentInfo = transaction.originalPaymentInfo as GiftCardPayment;
            rq.accountId = _clientConfig.accountId;
            rq.password = _clientConfig.password;
            rq.amount = -transaction.originalAmount.amount;
            rq.card = paymentInfo.giftCard.giftCardNum;
            rq.pinCode = paymentInfo.giftCard.pinCode;

            (var rp, _) = await SendPostRequest<GiftCardDto.TransactionResponse>(_endpoint + "/v1/TransferMoney", rq).ConfigureAwait(false);
            GifCardAuthroizerResponse response = new GifCardAuthroizerResponse();
            if (!IsError(rp, response))
            {
                response.success = true;
                response.transactionId = rp.data.trId;
            }
            return response;
        }
    }
}
