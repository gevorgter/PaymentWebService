using CommonDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentDTO.Payment
{
    public enum ERRORCODE { NONE = 0, FAIL = 10 };
    public class TransactionRequestDTO : AuthroizedRequestDTO
    {
        public string source { get; set; }
    }

    public class SaleRequestDTO: TransactionRequestDTO, ISchedulingInfo, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; }
        public PaymentInfo paymentInfo { get; set; }
        public SchedulingInfo schedulingInfo { get; set; }
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
        public string additionalInfo { get; set; }
    }

    public class SaleResponseDTO: ResponseDTO
    {
        public Amount amount {get;set;}
        public string requestId { get; set; }
        public int? trId { get; set; }
        public int? scheduledTrId { get; set; }
    }

    public class AuthRequestDTO : TransactionRequestDTO, ISchedulingInfo, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; }
        public PaymentInfo paymentInfo { get; set; }
        public SchedulingInfo schedulingInfo { get; set; }
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
        public string additionalInfo { get; set; }
    }

    public class AuthResponseDTO : ResponseDTO
    {
        public Amount amount { get; set; }
        public string requestId { get; set; }
        public int? trId { get; set; }
        public int? scheduledTrId { get; set; }
    }

    public class VoidRequestDTO : TransactionRequestDTO
    {
        public string originalTrId { get; set; }
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
    }
    public class VoidResponseDTO : ResponseDTO
    {
        public string requestId { get; set; }
        public int? trId { get; set; }
    }

    public class RefundRequestDTO : TransactionRequestDTO
    {
        public Amount amount { get; set; }
        public string originalTrId { get; set; }
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
    }

    public class RefundResponseDTO : ResponseDTO
    {
        public string requestId { get; set; }
        public int? trId { get; set; }
    }

    public class SubscribeToEventRequestDTO : AuthroizedRequestDTO
    {
        public EVENTTYPE eventType { get; set; }
        public int eventSubType { get; set; }
        public string eventId { get; set; }
        public HOOKTYPE hookType { get; set; }
        public string hookDataJson { get; set; }
    }

    public class SubscribeToEventResponseDTO : ResponseDTO
    {
        public int subscriptionId { get; set; }
    }
}
