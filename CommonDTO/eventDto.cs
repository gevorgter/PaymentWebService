using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDTO
{
    public enum EVENTTYPE { NONE = 0, TRANSACTION = 1 };
    public enum TRANSACTIONEVENTSUBTYPE { REFUND = 1, SALE = 2, AUTH = 3, VOID = 4 };
    public enum HOOKTYPE { URL = 1 }

    public abstract class EventData
    {

    }

    public class SaleEventData : EventData
    {
        public int transactionId;
        public Amount amount;
    }
    public class AuthEventData : EventData
    {
        public int transactionId;
        public Amount amount;
    }
    public class RefundEventData : EventData
    {
        public string originalTransactionId;
        public string refundTransactionId;
        public Amount amount;
    }
    public class VoidEventData : EventData
    {
        public string originalTransactionId;
        public int transactionId;
    }

    public abstract class HookData
    {
       
    }

    public class UrlHookData : HookData
    {
        public string url;
    }
    
}
