using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    class AuthorizerException : Exception
    {
        public AuthorizerException(string msg) :
            base(msg)
        {

        }
    }
}
