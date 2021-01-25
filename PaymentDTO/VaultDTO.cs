using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentDTO.Vault
{
    public enum ERRORCODE { NOTFOUND = 10 };
    public enum AchAccountType { CHECKING = 0, SAVING = 1 };
    public enum CCType { UNKNOWN = 0, VISA = 5, MASTERCARD = 5, AMEX = 3, DISCOVER = 6 }

    public class Vault
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
    public class CreditCard
    {
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
    public class Ach
    {
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
    
    public class VaultRequest : AuthroizedRequestDTO
    {
        public string vaultId { get; set; }
    }
    public class VaultSequenceRequest : AuthroizedRequestDTO
    {
        public string vaultId { get; set; }
        public int sequence { get; set; } = 1;
    }
    public class GetAllCCResponse : ResponseDTO
    {
        public IEnumerable<CreditCard> data;
    }
    public class GetAllAchResponse : ResponseDTO
    {
        public IEnumerable<Ach> data;
    }
    public class GetVaultResponse : ResponseDTO
    {
        public Vault data;
    }
    public class GetCCResponse : ResponseDTO
    {
        public CreditCard data;
    }
    public class GetAchResponse : ResponseDTO
    {
        public Ach data;
    }

    public class UpdateVaultRequest : AuthroizedRequestDTO
    {
        public Vault data { get; set; }
    }
    public class UpdateCCRequest : AuthroizedRequestDTO
    {
        public string vaultId { get; set; }
        public CreditCard data { get; set; }
    }
    public class UpdateAchRequest : AuthroizedRequestDTO
    {
        public string vaultId { get; set; }
        public Ach data { get; set; }
    }
}
