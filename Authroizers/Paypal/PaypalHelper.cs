using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizers
{
    public class PaypalRequestBuilder
    {
        System.Text.StringBuilder _bld;

        public PaypalRequestBuilder(string userName, string password, string signature)
        {
            _bld = new System.Text.StringBuilder(1000);
            Append("USER", userName, 1000);
            Append("PWD", password, 1000);
            Append("SIGNATURE", signature, 1000);
            Append("VERSION", "101", 1000);
        }

        public void Append(string sName, string sValue, int iMaxLen)
        {
            if (sValue == null)
                sValue = "";
            if (sValue.Length > iMaxLen)
                sValue = sValue.Substring(0, iMaxLen);
            if (_bld.Length != 0)
                _bld.Append('&');
            _bld.Append(sName);
            _bld.Append("=");
            _bld.Append(System.Web.HttpUtility.UrlEncode(sValue));
        }

        public override string ToString()
        {
            return _bld.ToString();
        }

        static readonly char[] separators = { '&', '=' };
        public static void ParseResponse(string response, Action<string,string> pr)
        {
            string[] res = response.Split(separators);
            if (res.Length % 2 != 0)
                return; //probably not a response from paypal
            for (int i = 0; i < res.Length; i+=2)
            {
                string name = res[i];
                string value = System.Web.HttpUtility.UrlDecode(res[i+1]);
                pr(name, value);
            }
        }
    }
}
