using Authorizers.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public class PaypalMerchantCCConfig : ClientAuthorizerConfig
    {
        public string userName { get; set; } = "sb-kpjis2561048_api1.business.example.com";
        public string clientId { get; set; } = "ARvhTPulD9_s2zDn1b_ItSfZtMXWfkS2eALaw0jA-ytmaH-pYLaS8pzCrRNAG-QMek_d-zww7QeNMZcO";
        public string secret { get; set; } = "EJFtIHUGLh45nUmPk8URfn1Qu14RvwShL1YCXqZHaB2xbx1ghwXa59y9VwC8YExCh9mtaOb86Lpj4WqV";
        public string password { get; set; } = "VEFTZNVMTVP9K8WC";
        public string signature { get; set; } = "AdqEqpafX86eR0leV3eRlFRQ.QViAAOk9ek4s-9SJwoiG5tGqQswRd9H";
        public string appName { get; set; } = "Default Application";

        public override Authorizer GetAuthorizer()
        {
            return new PaypalCCAuthorizer(this);
        }
    }
}
