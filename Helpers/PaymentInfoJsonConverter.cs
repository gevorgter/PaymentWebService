using CommonDTO;
using Crypto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Helpers
{
    public class PaymentInfoJsonConverter : Newtonsoft.Json.JsonConverter<PaymentInfo>
    {
        public bool _withEncryption = true;
        public JsonSerializer _serializer = new JsonSerializer()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public override PaymentInfo ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, [AllowNull] PaymentInfo existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var jo = Newtonsoft.Json.Linq.JObject.Load(reader);

            if (jo["vaultId"] != null)
                return jo.ToObject<VaultPayment>(_serializer); //(VaultPayment)jo.ToObject(typeof(VaultPayment));

            if (jo["cryptogram"] != null)
                return jo.ToObject<CryptogramPayment>(_serializer); //(CryptogramPayment)jo.ToObject(typeof(CryptogramPayment));

            if (jo["achInfo"] != null)
            {
                var achInfo = jo.ToObject<AchPaymentInfo>(_serializer); //(AchPaymentInfo)jo.ToObject(typeof(AchPaymentInfo));
                if (_withEncryption)
                {
                    achInfo.achInfo.aba = achInfo.achInfo.aba.Decrypt();
                    achInfo.achInfo.dda = achInfo.achInfo.dda.Decrypt();
                }
                return achInfo;
            }

            if (jo["card"] != null)
            {
                var ccInfo = jo.ToObject<CCPaymentInfo>(_serializer); //(CCPaymentInfo)jo.ToObject(typeof(CCPaymentInfo));
                if (_withEncryption)
                    ccInfo.card.ccNum = ccInfo.card.ccNum.Decrypt();
                return ccInfo;
            }

            if (jo["giftCard"] != null)
            {
                var giftInfo = jo.ToObject<GiftCardPayment>(_serializer); //(GiftCardPayment)jo.ToObject(typeof(GiftCardPayment));
                if (_withEncryption)
                    giftInfo.giftCard.giftCardNum = giftInfo.giftCard.giftCardNum.Decrypt();
                return giftInfo;
            }

            throw new Exception($"Unknow payment type {objectType.Name}");
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, [AllowNull] PaymentInfo value, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jo = null;
            if (value is CCPaymentInfo ccPaymentInfo)
            {
                if (_withEncryption)
                    ccPaymentInfo.card.ccNum = ccPaymentInfo.card.ccNum.Encrypt();
                jo = Newtonsoft.Json.Linq.JObject.FromObject(ccPaymentInfo, _serializer);
                jo.WriteTo(writer);
                return;
            }
            if (value is AchPaymentInfo achPaymentInfo)
            {
                if (_withEncryption)
                {
                    achPaymentInfo.achInfo.aba = achPaymentInfo.achInfo.aba.Encrypt();
                    achPaymentInfo.achInfo.aba = achPaymentInfo.achInfo.aba.Decrypt();
                }
                jo = Newtonsoft.Json.Linq.JObject.FromObject(achPaymentInfo, _serializer);
                jo.WriteTo(writer);
                return;
            }
            if (value is GiftCardPayment giftCardPayment)
            {
                if (_withEncryption)
                    giftCardPayment.giftCard.giftCardNum = giftCardPayment.giftCard.giftCardNum.Encrypt();
                jo = Newtonsoft.Json.Linq.JObject.FromObject(giftCardPayment, _serializer);
                jo.WriteTo(writer);
                return;
            }
            jo = Newtonsoft.Json.Linq.JObject.FromObject(value, _serializer);
            jo.WriteTo(writer);
        }
    }
}
