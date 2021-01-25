using Authorizers.Common;
using System;

namespace Authorizers
{
    public class GiftCardClientConfig: ClientAuthorizerConfig
    {
        public int accountId { get; set; }
        public string password { get; set; }

        public override Authorizer GetAuthorizer()
        {
            return new GiftCardAuthorizer(this);
        }
    }
}
