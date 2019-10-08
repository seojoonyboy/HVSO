using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BestHTTP;
using dataModules;
using Spine;
using Spine.Unity;


public class BoxRewardManager : MonoBehaviour
{
    [SerializeField] Transform boxObject;
    [SerializeField] CardDictionaryManager cardDic;
    [SerializeField] TMPro.TextMeshProUGUI supplyStore;
    [SerializeField] TMPro.TextMeshProUGUI storeTimer;
    [SerializeField] SkeletonGraphic boxSpine;
    [SerializeField] Transform additionalSupply;
    [SerializeField] MenuSceneController menuSceneController;
    // Start is called before the first frame update
    Transform hudCanvas;

    AccountManager accountManager;
    NetworkManager networkManager;

    public UnityEvent OnBoxLoadFinished = new UnityEvent();
    static bool openningBox;
    void Awake() {
        accountManager = AccountManager.Instance;
        hudCanvas = transform.parent;
        accountManager.userResource.LinkTimer(storeTimer);


        //cardDic.SetCardsFinished.AddListener(() => SetBoxAnimation());
        OnBoxLoadFinished.AddListener(() => accountManager.RefreshInventories(OnInventoryRefreshFinished));
    }

    public void SetBoxObj() {
        boxObject.Find("SupplyGauge/Value").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userResource.supply * 0.01f;
        boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supply.ToString() + "/100";
        supplyStore.text = AccountManager.Instance.userResource.supplyStore.ToString();
        if (AccountManager.Instance.userResource.supplyBox > 0) {
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(true);
            boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supplyBox.ToString();
        }
        else
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(false);
        additionalSupply.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.additionalPreSupply.ToString();
    }

    public void OpenBox() {
        if (openningBox) return;
        if (AccountManager.Instance.userResource.supplyBox <= 0) return;
        openningBox = true;
        WaitReward();
    }

    public void SetBoxAnimation() {
        transform.Find("ShowBox").gameObject.SetActive(true);
        
        boxSpine.Initialize(true);
        boxSpine.Update(0);
        boxSpine.AnimationState.SetAnimation(0, "01.START", false);
        boxSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
    }
    

    void WaitReward() {
        accountManager.RequestRewardInfo((req, res) => {
            if (res != null) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);

                    accountManager.rewardList = result;
                    accountManager.SetRewardInfo(result);
                    SetBoxAnimation();
                    OnBoxLoadFinished.Invoke();
                    accountManager.RequestUserInfo(accountManager.SetSignInData);
                }
            }
        });
    }

    public void GetResult() {
        transform.Find("OpenBox").gameObject.SetActive(true);
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        boxSpine.AnimationState.SetAnimation(2, "03.TOUCH", false);
        StartCoroutine(ShowRewards());
    }

    IEnumerator ShowRewards() {
        SetRewards(accountManager.rewardList);
        Transform boxParent = transform.Find("OpenBox");
        Transform effects = transform.Find("EffectSpines");
        yield return new WaitForSeconds(1.2f);
        effects.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        effects.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        iTween.ScaleTo(boxParent.GetChild(0).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
        effects.GetChild(2).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        iTween.ScaleTo(boxParent.GetChild(1).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
        effects.GetChild(3).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        iTween.ScaleTo(boxParent.GetChild(2).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.05f);
        if(boxParent.GetChild(3).Find("Card").gameObject.activeSelf)
            iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
        else
            iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.4f);
        //cardDic.SetToHumanCards();
        transform.Find("ExitButton").gameObject.SetActive(true);
    }

    public void ExitBoxOpen() {
        Transform boxParent = transform.Find("OpenBox");
        Transform effects = transform.Find("EffectSpines");
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
        boxParent.GetChild(3).Find("Card/GetCrystal").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Resource").gameObject.SetActive(false);
        for (int i = 0; i < 3; i++) {
            boxParent.GetChild(2).GetChild(i).gameObject.SetActive(false);
            boxParent.GetChild(3).Find("Resource").GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < 4; i++)
            effects.GetChild(i).gameObject.SetActive(false);
        SetBoxObj();
        openningBox = false;
    }

    public void SetRewards(RewardClass[] rewardList) {
        Transform boxParent = transform.Find("OpenBox");
        Transform effects = transform.Find("EffectSpines");
        boxParent.GetChild(0).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[0].item);

        effects.GetChild(0).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(0).GetComponent<SkeletonGraphic>().Update(0);
        if (accountManager.allCardsDic[rewardList[0].item].type == "unit")
            effects.GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
        else
            effects.GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
        effects.GetChild(0).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
        if (rewardList[0].amount > 0) {
            boxParent.GetChild(0).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(0).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[0].amount.ToString();
        }
        else
            CheckNewCardList(rewardList[0].item);

        boxParent.GetChild(1).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[1].item);
        effects.GetChild(1).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(1).GetComponent<SkeletonGraphic>().Update(0);
        if (accountManager.allCardsDic[rewardList[1].item].type == "unit")
            effects.GetChild(1).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
        else
            effects.GetChild(1).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
        effects.GetChild(1).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
        if (rewardList[1].amount > 0) {
            boxParent.GetChild(1).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(1).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[1].amount.ToString();
        }
        else
            CheckNewCardList(rewardList[1].item);

        boxParent.GetChild(2).Find(rewardList[2].item).gameObject.SetActive(true);
        boxParent.GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[2].amount.ToString();
        effects.GetChild(2).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(2).GetComponent<SkeletonGraphic>().Update(0);
        effects.GetChild(2).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);

        effects.GetChild(3).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(3).GetComponent<SkeletonGraphic>().Update(0);
        if (rewardList[3].type == "card"){
            boxParent.GetChild(3).Find("Card").gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Card").GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[3].item);
            if (accountManager.allCardsDic[rewardList[3].item].type == "unit")
                effects.GetChild(3).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
            else
                effects.GetChild(3).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
            if (rewardList[3].amount > 0) {
                boxParent.GetChild(3).Find("Card/GetCrystal").gameObject.SetActive(true);
                boxParent.GetChild(3).Find("Card/GetCrystal").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
            }
            else
                CheckNewCardList(rewardList[3].item);
        }
        else {
            boxParent.GetChild(3).Find("Resource").gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find(rewardList[3].item).gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
            effects.GetChild(3).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("4.item");
        }
        effects.GetChild(3).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
    }


    public void OnInventoryRefreshFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null) {
            if (response.StatusCode == 200 || response.StatusCode == 304) {
                var result = JsonReader.Read<MyCardsInfo>(response.DataAsText);

                accountManager.myCards = result.cardInventories;
                accountManager.SetCardData();
                accountManager.SetHeroInventories(result.heroInventories);
                //cardDic.SetCardsFinished.Invoke();
            }
        }
    }

    void CheckNewCardList(string cardId) {
        CollectionCard cardData = accountManager.allCardsDic[cardId];
        if (cardData.camp == "human") {
            if (!accountManager.cardPackage.checkHumanCard.Contains(cardId)) {
                accountManager.cardPackage.checkHumanCard.Add(cardId);
                accountManager.cardPackage.rarelityHumanCardCheck[cardData.rarelity].Add(cardId);
            }
        }
        else{
            if (!accountManager.cardPackage.checkOrcCard.Contains(cardId)) {
                accountManager.cardPackage.checkOrcCard.Add(cardId);
                accountManager.cardPackage.rarelityOrcCardCheck[cardData.rarelity].Add(cardId);
            }
        }
        menuSceneController.SetCardNumbersPerDic();
    }
}

public class RewardClass {
    public string item;
    public int amount;
    public string type;
}