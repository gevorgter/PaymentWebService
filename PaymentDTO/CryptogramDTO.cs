using CommonDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentDTO.Cryptogram
{
    public enum ERRORCODE { CRYPTOGRAMNOTFOUND = 10 };

    public class PutObjectRequestDTO : AuthroizedRequestDTO
    {
        public object payload { get; set; }
        public bool encrypted { get; set; } = false;
        public int ttlInMinutes { get; set; } = 10;
        public int retriveCount { get; set; } = 1;
    }

    public class PutObjectResponseDTO : ResponseDTO
    {
        public string cryptogram { get; set; }
    }

    public class GetObjectRequestDTO : AuthroizedRequestDTO
    {
        public string cryptogram { get; set; }
    }

    public class GetObjectResponseDTO : ResponseDTO
    {
        public string payload { get; set; }
    }

    public class PayloadDTO
    {
        public PaymentInfo paymentInfo { get; set; }
    }
}
