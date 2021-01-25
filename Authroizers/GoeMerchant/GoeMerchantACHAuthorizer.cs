using Authorizers.Common;
using CommonDTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Authorizers
{
    class GoeMerchantACHAuthorizer : Authorizer
    {
        const string _endpoint = "https://secure.1stpaygateway.net/secure/RestGW/Gateway/Transaction";
        readonly GoeMerchantACHConfig _clientConfig;

        public GoeMerchantACHAuthorizer(GoeMerchantACHConfig clientConfig)
        {
            _clientConfig = clientConfig;
        }

        public AuthorizerResponse IsError(string response, HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.OK)
                return null;
            if (!String.IsNullOrEmpty(response))
            {
                var rp = JsonConvert.DeserializeObject<GoeResponse<object>>(response);
                if (rp.isError)
                {
                    string message = "";
                    foreach (var v in rp.errorMessages)
                    {
                        if (message.Length > 0)
                            message += '|';
                        message += v;
                    }
                    return new GoeMerchantTransactionResponse()
                    {
                        success = false,
                        message = message
                    };
                }
                if (rp.validationHasFailed)
                {
                    string message = "";
                    foreach (var v in rp.validationFailures)
                    {
                        if (message.Length > 0)
                            message += '|';
                        message += v.key + ":" + v.message;
                    }
                    return new GoeMerchantTransactionResponse()
                    {
                        success = false,
                        message = message
                    };
                }
            }

            return new GoeMerchantTransactionResponse()
            {
                success = false,
                message = "Bad Response from GOE",
            };
        }


        public override Task<AuthorizerResponse> AuthTransaction(AuthorizerAuthTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override async Task<AuthorizerResponse> RefundTransaction(AuthorizerRefundTransaction transaction)
        {
            //var originalRp = transaction.originalTransactionResponse as GoeMerchantSaleTransactionResponse;
            AchPaymentInfo paymentInfo = transaction.originalPaymentInfo as AchPaymentInfo;

            var tr = new GoeACHTransaction()
            {
                merchantKey = _clientConfig.merchantKey,
                processorId = _clientConfig.processorId,
                ipAddress = "127.0.0.1",

                transactionAmount = transaction.amount.amount.ToString("0.00"),

                aba = paymentInfo.achInfo.aba,
                dda = paymentInfo.achInfo.dda,
                accountType = (paymentInfo.achInfo.accountType == AchAccountType.CHECKING) ? "C" : "S",
                categoryText = paymentInfo.achInfo.achCategory,

                ownerName = paymentInfo.billingAddress?.firstName + " " + paymentInfo.billingAddress?.lastName,
                ownerStreet = paymentInfo.billingAddress?.address1,
                ownerStreet2 = paymentInfo.billingAddress?.address2,
                ownerCity = paymentInfo.billingAddress?.city,
                ownerState = paymentInfo.billingAddress?.state,
                ownerZip = paymentInfo.billingAddress?.postalCode,
                ownerCountry = paymentInfo.billingAddress?.country,

                ownerEmail = paymentInfo.billingAddress?.email,
                ownerPhone = paymentInfo.billingAddress?.phone,
            };

            string data = JsonConvert.SerializeObject(tr);
            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint + "/AchCredit", data);
            AuthorizerResponse r = IsError(response, statusCode);
            if (r != null)
                return r;
            var rp = JsonConvert.DeserializeObject<GoeResponse<GoeACHTransactionResponse>>(response);
            return new GoeMerchantTransactionResponse ()
            {
                authCode = rp.data.authResponse,
                referenceNumber = rp.data.referenceNumber,
                success = true,
                message = null
            };

        }

        public override async Task<AuthorizerResponse> SaleTransaction(AuthorizerSaleTransaction transaction)
        {
            AchPaymentInfo paymentInfo = transaction.paymentInfo as AchPaymentInfo;
            var tr = new GoeACHTransaction()
            {
                merchantKey = _clientConfig.merchantKey,
                processorId = _clientConfig.processorId,
                ipAddress = "127.0.0.1",

                transactionAmount = transaction.amount.amount.ToString("0.00"),
                
                aba = paymentInfo.achInfo.aba,
                dda = paymentInfo.achInfo.dda,
                accountType = (paymentInfo.achInfo.accountType == AchAccountType.CHECKING) ? "C" : "S",
                categoryText = paymentInfo.achInfo.achCategory,

                ownerName = paymentInfo.billingAddress?.firstName + " " + paymentInfo.billingAddress?.lastName,
                ownerStreet = paymentInfo.billingAddress?.address1,
                ownerStreet2 = paymentInfo.billingAddress?.address2,
                ownerCity = paymentInfo.billingAddress?.city,
                ownerState = paymentInfo.billingAddress?.state,
                ownerZip = paymentInfo.billingAddress?.postalCode,
                ownerCountry = paymentInfo.billingAddress?.country,

                ownerEmail = paymentInfo.billingAddress?.email,
                ownerPhone = paymentInfo.billingAddress?.phone,
            };

            string data = JsonConvert.SerializeObject(tr);
            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint + "/AchDebit", data);
            AuthorizerResponse r = IsError(response, statusCode);
            if (r != null)
                return r;


            var rp = JsonConvert.DeserializeObject<GoeResponse<GoeACHTransactionResponse>>(response);
            return new GoeMerchantTransactionResponse ()
            {
                authCode = rp.data.authCode,
                authResponse = rp.data.authResponse,
                avsResponse = "",
                cvv2Response = "",
                referenceNumber = rp.data.referenceNumber,
                orderId = rp.data.orderId,
                success = true,
                message = null
            };

        }

        public override Task<AuthorizerResponse> StandaloneRefundTransaction(AuthorizerStandaloneRefundTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override async Task<AuthorizerResponse> VoidTransaction(AuthorizerVoidTransaction transaction)
        {
            var originalRp = transaction.originalTransactionResponse as IReferenceNumber;
            var tr = new GoeVoidTransaction()
            {
                merchantKey = _clientConfig.merchantKey,
                processorId = _clientConfig.processorId,
                refNumber = originalRp.referenceNumber,
            };

            string data = JsonConvert.SerializeObject(tr);
            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint + "/AchVoid", data);
            AuthorizerResponse r = IsError(response, statusCode);
            if (r != null)
                return r;

            var rp = JsonConvert.DeserializeObject<GoeResponse<GoeVoidTransactionResponse>>(response);
            return new GoeMerchantTransactionResponse ()
            {
                authCode = rp.data.authResponse,
                referenceNumber = rp.data.referenceNumber,
                success = true,
                message = null
            };

        }
    }

}
