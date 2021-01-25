using Authorizers.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public class GoeMerchantCCConfig : ClientAuthorizerConfig
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }

        public override Authorizer GetAuthorizer()
        {
            return new GoeMerchantCCAuthorizer(this);
        }

    }

    public class GoeMerchantACHConfig : ClientAuthorizerConfig
    {
        public string merchantKey { get; set; }
        public string processorId { get; set; }

        public override Authorizer GetAuthorizer()
        {
            return new GoeMerchantACHAuthorizer(this);
        }
    }
}
