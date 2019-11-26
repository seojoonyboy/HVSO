using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BestHTTP;
using dataModules;
using Spine;
using Spine.Unity;
using System;

public class BoxRewardManager : MonoBehaviour {
    [SerializeField] Transform boxObject;
    [SerializeField] TMPro.TextMeshProUGUI supplyStore;
    [SerializeField] TMPro.TextMeshProUGUI storeTimer;
    [SerializeField] SkeletonGraphic boxSpine;
    [SerializeField] SkeletonGraphic boxEffect;
    [SerializeField] Transform additionalSupply;
    [SerializeField] MenuSceneController menuSceneController;
    [SerializeField] Transform targetSpine;
    [SerializeField] Transform lastPos;
    [SerializeField] GameObject skipBtn;
    // Start is called before the first frame update
    Transform hudCanvas;

    AccountManager accountManager;
    NetworkManager networkManager;

    public UnityEvent OnBoxLoadFinished = new UnityEvent();
    static bool openningBox = false;
    bool openAni = false;
    int openCount;

    void Awake() {
        accountManager = AccountManager.Instance;
        hudCanvas = transform.parent;
        accountManager.userResource.LinkTimer(storeTimer);

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);

        OnBoxLoadFinished.AddListener(() => accountManager.RequestInventories());
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
    }

    private void OnBoxOpenRequest(Enum Event_Type, Component Sender, object Param) {
        SetBoxAnimation();
        OnBoxLoadFinished.Invoke();
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
        additionalSupply.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supplyX2Coupon.ToString();
    }

    public void OpenBox() {
        if (openningBox) return;
        if (openAni) return;
        if (AccountManager.Instance.userResource.supplyBox <= 0) return;
        openningBox = true;
        accountManager.RequestRewardInfo();
    }

    public void SetBoxAnimation() {
        InitBoxObjects();
        transform.Find("ShowBox").gameObject.SetActive(true);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = "4";
        openCount = 0;
        boxSpine.Initialize(true);
        boxSpine.Update(0);
        boxSpine.AnimationState.SetAnimation(0, "01.START", false);
        boxSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
        boxEffect.Initialize(true);
        boxEffect.Update(0);
        boxEffect.gameObject.SetActive(true);
        boxEffect.AnimationState.SetAnimation(0, "00.NOANI", false);
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().Initialize();
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().boneName = "card";
        SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
        SetRewards(accountManager.rewardList);
        transform.Find("OpenBox").gameObject.SetActive(true);
    }


    public void GetBoxResult() {
        if (openAni) return;
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("OpenBox/TargetSpine").gameObject.SetActive(false);
        switch (openCount) {
            case 0:
                boxSpine.AnimationState.SetAnimation(2, "03.TOUCH1", false);
                boxEffect.AnimationState.SetAnimation(1, "01.open", false);
                boxEffect.AnimationState.AddAnimation(2, "loop", true, 1.2f);
                break;
            case 1:
                transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
                transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
                boxSpine.AnimationState.SetAnimation(2, "04.TOUCH2", false);
                boxEffect.AnimationState.SetAnimation(3, "02.open", false);
                boxEffect.AnimationState.AddAnimation(4, "loop", true, 1.0f);
                break;
            case 2:
                transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
                transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
                boxSpine.AnimationState.SetAnimation(2, "04.TOUCH2", false);
                boxEffect.AnimationState.SetAnimation(5, "02.open", false);
                boxEffect.AnimationState.AddAnimation(6, "loop", true, 1.0f);
                break;
            case 3:
                transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
                transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
                boxSpine.AnimationState.SetAnimation(2, "04.TOUCH2", false);
                boxEffect.AnimationState.SetAnimation(7, "03.open", false);
                break;
            case 4:
                StartCoroutine(BoxTotalResult());
                openCount++;
                skipBtn.SetActive(false);
                return;
            case 5:
                //CloseBoxOpen();
                return;
        }

        SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN_2);
        int count = openCount;
        openCount++;
        StartCoroutine(ShowEachReward(count));
    }

    public void SkipBoxOpen() {
        if (openAni) return;
        if (openCount == 0) {
            boxSpine.AnimationState.SetAnimation(2, "03.TOUCH1", false);
            boxEffect.AnimationState.SetAnimation(1, "01.open", false);
        }
        if (openCount > 0) {
            transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
            transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
        }
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = 0.ToString();
        SetSkipBackSpine();
        StartCoroutine(BoxTotalResult());
        openCount = 5;
        skipBtn.SetActive(false);
    }

    public void SetSkipBackSpine() {
        for (int i = 0; i < 4; i++) {
            Transform target = transform.Find("OpenBox").GetChild(i).GetChild(0);

            if (target.name.Contains("Card")) {
                if (target.gameObject.activeSelf) {
                    string cardId = target.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().cardID;
                    string aniName = "";
                    var rarelity = accountManager.allCardsDic[cardId].rarelity;
                    if (accountManager.allCardsDic[cardId].type == "unit")
                        aniName += "u_";
                    else
                        aniName += "m_";
                    if (rarelity != "common")
                        aniName += accountManager.allCardsDic[cardId].rarelity;
                    else
                        aniName = "NOANI";
                    SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
                    spine.gameObject.SetActive(true);
                    spine.Initialize(true);
                    spine.Update(0);
                    spine.AnimationState.SetAnimation(0, aniName, true);
                }
            }
            else if (target.name.Contains("Hero")) {

            }
            else {
                string type = target.GetChild(0).name;
                SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
                if (type != "supplyX2Coupon") {
                    spine.gameObject.SetActive(true);
                    spine.Initialize(true);
                    spine.Update(0);
                    spine.AnimationState.SetAnimation(0, "g_superrare", true);
                }
                else 
                    spine.gameObject.SetActive(false);
            }
        }
    }



    public void InitBoxObjects() {
        skipBtn.SetActive(true);
        Transform boxParent = transform.Find("OpenBox");
        for (int i = 0; i < 4; i++) {
            Transform reward = boxParent.GetChild(i);
            reward.gameObject.SetActive(true);
            reward.localScale = Vector3.zero;
            reward.localPosition = new Vector3(0, -480, 0);
            reward.Find("Card/GetCrystal").gameObject.SetActive(false);
            reward.Find("Card/back").gameObject.SetActive(false);
            reward.Find("Card/Name").gameObject.SetActive(true);
            reward.Find("Card/Name").localScale = Vector3.zero;
            reward.Find("Card/Rarelity").localScale = Vector3.zero;
            reward.Find("Resource").GetChild(1).gameObject.SetActive(false);
            reward.Find("Resource/back").gameObject.SetActive(false);
            reward.GetChild(0).gameObject.SetActive(false);
            SkeletonGraphic crystalSpine = reward.Find("Card/GetCrystalEffect").GetComponent<SkeletonGraphic>();
            crystalSpine.gameObject.SetActive(true);
            crystalSpine.Initialize(true);
            crystalSpine.Update(0);
        }
    }

    public void CloseBoxOpen() {
        InitBoxObjects();
        Transform boxParent = transform.Find("OpenBox");
        boxParent.gameObject.SetActive(false);
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ShowBox/Text").gameObject.SetActive(true);
        transform.Find("ExitButton").gameObject.SetActive(false);
        SetBoxObj();
        openningBox = false;
    }

    IEnumerator ShowEachReward(int count) {
        openAni = true;
        transform.Find("OpenBox").GetChild(count).Find("Card/Name").localScale = Vector3.zero;
        if (count == 0)
            yield return new WaitForSeconds(0.95f);
        else
            yield return new WaitForSeconds(0.5f);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = (4 - openCount).ToString();
        Transform target = transform.Find("OpenBox").GetChild(count).GetChild(0);
        iTween.ScaleTo(target.parent.gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.4f));
        iTween.MoveTo(target.parent.gameObject, iTween.Hash("y", targetSpine.localPosition.y, "islocal", true, "time", 0.4f));
        SkeletonGraphic targetEffect = transform.Find("OpenBox/TargetSpine").GetComponent<SkeletonGraphic>();
        yield return new WaitForSeconds(0.1f);
        targetEffect.gameObject.SetActive(true);
        targetEffect.Initialize(true);
        targetEffect.Update(0);
        targetEffect.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(0.1f);

        if (target.name == "Card") {
            if (target.gameObject.activeSelf) {
                string cardId = target.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().cardID;
                string aniName = "";
                var rarelity = accountManager.allCardsDic[cardId].rarelity;
                if (accountManager.allCardsDic[cardId].type == "unit")
                    aniName += "u_";
                else
                    aniName += "m_";
                if (rarelity != "common")
                    aniName += accountManager.allCardsDic[cardId].rarelity;
                else
                    aniName = "NOANI";
                SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
                spine.gameObject.SetActive(true);
                spine.Initialize(true);
                spine.Update(0);
                spine.AnimationState.SetAnimation(0, aniName, true);

                SoundManager soundManager = SoundManager.Instance;
                switch (rarelity) {
                    case "common":
                    case "uncommon":
                        soundManager.PlaySound(UISfxSound.BOX_NORMAL);
                        break;
                    case "rare":
                        soundManager.PlaySound(UISfxSound.BOX_RARE);
                        break;
                    case "superrare":
                        soundManager.PlaySound(UISfxSound.BOX_SUPERRARE);
                        break;
                    case "legend":
                        soundManager.PlaySound(UISfxSound.BOX_EPIC);
                        break;
                }
                target.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.allCardsDic[cardId].name;
                target.Find("Rarelity").Find(accountManager.allCardsDic[cardId].rarelity).SetAsFirstSibling();
                target.Find("Rarelity").GetChild(0).gameObject.SetActive(true);
                iTween.ScaleTo(target.Find("Rarelity").gameObject, iTween.Hash("x", 0.7f, "y", 0.7f, "islocal", true, "time", 0.3f));
                yield return new WaitForSeconds(0.2f);
                iTween.ScaleTo(target.Find("Name").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
                if (target.Find("GetCrystal").gameObject.activeSelf) {
                    yield return new WaitForSeconds(0.2f);
                    SkeletonGraphic crystalSpine = target.Find("GetCrystalEffect").GetComponent<SkeletonGraphic>();
                    crystalSpine.AnimationState.SetAnimation(0, "animation", false);
                    yield return new WaitForSeconds(0.2f);
                    target.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);
                }
            }
        }
        else if(target.name == "Hero") {

        }
        else {
            string type = target.gameObject.name;
            SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
            if (type != "supplyX2Coupon") {
                spine.gameObject.SetActive(true);
                spine.Initialize(true);
                spine.Update(0);
                spine.AnimationState.SetAnimation(0, "g_superrare", true);
            }
            else 
                spine.gameObject.SetActive(false);

            SoundManager soundManager = SoundManager.Instance;
            switch (type) {
                case "gold":
                case "supplyX2Coupon":
                    soundManager.PlaySound(UISfxSound.BOX_NORMAL);
                    break;
                case "crystal":
                    soundManager.PlaySound(UISfxSound.BOX_NORMAL);
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.2f);
        openAni = false;
    }

    IEnumerator BoxTotalResult() {
        openAni = true;
        if (openCount == 0)
            yield return new WaitForSeconds(0.9f);
        boxEffect.gameObject.SetActive(false);
        Transform boxParent = transform.Find("OpenBox");
        boxParent.GetChild(3).localScale = Vector3.zero;
        boxParent.GetChild(3).gameObject.SetActive(false);
        for (int i = 0; i < 4; i++) {
            Transform reward = boxParent.GetChild(i);
            Transform rewardTarget = reward.GetChild(0);
            reward.gameObject.SetActive(true);
            reward.position = lastPos.GetChild(i).position;
            if (rewardTarget.name == "Card") {
                rewardTarget.Find("Name").gameObject.SetActive(false);
                rewardTarget.Find("Rarelity").localScale = Vector3.zero;
                rewardTarget.Find("Rarelity").GetChild(0).gameObject.SetActive(false);
                if(rewardTarget.Find("GetCrystal").gameObject.activeSelf)
                    rewardTarget.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);

            }
            yield return new WaitForSeconds(0.1f);
            iTween.ScaleTo(reward.gameObject, iTween.Hash("x", 0.9f, "y", 0.9f, "islocal", true, "time", 0.4f));
        }
        yield return new WaitForSeconds(0.5f);
        transform.Find("ExitButton").gameObject.SetActive(true);
        openAni = false;
    }

    public void SetRewards(RewardClass[] rewardList) {
        for(int i = 0; i < rewardList.Length; i++) 
            SetEachReward(rewardList[i], i);
    }

    public void SetEachReward(RewardClass reward, int index) {
        Transform boxTarget = transform.Find("OpenBox").GetChild(index);
        Transform effects = transform.Find("EffectSpines");
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Update(0);
        if (reward.type == "card") {
            Transform target = boxTarget.Find("Card");
            target.SetAsFirstSibling();
            target.gameObject.SetActive(true);
            target.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(reward.item);
            target.Find("Name").localScale = Vector3.zero;
            bool isUnit = accountManager.allCardsDic[reward.item].type == "unit";
            if (isUnit)
                effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
            else
                effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
            Transform getCrystal = target.Find("GetCrystal");
            if (reward.amount > 0) {
                getCrystal.gameObject.SetActive(true);
                getCrystal.Find("ObjectsParent").gameObject.SetActive(false);
                getCrystal.Find("ObjectsParent/UnitBlock").gameObject.SetActive(isUnit);
                getCrystal.Find("ObjectsParent/MagicBlock").gameObject.SetActive(!isUnit);
                getCrystal.Find("ObjectsParent / Value").GetComponent<TMPro.TextMeshProUGUI>().text = reward.amount.ToString();
            }
            else {
                CheckNewCardList(reward.item);
                getCrystal.gameObject.SetActive(false);
                target.Find("GetCrystalEffect").gameObject.SetActive(false);
            }
        }
        else if(reward.type == "hero") {
            Transform target = boxTarget.Find("Hero");
            target.SetAsFirstSibling();
            target.gameObject.SetActive(true);
            target.Find("Image").GetComponent<Image>().sprite = accountManager.resource.heroPortraite[reward.item + "_button"];
            target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();

        }
        else {
            Transform target = boxTarget.Find("Resource");
            target.SetAsFirstSibling();
            target.gameObject.SetActive(true);
            target.Find(reward.item).gameObject.SetActive(true);
            target.Find(reward.item).SetSiblingIndex(1);
            target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();
            effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("4.item");
        }
        effects.GetChild(index).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
    }




    void CheckNewCardList(string cardId) {
        CollectionCard cardData = accountManager.allCardsDic[cardId];
        if (cardData.camp == "human") {
            if (!accountManager.cardPackage.checkHumanCard.Contains(cardId)) {
                accountManager.cardPackage.checkHumanCard.Add(cardId);
                accountManager.cardPackage.rarelityHumanCardCheck[cardData.rarelity].Add(cardId);
            }
        }
        else {
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