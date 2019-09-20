using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_ANDROID
#if !USE_ONESTORE_IAP
using SA.Android.Vending.Billing;
#endif
#elif UNITY_IOS
using SA.iOS.StoreKit;
#endif
namespace Haegin
{
    public class IAPProduct
    {
        public string productId { get; private set; }
        public string title { get; private set; }
        public string price { get; private set; }
        public string description { get; private set; }
        public string currencyCode { get; private set; }
        public float priceOnly { get; private set; }

        /*  인벤토리 서비스 사용시 
#if UNITY_STANDALONE && USE_STEAM
		public string type { get; private set; }
#endif
        */
#if UNITY_IOS || UNITY_TVOS
        public IAPProduct(ISN_SKProduct prod)
        {
            productId = prod.ProductIdentifier;
            title = prod.LocalizedTitle;
            price = prod.LocalizedPrice;
            description = prod.LocalizedDescription;
            currencyCode = prod.PriceLocale.CurrencyCode;
            priceOnly = prod.Price;
        }
#elif UNITY_ANDROID
#if USE_ONESTORE_IAP
        public IAPProduct(OneStore.ProductDetail prod)
        {
#if MDEBUG
            Debug.Log(prod.ToString());
#endif
            productId = prod.productId;
            title = prod.title;
            price = "￦ " + prod.price;
            description = prod.title;
            currencyCode = "KRW";
            priceOnly = 0; 
            try { priceOnly = (float)int.Parse(prod.price); } catch { }
        }
#else
        public IAPProduct(AN_Product prod)
        {
#if MDEBUG
            Debug.Log(prod.OriginalJson);
#endif
            productId = prod.ProductId;
            title = prod.Title;
            price = prod.Price;
            description = prod.Description;
            currencyCode = prod.PriceCurrencyCode;
            priceOnly = (float)(prod.PriceAmountMicros) / 1000000.0f;
        }
#endif
#elif UNITY_STANDALONE && USE_STEAM
		public IAPProduct(string pid, string t, string p, string d)
		{
			productId = pid;
			title = t;
			price = p;
			description = d;
		}

		/*  인벤토리 서비스 사용시 
		public IAPProduct(string pid, string t, string p, string d, string c, string ty)
		{
			productId = pid;
			title = t;
			price = p;
			description = d;
			currencyCode = c;
			type = ty;
		}
        */
#endif

        public override string ToString()
        {
            return string.Format("[IAPProduct[: productId: {0}, title: {1}, price: {2}, description: {3}, priceCurrencyCode: {4}]",
                productId, title, price, description, currencyCode);
        }

    }
}
