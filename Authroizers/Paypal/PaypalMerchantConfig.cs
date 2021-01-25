using Authorizers.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public class PaypalMerchantCCConfig : ClientAuthorizerConfig
    {
        public string userName { get; set; } = "sb-kpjis2561048_api1.business.example.com";
        public string clientId { get; set; } = "";
        public string secret { get; set; } = "";
        public string password { get; set; } = "";
        public string signature { get; set; } = "";
        public string appName { get; set; } = "";

        public override Authorizer GetAuthorizer()
        {
            return new PaypalCCAuthorizer(this);
        }
    }
}
