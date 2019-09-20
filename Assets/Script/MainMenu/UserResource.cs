using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haegin;

public class UserResource : MonoBehaviour {
    [HideInInspector]
    public int Jewel { get; private set; }
    public int Gold { get; private set; }

    public delegate void OnConsumeCompleted(string resourceType, int amount);

    public void OnJewelBuyButtonClick() {
        BuyReq("com_haegin_test1", (consumedType, amount) => {

        });
    }
    public void OnGoldBuyButtonClick() {
        BuyReq("com_haegin_test1", (consumedType, amount) => {

        });
    }

    private void BuyReq(string productId, OnConsumeCompleted onConsumeCompleted) {
        IAP.purchaseProduct(productId, (result, purchasedData, errorMsg) => {
            switch (result) {
                case IAP.PurchaseResultCode.PURCHASE_SUCCESS:
                    //GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchased);
#if MDEBUG
                    Debug.Log("Unity : item purchased..");
#endif
                    // 데이터 갱신
                    //
                    // purchasedData 에 byte[] 로 구매된 게임내 아이템 정보가 넘어옵니다. 이 정보를 이용해서 업데이트 하시면 되겠습니다. 
                    //
                    if (purchasedData != null) {
#if MDEBUG
                        Debug.Log("PurchasedData Length = " + purchasedData.Length);
#endif
                        //
                        //   게임별로 네트워크 프로토콜 받을 때, PurchasedInfo class도 받아서 사용하시면 될 것 같습니다.
                        // 
                        PurchasedInfo purchasedInfo = PurchasedInfo.Deserialize(purchasedData);
#if MDEBUG
                        Debug.Log("Purchased Info : " + purchasedInfo.ProductId + ", " + purchasedInfo.TransactionId);
#endif
                    }
#if MDEBUG
                    else
                        Debug.Log("PurchasedData Length = " + 0);
#endif
                    break;
                case IAP.PurchaseResultCode.PURCHASE_FAIL:
                    //GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.FailedToPurchase);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_USER_CANCEL:
                    //GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Cancelled);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_INVALID_RECEIPT:
                    //GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.VerificationFailed);
                    break;
            }
        });
    }
}
