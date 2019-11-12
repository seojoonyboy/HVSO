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

public class BoxRewardManager : MonoBehaviour
{
    [SerializeField] Transform boxObject;
    [SerializeField] TMPro.TextMeshProUGUI supplyStore;
    [SerializeField] TMPro.TextMeshProUGUI storeTimer;
    [SerializeField] SkeletonGraphic boxSpine;
    [SerializeField] SkeletonGraphic boxEffect;
    [SerializeField] Transform additionalSupply;
    [SerializeField] MenuSceneController menuSceneController;
    [SerializeField] Transform targetSpine;
    [SerializeField] Transform lastPos;
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
        //boxSpine.AnimationState.SetAnimation(0, "00.NOANI", false);
        boxSpine.AnimationState.SetAnimation(0, "01.START", false);
        boxSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
        boxEffect.Initialize(true);
        boxEffect.Update(0);
        boxEffect.gameObject.SetActive(true);
        boxEffect.AnimationState.SetAnimation(0, "00.NOANI", false);
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().Initialize();
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().boneName = "card";
        SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
    }

    //public void GetResult() {
    //    transform.Find("OpenBox").gameObject.SetActive(true);
    //    transform.Find("ShowBox/Text").gameObject.SetActive(false);
    //    boxSpine.AnimationState.SetAnimation(2, "03.TOUCH", false);
    //    SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN_2);
    //    StartCoroutine(ShowRewards());
    //}

    public void GetBoxResult() {
        if (openAni) {
            //StopCoroutine(nowAni);
            //StopAni(openCount - 1);
            return;
        }
        transform.Find("OpenBox").gameObject.SetActive(true);
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("OpenBox/TargetSpine").gameObject.SetActive(false);
        switch (openCount) {
            case 0:
                SetRewards(accountManager.rewardList);
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
                return;
            case 5:
                CloseBoxOpen();
                return;
        }

        SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN_2);
        int count = openCount;
        openCount++;
        StartCoroutine(ShowEachReward(count));
    }

    //void StopAni(int count) {
    //    transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = (4 - openCount).ToString();
    //    Transform target = transform.Find("OpenBox").GetChild(count);
    //    Destroy(target.GetComponent<iTween>());
    //    target.localScale = new Vector3(1.4f, 1.4f, 1);
    //    target.localPosition = targetSpine.localPosition;
    //    transform.Find("OpenBox/TargetSpine").gameObject.SetActive(false);
    //    Transform targetObj = target;
    //    if (target.name == "Random") {
    //        if (target.transform.Find("Card").gameObject.activeSelf)
    //            target = targetObj.Find("Card");
    //        else
    //            target = targetObj.Find("Resource");
    //    }
    //    if (target.name.Contains("Card")) {
    //        if (target.gameObject.activeSelf) { 
    //            string cardId = target.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().cardID;
    //            string aniName = "";
    //            var rarelity = accountManager.allCardsDic[cardId].rarelity;
    //            if (accountManager.allCardsDic[cardId].type == "unit")
    //                aniName += "u_";
    //            else
    //                aniName += "m_";
    //            if (rarelity != "common")
    //                aniName += accountManager.allCardsDic[cardId].rarelity;
    //            else
    //                aniName = "NOANI";
    //            SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
    //            spine.gameObject.SetActive(true);
    //            spine.Initialize(true);
    //            spine.Update(0);
    //            spine.AnimationState.SetAnimation(0, aniName, true);
    //        }
    //    }
    //    openAni = false;
    //}

    

    public void InitBoxObjects() {
        Transform boxParent = transform.Find("OpenBox");
        for (int i = 0; i < 4; i++) {
            boxParent.GetChild(i).gameObject.SetActive(true);
            boxParent.GetChild(i).localScale = Vector3.zero;
            boxParent.GetChild(i).localPosition = new Vector3(0, -480, 0);
            if (i < 2) {
                boxParent.GetChild(i).Find("back").gameObject.SetActive(false);
                boxParent.GetChild(i).Find("Name").localScale = Vector3.zero;
                boxParent.GetChild(i).Find("Rarelity").localScale = Vector3.zero;
            }
            else if (i == 3) {
                boxParent.GetChild(i).Find("Card").Find("back").gameObject.SetActive(false);
                boxParent.GetChild(i).Find("Card").Find("Name").localScale = Vector3.zero;
                boxParent.GetChild(i).Find("Card").Find("Rarelity").localScale = Vector3.zero;
            }
        }
    }

    public void CloseBoxOpen() {
        InitBoxObjects();
        Transform boxParent = transform.Find("OpenBox");
        boxParent.gameObject.SetActive(false);
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ShowBox/Text").gameObject.SetActive(true);
        transform.Find("ExitButton").gameObject.SetActive(false);
        boxParent.GetChild(0).Find("GetCrystal").gameObject.SetActive(false);
        boxParent.GetChild(1).Find("GetCrystal").gameObject.SetActive(false);
        boxParent.GetChild(2).Find("back").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Card").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Card/GetCrystal").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Resource").gameObject.SetActive(false);
        boxParent.GetChild(3).Find("Resource/back").gameObject.SetActive(false);
        for (int i = 1; i < 4; i++) {
            boxParent.GetChild(2).GetChild(i).gameObject.SetActive(false);
            boxParent.GetChild(3).Find("Resource").GetChild(i).gameObject.SetActive(false);
        }
        SetBoxObj();
        openningBox = false;
    }

    IEnumerator ShowEachReward(int count) {
        openAni = true;
        if (count == 0)
            yield return new WaitForSeconds(0.95f);
        else
            yield return new WaitForSeconds(0.5f);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = (4 - openCount).ToString();
        Transform target = transform.Find("OpenBox").GetChild(count);
        iTween.ScaleTo(target.gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.4f));
        iTween.MoveTo(target.gameObject, iTween.Hash("y", targetSpine.localPosition.y, "islocal", true, "time", 0.4f));
        SkeletonGraphic targetEffect = transform.Find("OpenBox/TargetSpine").GetComponent<SkeletonGraphic>();
        yield return new WaitForSeconds(0.1f);
        targetEffect.gameObject.SetActive(true);
        targetEffect.Initialize(true);
        targetEffect.Update(0);
        targetEffect.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(0.1f);
        GameObject targetObj = target.gameObject;
        if (target.name == "Random") {
            if (target.transform.Find("Card").gameObject.activeSelf)
                target = targetObj.transform.Find("Card");
            else
                target = targetObj.transform.Find("Resource");
        }

        if (target.name.Contains("Card")) {
            if (!target.gameObject.activeSelf) yield return null;
            else {
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
                target.Find("Rarelity").Find(accountManager.allCardsDic[cardId].rarelity).gameObject.SetActive(true);
                iTween.ScaleTo(target.Find("Rarelity").gameObject, iTween.Hash("x", 0.7f, "y", 0.7f, "islocal", true, "time", 0.3f));
                yield return new WaitForSeconds(0.2f);
                iTween.ScaleTo(target.Find("Name").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
            }
        }
        else {
            string type = string.Empty;
            for (int i = 1; i < 4; i++) {
                if (target.GetChild(i).gameObject.activeSelf) {
                    type = target.GetChild(i).gameObject.name;
                }
            }
            if (type != "supplyX2Coupon") { 
                SkeletonGraphic spine = target.Find("back").GetComponent<SkeletonGraphic>();
                spine.gameObject.SetActive(true);
                spine.Initialize(true);
                spine.Update(0);
                spine.AnimationState.SetAnimation(0, "g_superrare", true);
            }

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
        boxEffect.gameObject.SetActive(false);
        Transform boxParent = transform.Find("OpenBox");
        boxParent.GetChild(3).localScale = Vector3.zero;
        boxParent.GetChild(3).gameObject.SetActive(false);
        for (int i = 0; i < 4; i++) {
            boxParent.GetChild(i).gameObject.SetActive(true);
            boxParent.GetChild(i).position = lastPos.GetChild(i).position;
            if (i < 2) {
                boxParent.GetChild(i).Find("Name").localScale = Vector3.zero;
                boxParent.GetChild(i).Find("Rarelity").localScale = Vector3.zero;
                for (int j = 0; j < 5; j++) {
                    boxParent.GetChild(i).Find("Rarelity").GetChild(j).gameObject.SetActive(false);
                }
            }
            else if (i == 3) {
                boxParent.GetChild(i).Find("Card").Find("Name").localScale = Vector3.zero;
                boxParent.GetChild(i).Find("Card").Find("Rarelity").localScale = Vector3.zero;
                for (int j = 0; j < 5; j++) {
                    boxParent.GetChild(i).Find("Card").Find("Rarelity").GetChild(j).gameObject.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.1f);
            iTween.ScaleTo(boxParent.GetChild(i).gameObject, iTween.Hash("x", 0.9f, "y", 0.9f, "islocal", true, "time", 0.4f));
        }
        yield return new WaitForSeconds(0.5f);
        openAni = false;
    }


    //IEnumerator ShowRewards() {
    //    Transform boxParent = transform.Find("OpenBox");
    //    //Transform effects = transform.Find("EffectSpines");
    //    yield return new WaitForSeconds(1.2f);
    //    //effects.GetChild(0).gameObject.SetActive(true);
    //    SoundManager.Instance.PlaySound(UISfxSound.BOX_NORMAL);
    //    yield return new WaitForSeconds(0.05f);
    //    //effects.GetChild(1).gameObject.SetActive(true);
    //    SoundManager.Instance.PlaySound(UISfxSound.BOX_RARE);
    //    yield return new WaitForSeconds(0.05f);
    //    iTween.ScaleTo(boxParent.GetChild(0).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
    //    //effects.GetChild(2).gameObject.SetActive(true);
    //    SoundManager.Instance.PlaySound(UISfxSound.BOX_SUPERRARE);
    //    yield return new WaitForSeconds(0.05f);
    //    iTween.ScaleTo(boxParent.GetChild(1).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
    //    //effects.GetChild(3).gameObject.SetActive(true);
    //    SoundManager.Instance.PlaySound(UISfxSound.BOX_EPIC);
    //    yield return new WaitForSeconds(0.05f);
    //    iTween.ScaleTo(boxParent.GetChild(2).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
    //    yield return new WaitForSeconds(0.05f);
    //    if(boxParent.GetChild(3).Find("Card").gameObject.activeSelf)
    //        iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1.4, "y", 1.4, "islocal", true, "time", 0.2f));
    //    else
    //        iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
    //    yield return new WaitForSeconds(0.4f);
    //    //cardDic.SetToHumanCards();
    //    transform.Find("ExitButton").gameObject.SetActive(true);
    //}

    //public void ExitBoxOpen() {
    //    Transform boxParent = transform.Find("OpenBox");
    //    Transform effects = transform.Find("EffectSpines");
    //    for (int i = 0; i < 4; i++) {
    //        boxParent.GetChild(i).localScale = Vector3.zero;
    //        iTween.RotateTo(boxParent.GetChild(i).gameObject, iTween.Hash("y", 180, "islocal", true));
    //        boxParent.GetChild(i).Find("BackImage").gameObject.SetActive(true);
    //        if(i < 2)
    //            boxParent.GetChild(i).transform.Find("back").gameObject.SetActive(false);
    //        else if(i == 3)
    //            boxParent.GetChild(i).Find("Card").transform.Find("back").gameObject.SetActive(false);
    //    }
    //    boxParent.gameObject.SetActive(false);
    //    transform.Find("ShowBox").gameObject.SetActive(false);
    //    transform.Find("ShowBox/Text").gameObject.SetActive(true);
    //    transform.Find("ExitButton").gameObject.SetActive(false);
    //    boxParent.GetChild(0).Find("GetCrystal").gameObject.SetActive(false);
    //    boxParent.GetChild(1).Find("GetCrystal").gameObject.SetActive(false);
    //    boxParent.GetChild(3).Find("Card").gameObject.SetActive(false);
    //    boxParent.GetChild(3).Find("Card/GetCrystal").gameObject.SetActive(false);
    //    boxParent.GetChild(3).Find("Resource").gameObject.SetActive(false);
    //    for (int i = 0; i < 3; i++) {
    //        boxParent.GetChild(2).GetChild(i).gameObject.SetActive(false);
    //        boxParent.GetChild(3).Find("Resource").GetChild(i).gameObject.SetActive(false);
    //    }
    //    for (int i = 0; i < 4; i++)             
    //        effects.GetChild(i).gameObject.SetActive(false);

    //    SetBoxObj();
    //    openningBox = false;
    //}

    public void SetRewards(RewardClass[] rewardList) {
        Transform boxParent = transform.Find("OpenBox");
        Transform effects = transform.Find("EffectSpines");
        boxParent.GetChild(0).Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(rewardList[0].item);

        effects.GetChild(0).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(0).GetComponent<SkeletonGraphic>().Update(0);
        if (accountManager.allCardsDic[rewardList[0].item].type == "unit")
            effects.GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
        else
            effects.GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
        effects.GetChild(0).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
        if (rewardList[0].amount > 0) {
            boxParent.GetChild(0).GetChild(2).gameObject.SetActive(true);
            boxParent.GetChild(0).GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[0].amount.ToString();
        }
        else
            CheckNewCardList(rewardList[0].item);

        boxParent.GetChild(1).Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(rewardList[1].item);
        effects.GetChild(1).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(1).GetComponent<SkeletonGraphic>().Update(0);
        if (accountManager.allCardsDic[rewardList[1].item].type == "unit")
            effects.GetChild(1).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
        else
            effects.GetChild(1).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
        effects.GetChild(1).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
        if (rewardList[1].amount > 0) {
            boxParent.GetChild(1).GetChild(2).gameObject.SetActive(true);
            boxParent.GetChild(1).GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[1].amount.ToString();
        }
        else
            CheckNewCardList(rewardList[1].item);

        boxParent.GetChild(2).Find(rewardList[2].item).gameObject.SetActive(true);
        boxParent.GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + rewardList[2].amount.ToString();
        effects.GetChild(2).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(2).GetComponent<SkeletonGraphic>().Update(0);
        effects.GetChild(2).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);

        effects.GetChild(3).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(3).GetComponent<SkeletonGraphic>().Update(0);
        if (rewardList[3].type == "card"){
            boxParent.GetChild(3).Find("Card").gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").gameObject.SetActive(false);
            boxParent.GetChild(3).Find("Card").Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(rewardList[3].item);
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
            boxParent.GetChild(3).Find("Card").gameObject.SetActive(false);
            boxParent.GetChild(3).Find("Resource").Find(rewardList[3].item).gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + rewardList[3].amount.ToString();
            effects.GetChild(3).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("4.item");
        }
        effects.GetChild(3).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
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

    //public void FlipCard(GameObject target) {
    //    StartCoroutine(FlipAni(target));
    //}

    //IEnumerator FlipAni(GameObject targetObj) {
    //    GameObject target = targetObj;
    //    iTween.RotateTo(target, iTween.Hash("y", 0, "islocal", true, "time", 0.8f));
    //    yield return new WaitForSeconds(0.1f);
    //    target.transform.Find("BackImage").gameObject.SetActive(false);
    //    yield return new WaitForSeconds(0.05f);
    //    if (target.name == "Random") {
    //        if(targetObj.transform.Find("Card").gameObject.activeSelf)
    //            target = targetObj.transform.Find("Card").gameObject;
    //        else 
    //            target = targetObj.transform.Find("Resource").gameObject;
    //    }   
    //    if (target.name.Contains("Card")) {
    //        if (!target.activeSelf) yield return null;
    //        else {
    //            string cardId = target.transform.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().cardID;
    //            string aniName = "";

    //            var rarelity = accountManager.allCardsDic[cardId].rarelity;

    //            if (accountManager.allCardsDic[cardId].type == "unit")
    //                aniName += "u_";
    //            else
    //                aniName += "m_";
    //            if (rarelity != "common")
    //                aniName += accountManager.allCardsDic[cardId].rarelity;
    //            else
    //                aniName = "NOANI";
    //            SkeletonGraphic spine = target.transform.Find("back").GetComponent<SkeletonGraphic>();
    //            spine.gameObject.SetActive(true);
    //            spine.Initialize(true);
    //            spine.Update(0);
    //            spine.AnimationState.SetAnimation(0, aniName, true);

    //            SoundManager soundManager = SoundManager.Instance;
    //            switch (rarelity) {
    //                case "common":
    //                case "uncommon":
    //                    soundManager.PlaySound(UISfxSound.BOX_NORMAL);
    //                    break;
    //                case "rare":
    //                    soundManager.PlaySound(UISfxSound.BOX_RARE);
    //                    break;
    //                case "superrare":
    //                    soundManager.PlaySound(UISfxSound.BOX_SUPERRARE);
    //                    break;
    //                case "legend":
    //                    soundManager.PlaySound(UISfxSound.BOX_EPIC);
    //                    break;
    //            }
    //        }
    //    }
    //    //재화 획득인 경우
    //    else {
    //        string type = string.Empty;
    //        for(int i=0; i<3; i++) {
    //            if (target.transform.GetChild(i).gameObject.activeSelf) {
    //                type = target.transform.GetChild(i).gameObject.name;
    //            }
    //        }

    //        SoundManager soundManager = SoundManager.Instance;
    //        switch (type) {
    //            case "gold":
    //            case "supplyStore":
    //                soundManager.PlaySound(UISfxSound.BOX_SUPERRARE);
    //                break;
    //            case "crystal":
    //                soundManager.PlaySound(UISfxSound.BOX_NORMAL);
    //                break;
    //        }
    //    }
    //}
}

public class RewardClass {
    public string item;
    public int amount;
    public string type;
}