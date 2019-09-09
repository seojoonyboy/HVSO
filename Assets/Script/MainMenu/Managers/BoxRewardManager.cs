using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BestHTTP;
using dataModules;


public class BoxRewardManager : MonoBehaviour
{
    [SerializeField] Transform boxObject;
    [SerializeField] CardDictionaryManager cardDic;
    [SerializeField] TMPro.TextMeshProUGUI supplyStore;
    [SerializeField] TMPro.TextMeshProUGUI storeTimer;
    // Start is called before the first frame update
    Transform hudCanvas;

    AccountManager accountManager;
    NetworkManager networkManager;

    public UnityEvent OnBoxLoadFinished = new UnityEvent();

    void Awake() {
        accountManager = AccountManager.Instance;
        hudCanvas = transform.parent;
        accountManager.userResource.LinkTimer(storeTimer);


        cardDic.SetCardsFinished.AddListener(() => transform.Find("ShowBox").gameObject.SetActive(true));
        OnBoxLoadFinished.AddListener(() => accountManager.RefreshInventories(OnInventoryRefreshFinished));
    }

    public void SetBoxObj() {
        boxObject.Find("SupplyGauge/Value").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userResource.supply * 0.01f;
        boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supply.ToString() + "/100";
        supplyStore.text = AccountManager.Instance.userResource.supplyStore.ToString() + "/200";
        if (AccountManager.Instance.userResource.supplyBox > 0) {
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(true);
            boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supplyBox.ToString();
        }
        else
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(false);
    }

    public void OpenBox() {
        if (AccountManager.Instance.userResource.supplyBox <= 0) return;
        WaitReward();
    }

    

    void WaitReward() {
        accountManager.RequestRewardInfo((req, res) => {
            if (res != null) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);

                    accountManager.rewardList = result;
                    accountManager.SetRewardInfo(result);
                    OnBoxLoadFinished.Invoke();
                    accountManager.RequestUserInfo(accountManager.SetSignInData);
                }
            }
        });
    }

    public void GetResult() {
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("OpenBox").gameObject.SetActive(true);
        StartCoroutine(ShowRewards());
    }

    IEnumerator ShowRewards() {
        SetRewards(accountManager.rewardList);
        Transform boxParent = transform.Find("OpenBox");
        boxParent.gameObject.SetActive(true);
        iTween.ScaleTo(boxParent.GetChild(0).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(1).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(2).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.5f);
        cardDic.SetToHumanCards();
        transform.Find("ExitButton").gameObject.SetActive(true);
    }

    public void ExitBoxOpen() {
        Transform boxParent = transform.Find("OpenBox");
        for (int i = 0; i < 4; i++) {
            boxParent.GetChild(i).localScale = Vector3.zero;
        }
        boxParent.gameObject.SetActive(false);
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ShowBox/Text").gameObject.SetActive(true);
        transform.Find("ExitButton").gameObject.SetActive(false);
        boxParent.GetChild(0).GetChild(1).gameObject.SetActive(false);
        boxParent.GetChild(1).GetChild(1).gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Card").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Card").GetChild(1).gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Resource").gameObject.SetActive(false);
        for (int i = 0; i < 3; i++) {
            boxParent.GetChild(2).GetChild(i).gameObject.SetActive(false);
            boxParent.GetChild(3).Find("Resource").GetChild(i).gameObject.SetActive(false);
        }
        SetBoxObj();
    }



    public void SetRewards(RewardClass[] rewardList) {
        Transform boxParent = transform.Find("OpenBox");
        boxParent.GetChild(0).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[0].item);
        if (rewardList[0].amount > 0) {
            boxParent.GetChild(0).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(0).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[0].amount.ToString();
        }
        boxParent.GetChild(1).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[1].item);
        if (rewardList[1].amount > 0) {
            boxParent.GetChild(1).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(1).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[1].amount.ToString();
        }

        boxParent.GetChild(2).Find(rewardList[2].item).gameObject.SetActive(true);
        boxParent.GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[2].amount.ToString();

        if (rewardList[3].type == "card"){
            boxParent.GetChild(3).Find("Card").gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Card").GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[3].item);
            if (rewardList[3].amount > 0) {
                boxParent.GetChild(3).Find("Card").GetChild(1).gameObject.SetActive(true);
                boxParent.GetChild(3).Find("Card").GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
            }
        }
        else {
            boxParent.GetChild(3).Find("Resource").gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find(rewardList[3].item).gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
        }
    }


    public void OnInventoryRefreshFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null) {
            if (response.StatusCode == 200 || response.StatusCode == 304) {
                var result = JsonReader.Read<MyCardsInfo>(response.DataAsText);

                accountManager.myCards = result.cardInventories;
                accountManager.SetCardData();
                accountManager.SetHeroInventories(result.heroInventories);
                cardDic.SetCardsFinished.Invoke();
            }
        }
    }

}

public class RewardClass {
    public string item;
    public int amount;
    public string type;
}