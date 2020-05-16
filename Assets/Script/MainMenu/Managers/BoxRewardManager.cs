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
    [SerializeField] Slider supplySlider;
    [SerializeField] Transform AdsWindow;
    //[SerializeField] protected SkeletonGraphic boxSpine;
    [SerializeField] protected SkeletonGraphic boxEffect;
    [SerializeField] Transform additionalSupply;
    [SerializeField] MenuSceneController menuSceneController;
    [SerializeField] Transform targetSpine;
    [SerializeField] Transform lastPos;
    [SerializeField] GameObject skipBtn;
    [SerializeField] Transform resourceInfo;
    [SerializeField] GameObject buttonGlow;
    // Start is called before the first frame update
    Transform hudCanvas;

    protected AccountManager accountManager;
    protected NetworkManager networkManager;

    public UnityEvent OnBoxLoadFinished = new UnityEvent();
    protected static bool openningBox = false;
    protected bool openAni = false;
    protected int openCount;
    protected float beforeBgmVolume;
    protected int countOfRewards;
    protected List<List<RewardClass>> multipleBoxes;
    protected SkeletonGraphic mainSpine;

    bool isSupplySliderInit = false;
    int supplyValue = 0;
    private bool canProceed = false;
    void Awake() {
        accountManager = AccountManager.Instance;
        hudCanvas = transform.parent;
        accountManager.userResource.LinkTimer(storeTimer, AdsWindow);
        canProceed = false;
        
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserInfoUpdated);
        
        OnBoxLoadFinished.AddListener(() => accountManager.RequestInventories());
    }

    private void OnUserInfoUpdated(Enum event_type, Component sender, object param) {
        supplyValue = accountManager.userResource.supply;
        canProceed = true;
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserInfoUpdated);
    }

    protected void OnBoxOpenRequest(Enum Event_Type, Component Sender, object Param) {
        SetBoxAnimation();
        OnBoxLoadFinished.Invoke();
    }

    public void SetBoxObj() {
        supplyStore.text = accountManager.userResource.supplyStore.ToString();
        buttonGlow.SetActive(accountManager.userResource.supplyStore > 0 ? true : false);
        if (accountManager.userResource.supplyBox > 0) {
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(true);
            boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.userResource.supplyBox.ToString();
        }
        else
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(false);
        additionalSupply.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.userResource.supplyX2Coupon.ToString();
        
        if (accountManager.startSpread) {
            if (accountManager.beforeBox != accountManager.userResource.supplyBox
            || accountManager.beforeSupply != accountManager.userResource.supply) {
                boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.beforeBox.ToString();
                boxObject.Find("SupplyGauge/ValueSlider").GetComponent<Slider>().value = accountManager.beforeSupply;
                boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.beforeSupply + "/100";
                accountManager.beforeBox = accountManager.userResource.supplyBox;
                accountManager.beforeSupply = accountManager.userResource.supply;
                AccountManager.Instance.startSpread = false;
            }
        }
        else {
            boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.userResource.supplyBox.ToString();
            boxObject.Find("SupplyGauge/ValueSlider").GetComponent<Slider>().value = accountManager.userResource.supply;
            boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.userResource.supply + "/100";
        }
    }

    private List<Area> sliderStack;
    public void AddSliderStack(int amount) {
        if(sliderStack == null) sliderStack = new List<Area>();

        int from = (int)boxObject.Find("SupplyGauge/ValueSlider").GetComponent<Slider>().value;
        int to = from + amount;

        if (to > 100) {
            sliderStack.Add(new Area(from, 100));
            sliderStack.Add(new Area(0, to - 100));
        }
        else sliderStack.Add(new Area(from, to));
    }
    
    public IEnumerator ProceedSupplySlider() {
        yield return new WaitUntil(() => canProceed);
        if (sliderStack != null) {
            foreach (var stack in sliderStack) {
                yield return __proceedSupplySlider(stack);
            }
        }

        boxObject.Find("SupplyGauge/ValueSlider").GetComponent<Slider>().value = supplyValue;
        if (supplyValue < 0) supplyValue = 0;
        boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = supplyValue + "/100";
        if (sliderStack != null) sliderStack.Clear();
    }
    
    IEnumerator __proceedSupplySlider(Area area) {
        int val = area.from;
        var slider = boxObject.Find("SupplyGauge/ValueSlider").GetComponent<Slider>();
        while (val <= area.to) {
            val += 1;
            slider.value = val;
        }
        yield return 0;
    }

    public class Area {
        public int from;
        public int to;

        public Area(int from, int to) {
            this.from = from;
            this.to = to;
        }
    }

    public virtual void OpenBox() {
        if (openningBox) return;
        if (openAni) return;
        if (AccountManager.Instance.userResource.supplyBox <= 0) return;
        openningBox = true;
        beforeBgmVolume = SoundManager.Instance.bgmController.BGMVOLUME;
        //SoundManager.Instance.bgmController.BGMVOLUME = 0;
        accountManager.RequestRewardInfo();
    }

    public void OpenBoxImmediately() {
        if (openAni) return;
        beforeBgmVolume = SoundManager.Instance.bgmController.BGMVOLUME;
        accountManager.RequestRewardInfo();
    }

    public virtual void SetBoxAnimation() {
        InitBoxObjects();
        transform.Find("ShowBox").gameObject.SetActive(true);
        openCount = 0;
        if (mainSpine != null) mainSpine.gameObject.SetActive(false);
        switch (accountManager.rewardList.Length) {
            case 3:
                mainSpine = transform.Find("ShowBox/BoxSpine/Small").GetComponent<SkeletonGraphic>();
                break;
            case 4:
                mainSpine = transform.Find("ShowBox/BoxSpine/Normal").GetComponent<SkeletonGraphic>();
                break;
            case 6:
                mainSpine = transform.Find("ShowBox/BoxSpine/Large").GetComponent<SkeletonGraphic>();
                break;
            case 8:
                mainSpine = transform.Find("ShowBox/BoxSpine/ExLarge").GetComponent<SkeletonGraphic>();
                break;
        }
        mainSpine.gameObject.SetActive(true);
        mainSpine.Initialize(true);
        mainSpine.Update(0);
        SoundManager.Instance.PlaySound(UISfxSound.BOX_APPEAR);
        mainSpine.AnimationState.SetAnimation(0, "01.START", false);
        mainSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
        boxEffect.Initialize(true);
        boxEffect.Update(0);
        boxEffect.gameObject.SetActive(true);
        boxEffect.AnimationState.SetAnimation(0, "00.NOANI", false);
        BoneFollowerGraphic numBone = transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>();
        numBone.skeletonGraphic = mainSpine;
        numBone.Initialize();
        numBone.boneName = "card";
        //SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
        SetRewards(accountManager.rewardList);
        countOfRewards = accountManager.rewardList.Length;
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = countOfRewards.ToString();
        transform.Find("OpenBox").gameObject.SetActive(true);
    }

    public virtual void SetRewardBoxAnimation(RewardClass[] reward) {
        InitBoxObjects();
        transform.Find("ShowBox").gameObject.SetActive(true);
        openCount = 0;
        if (mainSpine != null) mainSpine.gameObject.SetActive(false);
        switch (reward.Length) {
            case 3:
                mainSpine = transform.Find("ShowBox/BoxSpine/Small").GetComponent<SkeletonGraphic>();
                break;
            case 4:
                mainSpine = transform.Find("ShowBox/BoxSpine/Normal").GetComponent<SkeletonGraphic>();
                break;
            case 6:
                mainSpine = transform.Find("ShowBox/BoxSpine/Large").GetComponent<SkeletonGraphic>();
                break;
            case 8:
                mainSpine = transform.Find("ShowBox/BoxSpine/ExLarge").GetComponent<SkeletonGraphic>();
                break;
        }
        mainSpine.gameObject.SetActive(true);
        mainSpine.Initialize(true);
        mainSpine.Update(0);
        SoundManager.Instance.PlaySound(UISfxSound.BOX_APPEAR);
        mainSpine.AnimationState.SetAnimation(0, "01.START", false);
        mainSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
        boxEffect.Initialize(true);
        boxEffect.Update(0);
        boxEffect.gameObject.SetActive(true);
        boxEffect.AnimationState.SetAnimation(0, "00.NOANI", false);
        BoneFollowerGraphic numBone = transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>();
        numBone.skeletonGraphic = mainSpine;
        numBone.Initialize();
        numBone.boneName = "card";
        //SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
        SetRewards(reward);
        countOfRewards = (reward != null) ? reward.Length : 0;
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = countOfRewards.ToString();
        transform.Find("OpenBox").gameObject.SetActive(true);
    }


    public void GetBoxResult() {
        if (openCount > countOfRewards) return;
        if (openAni) return;
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("OpenBox/TargetSpine").gameObject.SetActive(false);
        if(openCount == 0) {
            SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
            mainSpine.AnimationState.SetAnimation(2, "03.TOUCH1", false);
            boxEffect.AnimationState.SetAnimation(1, "01.open", false);
            boxEffect.AnimationState.AddAnimation(2, "loop", true, 1.2f);
        }
        else
            SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN_2);
        if (openCount != 0 && openCount + 1 < countOfRewards) {
            transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
            transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
            mainSpine.AnimationState.SetAnimation(2, "04.TOUCH2", false);
            boxEffect.AnimationState.SetAnimation(1 + (openCount * 2), "02.open", false);
            boxEffect.AnimationState.AddAnimation(2 + (openCount * 2), "loop", true, 1.0f);
        }
        if (openCount != 0 && openCount + 1 == countOfRewards) {
            transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
            transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
            mainSpine.AnimationState.SetAnimation(2, "04.TOUCH2", false);
            boxEffect.AnimationState.SetAnimation(1 + (openCount * 2), "03.open", false);
        }
        if(openCount == countOfRewards) {
            StartCoroutine(BoxTotalResult());
            openCount++;
            skipBtn.SetActive(false);
            return;
        }

        int count = openCount;
        openCount++;
        StartCoroutine(ShowEachReward(count));
    }

    public void SkipBoxOpen() {
        if (openAni) return;
        if (openCount == 0) {
            mainSpine.AnimationState.SetAnimation(2, "03.TOUCH1", false);
            boxEffect.AnimationState.SetAnimation(1, "01.open", false);
            SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
            StartCoroutine(SkipAllReward());
        }
        if (openCount > 0) {
            transform.Find("OpenBox").GetChild(openCount - 1).gameObject.SetActive(false);
            transform.Find("OpenBox").GetChild(openCount - 1).localScale = Vector3.zero;
        }
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = 0.ToString();
        SetSkipBackSpine();
        StartCoroutine(BoxTotalResult());
        openCount = countOfRewards + 1;
        skipBtn.SetActive(false);
    }

    public IEnumerator SkipAllReward() {
        SkeletonGraphic targetEffect = transform.Find("OpenBox/TargetSpine").GetComponent<SkeletonGraphic>();
        yield return new WaitForSeconds(1.0f);
        targetEffect.gameObject.SetActive(true);
        targetEffect.Initialize(true);
        targetEffect.Update(0);
        targetEffect.AnimationState.SetAnimation(0, "animation", false);
        
    }

    public void SetSkipBackSpine() {
        for (int i = 0; i < 8; i++) {
            Transform target = transform.Find("OpenBox").GetChild(i).GetChild(1);
            SkeletonGraphic backSpine = target.parent.Find("back").GetComponent<SkeletonGraphic>();
            backSpine.Initialize(true);
            backSpine.Update(0);
            if (target.name.Contains("card")) {
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
                    
                    backSpine.gameObject.SetActive(true);
                    backSpine.AnimationState.SetAnimation(0, aniName, true);
                }
            }
            else if (target.name.Contains("hero")) {
                backSpine.gameObject.SetActive(true);
                backSpine.AnimationState.SetAnimation(0, "h_legend", true);
            }
            else {
                string type = target.GetChild(0).name;                
                if (type == "gold") {
                    backSpine.gameObject.SetActive(true);
                    backSpine.AnimationState.SetAnimation(0, "g_legend", true);
                }
                else
                    backSpine.gameObject.SetActive(false);
            }
        }
    }

    public void InitBoxObjects() {
        skipBtn.SetActive(true);
        Transform boxParent = transform.Find("OpenBox");
        for (int i = 0; i < 8; i++) {
            Transform reward = boxParent.GetChild(i);
            reward.gameObject.SetActive(true);
            reward.localScale = Vector3.zero;
            reward.localPosition = new Vector3(0, -480, 0);
            reward.Find("back").gameObject.SetActive(false);
            reward.Find("card/GetCrystal").gameObject.SetActive(false);
            reward.Find("Name").gameObject.SetActive(true);
            reward.Find("Name").localScale = Vector3.zero;
            reward.Find("Rarelity").localScale = Vector3.zero;
            reward.Find("resource").GetChild(0).gameObject.SetActive(false);
            reward.Find("hero/GetCrystal").gameObject.SetActive(false);
            reward.GetChild(1).gameObject.SetActive(false);
            if (i == 3) {
                reward.Find("AdReward").gameObject.SetActive(false);
                reward.Find("AdReward").GetComponent<Image>().enabled = true;
            }
            SkeletonGraphic crystalSpine = reward.Find("card/GetCrystalEffect").GetComponent<SkeletonGraphic>();
            crystalSpine.gameObject.SetActive(true);
            crystalSpine.Initialize(true);
            crystalSpine.Update(0);
        }
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ShowBox/Text").gameObject.SetActive(true);
        transform.Find("ShowBox/InfoText").gameObject.SetActive(false);
        transform.Find("ShowBox/Shadow").gameObject.SetActive(false);
        transform.Find("Buttons/AdButton").gameObject.SetActive(false);
        transform.Find("Buttons/ExitButton").gameObject.SetActive(false);
        transform.Find("OpenImmediately")?.gameObject.SetActive(false);
    }

    public virtual void CloseBoxOpen() {
        if (openAni) return;
        if (Input.touchCount > 1) return;
        InitBoxObjects();
        Transform boxParent = transform.Find("OpenBox");
        //SoundManager.Instance.bgmController.BGMVOLUME = beforeBgmVolume;
        boxParent.gameObject.SetActive(false);
        
        SetBoxObj();
        openningBox = false;        
        if(multipleBoxes != null && multipleBoxes.Count > 0) {
            multipleBoxes.RemoveAt(0);
            if (multipleBoxes.Count > 0)
                SetRewardBoxAnimation(multipleBoxes[0].ToArray());
        }
    }

    IEnumerator ShowEachReward(int count) {
        openAni = true;
        SoundManager soundManager = SoundManager.Instance;
        transform.Find("OpenBox").GetChild(count).Find("Name").localScale = Vector3.zero;
        if (count == 0)
            yield return new WaitForSeconds(0.95f);
        else
            yield return new WaitForSeconds(0.5f);
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = (countOfRewards - openCount).ToString();
        Transform targetBox = transform.Find("OpenBox").GetChild(count);
        Transform target = targetBox.GetChild(1);
        iTween.ScaleTo(target.parent.gameObject, iTween.Hash("x", 1.6, "y", 1.6, "islocal", true, "time", 0.4f));
        iTween.MoveTo(target.parent.gameObject, iTween.Hash("y", targetSpine.localPosition.y, "islocal", true, "time", 0.4f));
        SkeletonGraphic targetEffect = transform.Find("OpenBox/TargetSpine").GetComponent<SkeletonGraphic>();
        SkeletonGraphic backSpine = target.parent.Find("back").GetComponent<SkeletonGraphic>();
        backSpine.Initialize(true);
        backSpine.Update(0);
        yield return new WaitForSeconds(0.1f);
        targetEffect.gameObject.SetActive(true);
        targetEffect.Initialize(true);
        targetEffect.Update(0);
        targetEffect.AnimationState.SetAnimation(0, "animation", false);
        if (target.name == "card") {
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
                backSpine.gameObject.SetActive(true);
                backSpine.AnimationState.SetAnimation(0, aniName, true);

                
                targetBox.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.allCardsDic[cardId].name;
                targetBox.Find("Rarelity").Find(accountManager.allCardsDic[cardId].rarelity).SetAsFirstSibling();
                targetBox.Find("Rarelity").GetChild(0).gameObject.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                iTween.ScaleTo(targetBox.Find("Name").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
                yield return new WaitForSeconds(0.2f);
                iTween.ScaleTo(targetBox.Find("Rarelity").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
                
                if (target.Find("GetCrystal").gameObject.activeSelf) {
                    yield return new WaitForSeconds(0.2f);
                    SkeletonGraphic crystalSpine = target.Find("GetCrystalEffect").GetComponent<SkeletonGraphic>();
                    crystalSpine.AnimationState.SetAnimation(0, "animation", false);
                    yield return new WaitForSeconds(0.2f);
                    target.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);
                }

                yield return new WaitForSeconds(0.1f);
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
            }
        }
        else if(target.name == "hero") {
            backSpine.gameObject.SetActive(true);
            backSpine.AnimationState.SetAnimation(0, "h_legend", true);
            targetBox.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.myHeroInventories[target.GetChild(0).name].name;
            targetBox.Find("Rarelity").Find("hero").SetAsFirstSibling();
            targetBox.Find("Rarelity").GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            iTween.ScaleTo(targetBox.Find("Name").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
            yield return new WaitForSeconds(0.2f);
            iTween.ScaleTo(targetBox.Find("Rarelity").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
            soundManager.PlaySound(UISfxSound.BOX_EPIC);
            if (target.Find("GetCrystal").gameObject.activeSelf) {
                yield return new WaitForSeconds(0.4f);
                SkeletonGraphic crystalSpine = target.Find("GetCrystalEffect").GetComponent<SkeletonGraphic>();
                crystalSpine.AnimationState.SetAnimation(0, "animation", false);
                yield return new WaitForSeconds(0.2f);
                target.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);
            }
        }
        else {
            string type = target.GetChild(0).gameObject.name;
            if (type.Contains("gold")) {
                backSpine.gameObject.SetActive(true);
                backSpine.AnimationState.SetAnimation(0, "g_legend", true);
            }
            else {
                backSpine.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.2f);
            if (type.Contains("gold")) {
                targetBox.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = "금화";
                soundManager.PlaySound(UISfxSound.BOX_EPIC);
            }
            else if (type.Contains("Coupon")) {
                targetBox.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = "2배 쿠폰";
                soundManager.PlaySound(UISfxSound.BOX_NORMAL);
            }
            else if (type.Contains("crystal")) {
                targetBox.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = "마력결정";
                soundManager.PlaySound(UISfxSound.BOX_NORMAL);
            }
            yield return new WaitForSeconds(0.2f);
            targetBox.Find("Rarelity").Find("resource").SetAsFirstSibling();
            targetBox.Find("Rarelity").GetChild(0).gameObject.SetActive(true);
            iTween.ScaleTo(targetBox.Find("Name").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
            yield return new WaitForSeconds(0.2f);
            iTween.ScaleTo(targetBox.Find("Rarelity").gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "islocal", true, "time", 0.3f));
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
        boxParent.GetChild(countOfRewards - 1).localScale = Vector3.zero;
        boxParent.GetChild(countOfRewards - 1).gameObject.SetActive(false);
        for (int i = 0; i < countOfRewards; i++) {
            Transform reward = boxParent.GetChild(i);
            Transform rewardTarget = reward.GetChild(1);
            reward.gameObject.SetActive(true);
            reward.position = lastPos.Find(countOfRewards.ToString()).GetChild(i).position;
            reward.Find("Name").gameObject.SetActive(false);
            reward.Find("Rarelity").localScale = Vector3.zero;
            reward.Find("Rarelity").GetChild(0).gameObject.SetActive(false);
            if (rewardTarget.name == "card" || rewardTarget.name == "hero") {
                if(rewardTarget.Find("GetCrystal").gameObject.activeSelf)
                    rewardTarget.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(0.1f);
            iTween.ScaleTo(reward.gameObject, iTween.Hash("x", 0.9f, "y", 0.9f, "islocal", true, "time", 0.4f));
        }
        SoundManager.Instance.PlaySound(UISfxSound.BOX_OPEN_FINISH);
        yield return new WaitForSeconds(0.3f);
        if(countOfRewards == 3) {
            Transform reward = boxParent.GetChild(3);
            Transform rewardTarget = reward.Find("AdReward");
            reward.Find("AdReward").gameObject.SetActive(true);
            reward.gameObject.SetActive(true);
            reward.position = lastPos.Find(countOfRewards.ToString()).GetChild(3).position;
            reward.Find("Name").gameObject.SetActive(false);
            reward.Find("Rarelity").localScale = Vector3.zero;
            reward.Find("Rarelity").GetChild(0).gameObject.SetActive(false);
            if (rewardTarget.name == "card" || rewardTarget.name == "hero") {
                if (rewardTarget.Find("GetCrystal").gameObject.activeSelf)
                    rewardTarget.Find("GetCrystal").GetChild(0).gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(0.1f);
            iTween.ScaleTo(reward.gameObject, iTween.Hash("x", 0.9f, "y", 0.9f, "islocal", true, "time", 0.4f));
            yield return new WaitForSeconds(0.5f);
        }
        transform.Find("ShowBox/InfoText").gameObject.SetActive(true);
        transform.Find("ShowBox/Shadow").gameObject.SetActive(true);
        transform.Find("Buttons/ExitButton").gameObject.SetActive(true);
        if (countOfRewards == 3) {
            int boxNum = AccountManager.Instance.userResource.supplyBox;
            Transform openImmediately = transform.Find("OpenImmediately");

            transform.Find("Buttons/AdButton").gameObject.SetActive(true);
            transform.Find("Buttons/AdButton").gameObject.GetComponent<Button>().interactable = true;
            if (boxNum > 0 && openImmediately) {     
                transform.Find("OpenImmediately").gameObject.SetActive(AccountManager.Instance.userResource.supplyBox > 0);
                transform.Find("OpenImmediately/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = boxNum.ToString();
            }
            else
                transform.Find("OpenImmediately")?.gameObject.SetActive(false);
        }
        openAni = false;
    }

    public virtual void SetRewards(RewardClass[] rewardList) {
        for(int i = 0; i < rewardList.Length; i++) {
            Logger.Log("보상정보 : " + rewardList[i].type);
            SetEachReward(rewardList[i], i);
        }
    }

    public virtual void SetEachReward(RewardClass reward, int index) {
        Transform boxTarget = transform.Find("OpenBox").GetChild(index);
        Transform effects = transform.Find("EffectSpines");
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Update(0);
        if (reward.type == "card") {
            Logger.Log("카드 보상 :" + reward.item);
            Transform target = boxTarget.Find("card");
            target.SetSiblingIndex(1);
            target.gameObject.SetActive(true);
            target.Find("DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(reward.item);
            boxTarget.Find("Name").localScale = Vector3.zero;
            bool isUnit = accountManager.allCardsDic[reward.item].type == "unit";
            if (isUnit)
                effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
            else
                effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.magic");
            Transform getCrystal = target.Find("GetCrystal");
            if (reward.crystal > 0) {
                getCrystal.gameObject.SetActive(true);
                getCrystal.Find("ObjectsParent").gameObject.SetActive(false);
                getCrystal.Find("ObjectsParent/UnitBlock").gameObject.SetActive(isUnit);
                getCrystal.Find("ObjectsParent/MagicBlock").gameObject.SetActive(!isUnit);
                getCrystal.Find("ObjectsParent/Value").GetComponent<TMPro.TextMeshProUGUI>().text = reward.crystal.ToString();
            }
            else {
                CheckNewCardList(reward.item);
                getCrystal.gameObject.SetActive(false);
                target.Find("GetCrystalEffect").gameObject.SetActive(false);

                var alertManager = NewAlertManager.Instance;
                alertManager
                    .SetUpButtonToAlert(
                        alertManager.referenceToInit[NewAlertManager.ButtonName.DICTIONARY],
                        NewAlertManager.ButtonName.DICTIONARY,
                        false
                    );
                alertManager
                    .SetUpButtonToUnlockCondition(
                    NewAlertManager.ButtonName.DICTIONARY,
                        reward.item
                );
            }
        }
        else if(reward.type == "hero") {
            Transform target = boxTarget.Find("hero");
            target.SetSiblingIndex(1);
            target.gameObject.SetActive(true);
            target.GetChild(0).GetComponent<Image>().sprite = accountManager.resource.heroPortraite[reward.item + "_button"];
            target.GetChild(0).name = reward.item;
            target.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            target.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OpenHeroInfoBtn(reward.item));
            target.Find("Value").gameObject.SetActive(reward.crystal == 0);
            target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();
            if (!target.Find("Value").gameObject.activeSelf) {
                Transform getCrystal = target.Find("GetCrystal");
                getCrystal.gameObject.SetActive(true);
                getCrystal.Find("ObjectsParent").gameObject.SetActive(false);
                getCrystal.Find("ObjectsParent/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.crystal.ToString();
            }
        }
        else {
            Transform target = boxTarget.Find("resource");
            target.SetSiblingIndex(1);
            target.gameObject.SetActive(true);
            target.Find(reward.item).gameObject.SetActive(true);
            target.Find(reward.item).SetSiblingIndex(0);
            target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();
            effects.GetChild(index).GetComponent<SkeletonGraphic>().Skeleton.SetSkin("4.item");
        }
        effects.GetChild(index).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
    }

    public void SetAdReward(Enum Event_Type, Component Sender, object Param) {
        OnBoxLoadFinished.Invoke();
        transform.Find("Buttons/AdButton").gameObject.GetComponent<Button>().interactable = false;
        SetEachReward(accountManager.boxAdReward, 3);
        StartCoroutine(ShowAdReward());
    }

    IEnumerator ShowAdReward() {
        openAni = true;
        GameObject resource = transform.Find("OpenBox").GetChild(3).GetChild(1).gameObject;
        Transform rewardTarget = transform.Find("OpenBox").GetChild(3).Find("AdReward");
        SkeletonGraphic graphic = rewardTarget.GetChild(0).GetComponent<SkeletonGraphic>();
        Vector3 scale = resource.transform.localScale;
        transform.Find("OpenBox").GetChild(3).GetChild(1).transform.localScale = new Vector3(0, scale.y, 1);
        rewardTarget.transform.GetChild(0).gameObject.SetActive(true);
        graphic.Initialize(true);
        graphic.AnimationState.Update(0);
        yield return new WaitForSeconds(0.2f);
        rewardTarget.GetComponent<Image>().enabled = false;
        graphic.AnimationState.SetAnimation(0, "open", false);
        yield return new WaitForSeconds(1.3f);
        iTween.ScaleTo(resource, iTween.Hash("x", scale.x, "time", 0.4f, "isLocal", true));
        yield return new WaitForSeconds(0.5f);
        openAni = false;
    }



    public void OpenHeroInfoBtn(string heroId) {
        MenuHeroInfo.heroInfoWindow.SetHeroInfoWindow(heroId);
        MenuHeroInfo.heroInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuHeroInfo.heroInfoWindow.gameObject.SetActive(true);
    }

    public void OpenResourceInfo(string resource) {
        if (resource == "adReward") return;
        RewardDescriptionHandler rewardDescriptionHandler = RewardDescriptionHandler.instance;
        rewardDescriptionHandler.RequestDescriptionModal(resource);
    }

    public void CloseResourceInfo() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseResourceInfo);
        resourceInfo.GetChild(1).gameObject.SetActive(false);
        resourceInfo.gameObject.SetActive(false);
    }


    protected virtual void CheckNewCardList(string cardId) {
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

    public void OpenMultipleBoxes(List<List<RewardClass>> rewardList) {
        if (rewardList.Count == 0) return;
        OnBoxLoadFinished.Invoke();
        multipleBoxes = rewardList;
        SetRewardBoxAnimation(multipleBoxes[0].ToArray());
    }


    public void OpenMainAdWindow() {
        AdsWindow.gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseMainAdWindow);
    }

    public void CloseMainAdWindow() {
        AdsWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseMainAdWindow);
    }
}

public class RewardClass {
    public string item;
    public int amount;
    public string type;
    public int crystal;
}