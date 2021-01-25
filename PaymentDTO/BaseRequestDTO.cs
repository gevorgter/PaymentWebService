using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentDTO
{
    public enum ERRORCODE { NONE = 0, GENERALERROR = 1, AUTHTOKENEXPIRED = 2, INVALIDAUTH = 3 };

    public class RequestDTO
    {

    }

    public class ResponseDTO
    {
        public int errorCode { get; set; } = (int)ERRORCODE.NONE;
        public IEnumerable<string> errors { get; set; }
    }

    public class AuthroizedRequestDTO : RequestDTO
    {
        public string authTocken { get; set; }
        public int accountId { get; set; }
        public string requestId { get; set; }
    }
}
