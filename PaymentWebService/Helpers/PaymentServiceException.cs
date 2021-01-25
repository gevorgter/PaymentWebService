using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentWebService
{
    public class PaymentServiceException : Exception
    {
        public PaymentServiceException(string msg) :
            base(msg)
        {

        }
    }
}
