using CommonDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public interface ITransactionId
    {
        public string transactionId { get; set; }
    }
    public class PaypalTransactionResponse : AuthorizerResponse
    {

    }
    public class PaypalSaleTransactionResponse : PaypalTransactionResponse, ITransactionId
    {
        public string transactionId { get; set; }
        public string avsResponse { get; set; }
        public string cvv2Response { get; set; }
    }
    public class PaypalAuthTransactionResponse : PaypalTransactionResponse, ITransactionId
    {
        public string transactionId { get; set; }
        public string avsResponse { get; set; }
        public string cvv2Response { get; set; }
    }
    public class PaypalVoidTransactionResponse : PaypalTransactionResponse
    {
    }
    public class PaypalRefundTransactionResponse : PaypalTransactionResponse
    {
    }

}
