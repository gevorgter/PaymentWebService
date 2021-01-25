using CommonDTO;
using System;
using System.ComponentModel;

namespace PaymentDB
{
    public class IdDBDTO
    {
        public int id { get; set; }
    }

    public class TransactionDBDTO : IdDBDTO
    {
        public TransactionType trType { get; set; }
        public int lockId { get; set; }
        public int parentId { get; set; } = -1;
        public int accountId { get; set; }
        public DateTime createdDate { get; set; }
        public int status { get; set; }
        public string statusMessage { get; set; }
        public decimal originalAmount { get; set; }
        public decimal amountLeft { get; set; }
        public string trJson { get; set; }
        public string authorizerResponseJson { get; set; }
    }

    public class ScheduledTransactionDBDTO : IdDBDTO
    {
        public int accountId { get; set; }
        public int disabled { get; set; }
        public int errorCount { get; set; }
        public int maxErrorCount { get; set; }
        public DateTime startOn { get; set; }
        public DateTime endOn { get; set; }
        public DateTime nextRunOn { get; set; }
        public string skipDates { get; set; }
        public ScheduleType interval { get; set; }
        public string trJson { get; set; }
    }
}
