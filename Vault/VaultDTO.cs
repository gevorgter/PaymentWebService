using CommonDTO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace VaultDTO
{
    public class Id
    {
        public int id { get; set; } = -1;
    }

    public class CustomerDBDTO : Id
    {
        public string vaultId { get; set; }
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

    public class CreditCardDBDTO
    {
        public int vaultCustomerId { get; set; } = -1;
        public int sequence { get; set; } = 1;
        public string ccNum { get; set; } //412564646546546
        public int ccExpMonth { get; set; } // 0 - January
        public int ccExpYear { get; set; } //2020
        public CCType ccType { get; set; }
        //Billing address
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }

    public class AchDBDTO
    {
        public int vaultCustomerId { get; set; } = -1;
        public int sequence { get; set; } = 1;
        public string aba { get; set; }
        public string dda { get; set; }
        public AchAccountType accountType { get; set; }

        //Billing address
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }
}
