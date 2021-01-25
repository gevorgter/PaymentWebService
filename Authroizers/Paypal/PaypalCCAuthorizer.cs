using Authorizers.Common;
using CommonDTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Authorizers
{
    public class PaypalCCAuthorizer : Authorizer
    {
        const string _endpoint = "https://api-3t.sandbox.paypal.com/nvp";

        PaypalMerchantCCConfig _clientConfig;

        public PaypalCCAuthorizer(PaypalMerchantCCConfig clientConfig)
        {
            _clientConfig = clientConfig;
        }

        public AuthorizerResponse IsError(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.OK)
                return null;
            return new PaypalTransactionResponse()
            {
                success = false,
                message = "Could not contact paypal"
            };
        }

        public override async Task<AuthorizerResponse> AuthTransaction(AuthorizerAuthTransaction transaction)
        {
            CCPaymentInfo paymentInfo = transaction.paymentInfo as CCPaymentInfo;
            
            string sExpDate = paymentInfo.card.ccExpMonth.ToString("00");
            if (paymentInfo.card.ccExpYear < 2000)
                paymentInfo.card.ccExpYear += 2000;
            sExpDate += paymentInfo.card.ccExpYear.ToString();

            PaypalRequestBuilder requestBuilder = new PaypalRequestBuilder(_clientConfig.userName, _clientConfig.password, _clientConfig.signature);
            requestBuilder.Append("METHOD", "DoDirectPayment", 1000);
            requestBuilder.Append("PAYMENTACTION", "Authorization", 1000);
            requestBuilder.Append("IPADDRESS", "127.0.0.1", 15);
            requestBuilder.Append("ACCT", paymentInfo.card.ccNum, 1000);
            requestBuilder.Append("EXPDATE", sExpDate, 1000);
            requestBuilder.Append("CVV2", paymentInfo.card.cvv, 1000);

            requestBuilder.Append("FIRSTNAME", paymentInfo.billingAddress.firstName, 25);
            requestBuilder.Append("LASTNAME", paymentInfo.billingAddress.lastName, 25);
            requestBuilder.Append("STREET", paymentInfo.billingAddress.address1, 100);
            requestBuilder.Append("STREET2", paymentInfo.billingAddress.address2, 100);
            requestBuilder.Append("CITY", paymentInfo.billingAddress.city, 40);
            requestBuilder.Append("STATE", paymentInfo.billingAddress.state, 40);
            requestBuilder.Append("ZIP", paymentInfo.billingAddress.postalCode, 40);
            requestBuilder.Append("COUNTRYCODE", paymentInfo.billingAddress.country, 2);

            requestBuilder.Append("AMT", transaction.amount.amount.ToString("0.00"), 40);
            requestBuilder.Append("CURRENCYCODE", "USD", 40);
            requestBuilder.Append("INVNUM", "PR_inv", 40);

            /*
                        requestBuilder.Append("SHIPTONAME", sShFirst + " " + sShLast, 25);
                        requestBuilder.Append("SHIPTOSTREET", sShLine1, 100);
                        requestBuilder.Append("SHIPTOSTREET2", sShLine2, 100);
                        requestBuilder.Append("SHIPTOCITY", sShCity, 40);
                        requestBuilder.Append("SHIPTOSTATE", sShState, 40);
                        requestBuilder.Append("SHIPTOZIP", sShZip, 40);
                        requestBuilder.Append("SHIPTOCOUNTRY", clsGlobal.GetCountryCode(sShCountry), 2);

            */

            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint, requestBuilder.ToString(), "application/x-www-form-urlencoded");
            AuthorizerResponse r = IsError(statusCode);
            if (r != null)
                return r;

            //"TIMESTAMP=2020%2d07%2d09T00%3a10%3a00Z&CORRELATIONID=9a8182e401a35&ACK=Success&VERSION=78%2e0&BUILD=54677068&AMT=1%2e90&CURRENCYCODE=USD&AVSCODE=A&CVV2MATCH=M&TRANSACTIONID=2AX69342E26784917"
            //"TIMESTAMP=2020%2d07%2d09T00%3a14%3a54Z&CORRELATIONID=ab6833a9ee4c1&ACK=Failure&VERSION=78%2e0&BUILD=54677068&L_ERRORCODE0=10527&L_SHORTMESSAGE0=Invalid%20Data&L_LONGMESSAGE0=This%20transaction%20cannot%20be%20processed%2e%20Please%20enter%20a%20valid%20credit%20card%20number%20and%20type%2e&L_SEVERITYCODE0=Error&AMT=1%2e90&CURRENCYCODE=USD"	string
            string sAck = "", sAVSResponse = "", sTransactionId = "", sCVVMAtch = "", message = "";
            PaypalRequestBuilder.ParseResponse(response, (name, value) =>
            {
                if (String.Compare(name, "ACK", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAck = value;
                if (String.Compare(name, "AVSCODE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAVSResponse = value;
                if (String.Compare(name, "TRANSACTIONID", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sTransactionId = value;
                if (String.Compare(name, "CVV2MATCH", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sCVVMAtch = value;
                if (String.Compare(name, "L_LONGMESSAGE0", StringComparison.InvariantCultureIgnoreCase) == 0)
                    message = value;
            });

            if ((String.Compare(sAck, "Success", false) != 0) && (String.Compare(sAck, "SuccessWithWarning", false) == 0))
            {
                return new PaypalAuthTransactionResponse()
                {
                    success = false,
                    message = message
                };
            }

            return new PaypalAuthTransactionResponse()
            {
                success = true,
                message = null,
                avsResponse = sAVSResponse,
                cvv2Response = sCVVMAtch,
                transactionId = sTransactionId
            };
        }

        public override async Task<AuthorizerResponse> RefundTransaction(AuthorizerRefundTransaction transaction)
        {
            var originalRp = transaction.originalTransactionResponse as ITransactionId;

            PaypalRequestBuilder requestBuilder = new PaypalRequestBuilder(_clientConfig.userName, _clientConfig.password, _clientConfig.signature);
            requestBuilder.Append("METHOD", "RefundTransaction", 1000);
            requestBuilder.Append("REFUNDTYPE", "Partial", 1000);
            requestBuilder.Append("AMT", transaction.amount.amount.ToString("0.00"), 1000);
            requestBuilder.Append("TRANSACTIONID", originalRp.transactionId, 1000);

            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint, requestBuilder.ToString(), "application/x-www-form-urlencoded");
            AuthorizerResponse r = IsError(statusCode);
            if (r != null)
                return r;
            string sAck = "", message = "";
            PaypalRequestBuilder.ParseResponse(response, (name, value) =>
            {
                if (String.Compare(name, "ACK", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAck = value;
                if (String.Compare(name, "L_LONGMESSAGE0", StringComparison.InvariantCultureIgnoreCase) == 0)
                    message = value;
            });
            if ((String.Compare(sAck, "Success", false) != 0) && (String.Compare(sAck, "SuccessWithWarning", false) == 0))
            {
                return new PaypalRefundTransactionResponse()
                {
                    success = false,
                    message = message
                };
            }

            return new PaypalRefundTransactionResponse()
            {
                success = true,
                message = null,
            };
        }

        public override async Task<AuthorizerResponse> SaleTransaction(AuthorizerSaleTransaction transaction)
        {
            CCPaymentInfo paymentInfo = transaction.paymentInfo as CCPaymentInfo;
            
            string sExpDate = paymentInfo.card.ccExpMonth.ToString("00");
            if (paymentInfo.card.ccExpYear < 2000) 
                paymentInfo.card.ccExpYear += 2000;
            sExpDate += paymentInfo.card.ccExpYear.ToString();

            PaypalRequestBuilder requestBuilder = new PaypalRequestBuilder(_clientConfig.userName, _clientConfig.password, _clientConfig.signature);
            requestBuilder.Append("METHOD", "DoDirectPayment", 1000);
            requestBuilder.Append("PAYMENTACTION", "Sale", 1000);
            requestBuilder.Append("IPADDRESS", "127.0.0.1", 15);
            requestBuilder.Append("ACCT", paymentInfo.card.ccNum, 1000);
            requestBuilder.Append("EXPDATE", sExpDate, 1000);
            requestBuilder.Append("CVV2", paymentInfo.card.cvv, 1000);

            requestBuilder.Append("FIRSTNAME", paymentInfo.billingAddress?.firstName, 25);
            requestBuilder.Append("LASTNAME", paymentInfo.billingAddress?.lastName, 25);
            requestBuilder.Append("STREET", paymentInfo.billingAddress?.address1, 100);
            requestBuilder.Append("STREET2", paymentInfo.billingAddress?.address2, 100);
            requestBuilder.Append("CITY", paymentInfo.billingAddress?.city, 40);
            requestBuilder.Append("STATE", paymentInfo.billingAddress?.state, 40);
            requestBuilder.Append("ZIP", paymentInfo.billingAddress?.postalCode, 40);
            requestBuilder.Append("COUNTRYCODE", paymentInfo.billingAddress?.country, 2);

            requestBuilder.Append("AMT", transaction.amount.amount.ToString("0.00"), 40);
            requestBuilder.Append("CURRENCYCODE", "USD", 40);
            //requestBuilder.Append("INVNUM", "PR_inv1", 40);

/*
            requestBuilder.Append("SHIPTONAME", sShFirst + " " + sShLast, 25);
            requestBuilder.Append("SHIPTOSTREET", sShLine1, 100);
            requestBuilder.Append("SHIPTOSTREET2", sShLine2, 100);
            requestBuilder.Append("SHIPTOCITY", sShCity, 40);
            requestBuilder.Append("SHIPTOSTATE", sShState, 40);
            requestBuilder.Append("SHIPTOZIP", sShZip, 40);
            requestBuilder.Append("SHIPTOCOUNTRY", clsGlobal.GetCountryCode(sShCountry), 2);

*/

            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint, requestBuilder.ToString(), "application/x-www-form-urlencoded");
            AuthorizerResponse r = IsError(statusCode);
            if (r != null)
                return r;

            //"TIMESTAMP=2020%2d07%2d09T00%3a10%3a00Z&CORRELATIONID=9a8182e401a35&ACK=Success&VERSION=78%2e0&BUILD=54677068&AMT=1%2e90&CURRENCYCODE=USD&AVSCODE=A&CVV2MATCH=M&TRANSACTIONID=2AX69342E26784917"
            //"TIMESTAMP=2020%2d07%2d09T00%3a14%3a54Z&CORRELATIONID=ab6833a9ee4c1&ACK=Failure&VERSION=78%2e0&BUILD=54677068&L_ERRORCODE0=10527&L_SHORTMESSAGE0=Invalid%20Data&L_LONGMESSAGE0=This%20transaction%20cannot%20be%20processed%2e%20Please%20enter%20a%20valid%20credit%20card%20number%20and%20type%2e&L_SEVERITYCODE0=Error&AMT=1%2e90&CURRENCYCODE=USD"	string
            string sAck = "", sAVSResponse = "", sTransactionId = "", sCVVMAtch = "", message = "";
            PaypalRequestBuilder.ParseResponse(response, (name, value) =>
            {
                if (String.Compare(name, "ACK", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAck = value;
                if (String.Compare(name, "AVSCODE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAVSResponse = value;
                if (String.Compare(name, "TRANSACTIONID", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sTransactionId = value;
                if (String.Compare(name, "CVV2MATCH", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sCVVMAtch = value;
                if (String.Compare(name, "L_LONGMESSAGE0", StringComparison.InvariantCultureIgnoreCase) == 0)
                    message = value;
            });

            if ((String.Compare(sAck, "Success", false) != 0) && (String.Compare(sAck, "SuccessWithWarning", false) == 0))
            {
                return new PaypalSaleTransactionResponse()
                {
                    success = false,
                    message = message
                };
            }

            return new PaypalSaleTransactionResponse()
            {
                success = true,
                message = null,
                avsResponse = sAVSResponse,
                cvv2Response = sCVVMAtch,
                transactionId = sTransactionId
            };
        }

        public override Task<AuthorizerResponse> StandaloneRefundTransaction(AuthorizerStandaloneRefundTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override async Task<AuthorizerResponse> VoidTransaction(AuthorizerVoidTransaction transaction)
        {
            //Only transaction from Auth can be voided. Sale can only be refunded. 
            var originalRp = transaction.originalTransactionResponse as ITransactionId;

            PaypalRequestBuilder requestBuilder = new PaypalRequestBuilder(_clientConfig.userName, _clientConfig.password, _clientConfig.signature);
            if (transaction.originalTransactionResponse is PaypalSaleTransactionResponse)
            {
                requestBuilder.Append("METHOD", "RefundTransaction", 1000);
                requestBuilder.Append("REFUNDTYPE", "Full ", 1000);
                //requestBuilder.Append("AMT", transaction.originalAmount.amount.ToString("0.00"), 1000); //no need for full refund
            }
            if (transaction.originalTransactionResponse is PaypalAuthTransactionResponse)
            {
                requestBuilder.Append("METHOD", "DoVoid", 1000);
            }
            
            requestBuilder.Append("TRANSACTIONID", originalRp.transactionId, 1000);

            (string response, HttpStatusCode statusCode) = await SendPostRequest(_endpoint, requestBuilder.ToString(), "application/x-www-form-urlencoded");
            AuthorizerResponse r = IsError(statusCode);
            if (r != null)
                return r;
            string sAck = "", message = "";
            PaypalRequestBuilder.ParseResponse(response, (name, value) =>
            {
                if (String.Compare(name, "ACK", StringComparison.InvariantCultureIgnoreCase) == 0)
                    sAck = value;
                if (String.Compare(name, "L_LONGMESSAGE0", StringComparison.InvariantCultureIgnoreCase) == 0)
                    message = value;
            });
                
            if ((String.Compare(sAck, "Success", false) != 0) && (String.Compare(sAck, "SuccessWithWarning", false) == 0))
            {
                return new PaypalVoidTransactionResponse()
                {
                    success = false,
                    message = message
                };
            }

            return new PaypalVoidTransactionResponse()
            {
                success = true,
                message = null,
            };
        }
    }
}

