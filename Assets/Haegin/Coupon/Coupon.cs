using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Haegin
{
    public class Coupon
    {
        public enum RedeemResultCode
        {
            SuccessfullyRedeemed,
            CouponNotExist,
            CouponExpired,
            CouponNotAvailable,
            CouponMaxUsed,
            CouponUserUsed,
            NetworkErrorTryAgain
        } 

        public delegate void OnRedeemResult(RedeemResultCode redeemResult, byte[] data);

        public static void RedeemCoupon(string couponId, OnRedeemResult callback)
        {
            WebClient.GetInstance().RequestCoupon(couponId, (WebClient.ErrorCode error, Result result, byte[] data) =>
            {
                if(error == WebClient.ErrorCode.SUCCESS)
                {
                    switch (result)
                    {
                        case Result.OK:
                            callback(RedeemResultCode.SuccessfullyRedeemed, data);
                            return;
                        case Result.CouponExpired:
                            callback(RedeemResultCode.CouponExpired, data);
                            return;
                        case Result.CouponMaxUsed:
                            callback(RedeemResultCode.CouponMaxUsed, data);
                            return;
                        case Result.CouponUserUsed:
                            callback(RedeemResultCode.CouponUserUsed, data);
                            return;
                        case Result.CouponNotExisted:
                            callback(RedeemResultCode.CouponNotExist, data);
                            return;
                        case Result.CouponNotAvailable:
                            callback(RedeemResultCode.CouponNotAvailable, data);
                            return;
                    }
                }
                callback(RedeemResultCode.NetworkErrorTryAgain, null);
            });
        }
    }
}