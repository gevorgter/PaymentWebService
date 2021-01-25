using CommonDTO;
using System;

namespace Authorizers.Common
{
    public enum AuthorizerId { GiftCard = 1, CreditCard = 2, Ach = 3 };
    public enum PaymentType { Cash = 0, GiftCard = 1, CreditCard = 2, Ach = 3 };

    public abstract class ClientAuthorizerConfig
    {
        public abstract Authorizer GetAuthorizer();
    }

    
}
