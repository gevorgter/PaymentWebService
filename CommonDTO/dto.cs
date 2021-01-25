using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CommonDTO
{
    public enum Currency { DEFAULT = 0, USD = 1, CAD = 2 };
    public enum ScheduleType { ONETIME = 0, DAILY = 10, WEEKLY = 20, BIWEEKLY = 30, TRIWEEKLY = 40, MONTLY = 50, BIMONTHLY = 60, QUATERLY = 70, SEMIANNUAL = 80, ANNUAL = 90 };
    public enum AchAccountType { CHECKING = 0, SAVING = 1 };
    public enum TransactionType { SALE = 10, AUTHORIZE = 20, REFUND = 30, STANDALONE_REFUND=40, VOID = 100 }
    public enum CCType { UNKNOWN = 0, VISA = 5, MASTERCARD = 5, AMEX = 3, DISCOVER = 6 }
    public enum TransactionStatus { PENDING = 0, SUCCESS = 10, REFUNDED=80, VOIDED = 90, REJECTED=100 };

    public class Address
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class Amount
    {
        public decimal amount { get; set; }
        public Currency currency { get; set; } = Currency.DEFAULT;
    }

    public class CreditCard
    {
        public string ccNum { get; set; } //412564646546546
        public int ccExpMonth { get; set; } // 0 - January
        public int ccExpYear { get; set; } //2020
        public CCType ccType { get; set; } 
        public string cvv { get; set; } //123
    }
    public class AchInfo
    {
        public string aba { get; set; }
        public string dda { get; set; }
        public AchAccountType accountType { get; set; }
        public string achCategory { get; set; }
    }

    public class SchedulingInfo
    {
        public bool runTransactionTodayAsWell { get; set; } = false;
        public ScheduleType scheduleType { get; set; }
        public DateTime startOn { get; set; }
        public DateTime endOn { get; set; } = DateTime.MinValue;
    }

    public abstract class PaymentInfo
    {

    }

    public class Vault
    {
        public string vaultId { get; set; }
        public int sequenceId { get; set; }
    }
    public class GiftCard
    {
        public string giftCardNum { get; set; } //412564646546546
        public string pinCode { get; set; }
    }

    public class VaultPayment : PaymentInfo
    {
        public Vault vaultInfo { get; set; }
    }
    public class CryptogramPayment : PaymentInfo
    {
        public string cryptogram { get; set; }
    }

    public class GiftCardPayment : PaymentInfo
    {
        public GiftCard giftCard {get;set;}
    }

    public class CCPaymentInfo : PaymentInfo
    {
        public CreditCard card { get; set; }
        public Address billingAddress { get; set; }
    }

    public class AchPaymentInfo : PaymentInfo
    {
        public AchInfo achInfo { get; set; }
        public Address billingAddress { get; set; }
    }

    public abstract class AuthorizerSpecificInfo
    {
    }
    public abstract class AuthorizerSpecificResponse
    {
    }
    public abstract class Transaction
    {
        [IgnoreDataMember]
        public int? id { get; set; } = null;
        [IgnoreDataMember]
        public TransactionStatus status { get; set; } = TransactionStatus.PENDING;
        [IgnoreDataMember]
        public string statusMessage { get; set; } = "";
        [IgnoreDataMember]
        public string source { get; set; }
    }

    public interface ISchedulingInfo
    {
        SchedulingInfo schedulingInfo { get; set; }
    }

    public interface IPaymentInfo
    {
        PaymentInfo paymentInfo { get; set; }
    }
    public interface IAmount
    {
        Amount amount { get; set; }
    }
    public class AdditionalFields
    {
        public int fieldId { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
    public class SaleTransaction : Transaction, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; } = null;
        public PaymentInfo paymentInfo { get; set; } = null;
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
    }
    public class AuthTransaction : Transaction, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; } = null;
        public PaymentInfo paymentInfo { get; set; } = null;
        public IEnumerable<AdditionalFields> additionalFields { get; set; } = null;
    }
    public class VoidTransaction : Transaction
    {
        public int originalTrId { get; set; }
    }

    public class RefundTranaction : Transaction
    {
        public int originalTrId { get; set; }
        public Amount amount { get; set; }
    }

    public abstract class AuthorizerTransaction
    {

    }

    public class AuthorizerAuthTransaction : AuthorizerTransaction, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; }
        public PaymentInfo paymentInfo { get; set; }
    }

    public class AuthorizerSaleTransaction: AuthorizerTransaction, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; }
        public PaymentInfo paymentInfo { get; set; }
    }
    
    public class AuthorizerStandaloneRefundTransaction : AuthorizerTransaction, IPaymentInfo, IAmount
    {
        public Amount amount { get; set; }
        public PaymentInfo paymentInfo { get; set; }
    }
    public class AuthorizerRefundTransaction : AuthorizerTransaction
    {
        public Amount amount { get; set; }
        public PaymentInfo originalPaymentInfo { get; set; }
        public AuthorizerResponse originalTransactionResponse { get; set; }
    }
    public class AuthorizerVoidTransaction : AuthorizerTransaction
    {
        public Amount originalAmount { get; set; }
        public PaymentInfo originalPaymentInfo { get; set; }
        public AuthorizerResponse originalTransactionResponse { get; set; }
    }

    public class AuthorizerResponse : AuthorizerTransaction
    {
        public bool success { get; set; }
        public string message { get; set; }
    }   
}
