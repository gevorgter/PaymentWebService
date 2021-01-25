using CommonDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public class GoeMerchantTransactionResponse : AuthorizerResponse
    {
        public string authCode { get; set; }
        public string authResponse { get; set; }
        public string referenceNumber { get; set; }
        public string orderId { get; set; }
        public string avsResponse { get; set; }
        public string cvv2Response { get; set; }
    }

    

    public class ValidationErrors
    {
        public string key { get; set; }
        public string message { get; set; }
    }
    public class GoeResponse<T>
    {
        public bool isError { get; set; }
        public IEnumerable<string> errorMessages { get; set; }
        public bool validationHasFailed { get; set; }
        public IEnumerable<ValidationErrors> validationFailures { get; set; }
        public T data { get; set; }
    }

    public class GoeCCSaleTransaction
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }
        public string ipAddress { get; set; }

        public string transactionAmount { get; set; }
        public string cardNumber { get; set; }
        public string cardExpMonth { get; set; }
        public string cardExpYear { get; set; }
        public string cVV { get; set; }


        public string ownerName { get; set; }
        public string ownerStreet { get; set; }
        public string ownerStreet2 { get; set; }
        public string ownerCity { get; set; }
        public string ownerState { get; set; }
        public string ownerZip { get; set; }
        public string ownerCountry { get; set; }
        public string ownerEmail { get; set; }
        public string ownerPhone { get; set; }

        public string preventPartial { get; set; }
    }
    public class GoeCCSaleTransactionResponse
    {
        public string authResponse { get; set; }
        public string authCode { get; set; }
        public string referenceNumber { get; set; }
        public string isPartial { get; set; }
        public string partialId { get; set; }
        public string originalFullAmount { get; set; }
        public string partialAmountApproved { get; set; }
        public string avsResponse { get; set; }
        public string cvv2Response { get; set; }
        public string orderId { get; set; }
        public string cardDeclinedMessage { get; set; }
        public string cardDeclinedNo { get; set; }
    }

    public interface IReferenceNumber
    {
        public string referenceNumber { get; set; }
    }
    public class GoeVoidTransaction
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }

        public string refNumber { get; set; }
    }
    public class GoeVoidTransactionResponse: IReferenceNumber
    {
        public string authResponse { get; set; }
        public string referenceNumber { get; set; }
    }

    public class GoeCCCreditTransaction
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }

        public string refNumber { get; set; }
        public string transactionAmount { get; set; }
    }
    public class GoeCCCreditTransactionResponse : IReferenceNumber
    {
        public string authResponse { get; set; }
        public string referenceNumber { get; set; }
        public string creditAmount { get; set; }
    }


    public class GoeACHTransaction
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }
        public string ipAddress { get; set; }

        public string transactionAmount { get; set; }
        public string accountType { get; set; }
        public string categoryText { get; set; }
        public string aba { get; set; }
        public string dda { get; set; }


        public string ownerName { get; set; }
        public string ownerStreet { get; set; }
        public string ownerStreet2 { get; set; }
        public string ownerCity { get; set; }
        public string ownerState { get; set; }
        public string ownerZip { get; set; }
        public string ownerCountry { get; set; }
        public string ownerEmail { get; set; }
        public string ownerPhone { get; set; }

    }
    public class GoeACHTransactionResponse
    {
        public string authResponse { get; set; }
        public string authCode { get; set; }
        public string referenceNumber { get; set; }
        public string orderId { get; set; }
    }

}
