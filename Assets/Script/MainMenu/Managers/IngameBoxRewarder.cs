using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;

public class IngameBoxRewarder : BoxRewardManager
{


    private void Awake() {
        accountManager = AccountManager.Instance;
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
        OnBoxLoadFinished.AddListener(() => accountManager.RequestInventories());
    }

    private void Start() {
        
    }


    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
    }

    public void SetRewardBox() {
        if (PlayMangement.chapterData == null) return;
        if (openningBox) return;
        if (openAni) return;      
        int num = PlayMangement.chapterData.stageSerial;
        accountManager.RequestScenarioReward(num);
    }

    public void BoxSetFinish() {
        StartCoroutine(StartBox());
    }

    IEnumerator StartBox() {
        PlayMangement.instance.resultManager.ShowBox();
        yield return new WaitForSeconds(1.0f);
        accountManager.RequestInventories();
        SetBoxAnimation();
    }



    public override void OpenBox() {
        if (openningBox) return;
        if (openAni) return;
        if (AccountManager.Instance.userResource.supplyBox <= 0) return;
        openningBox = true;
        beforeBgmVolume = SoundManager.Instance.bgmController.BGMVOLUME;
        //SoundManager.Instance.bgmController.BGMVOLUME = 0;
        accountManager.RequestRewardInfo();
    }

    public override void SetBoxAnimation() {
        InitBoxObjects();
        transform.Find("ShowBox").gameObject.SetActive(true);
        openCount = 0;
        if(mainSpine == null) 
            mainSpine = transform.Find("ShowBox/BoxSpine").GetComponent<SkeletonGraphic>();
        mainSpine.Initialize(true);
        mainSpine.Update(0);
        SoundManager.Instance.PlaySound(UISfxSound.BOX_APPEAR);
        mainSpine.AnimationState.SetAnimation(0, "01.START", false);
        mainSpine.AnimationState.AddAnimation(1, "02.IDLE", true, 0.5f);
        boxEffect.Initialize(true);
        boxEffect.Update(0);
        boxEffect.gameObject.SetActive(true);
        boxEffect.AnimationState.SetAnimation(0, "00.NOANI", false);
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().Initialize();
        transform.Find("ShowBox/BoxSpine/Image").GetComponent<BoneFollowerGraphic>().boneName = "card";
        //SoundManager.Instance.PlaySound(UISfxSound.BOXOPEN);
        countOfRewards = accountManager.rewardList.Length;
        transform.Find("ShowBox/BoxSpine/Image/Num").GetComponent<Text>().text = countOfRewards.ToString();
        SetRewards(accountManager.rewardList);
        transform.Find("OpenBox").gameObject.SetActive(true);
    }

    public override void SetRewards(RewardClass[] rewardList) {
        for (int i = 0; i < rewardList.Length; i++) {
            Logger.Log("보상정보 : " + rewardList[i].type);
            SetEachReward(rewardList[i], i);
        }
    }


    public override void SetEachReward(RewardClass reward, int index) {
        Transform boxTarget = transform.Find("OpenBox").GetChild(index);
        Transform effects = transform.Find("EffectSpines");
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Initialize(false);
        effects.GetChild(index).GetComponent<SkeletonGraphic>().Update(0);
        if (reward.type == "card") {
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
                getCrystal.Find("ObjectsParent/Value").GetComponent<TMPro.TextMeshProUGUI>().text = reward.amount.ToString();
            }
            else {
                CheckNewCardList(reward.item);
                getCrystal.gameObject.SetActive(false);
                target.Find("GetCrystalEffect").gameObject.SetActive(false);
            }
        }
        else if (reward.type == "hero") {
            Transform target = boxTarget.Find("Hero");
            target.SetSiblingIndex(1);
            target.gameObject.SetActive(true);
            target.GetChild(0).GetComponent<Image>().sprite = accountManager.resource.heroPortraite[reward.item + "_button"];
            target.GetChild(0).name = reward.item;
            target.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            target.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OpenHeroInfoBtn(reward.item));
            target.Find("Value").gameObject.SetActive(reward.amount < 100);
            target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();
            if (!target.Find("Value").gameObject.activeSelf) {
                Transform getCrystal = target.Find("GetCrystal");
                getCrystal.gameObject.SetActive(true);
                getCrystal.Find("ObjectsParent").gameObject.SetActive(false);
                getCrystal.Find("ObjectsParent/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + reward.amount.ToString();
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

    protected override void CheckNewCardList(string cardId) {
        dataModules.CollectionCard cardData = accountManager.allCardsDic[cardId];
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
    }

    public override void CloseBoxOpen() {
        if (openAni) return;
        InitBoxObjects();
        Transform boxParent = transform.Find("OpenBox");
        //SoundManager.Instance.bgmController.BGMVOLUME = beforeBgmVolume;
        boxParent.gameObject.SetActive(false);
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ShowBox/Text").gameObject.SetActive(true);
        transform.Find("ShowBox/InfoText").gameObject.SetActive(false);
        transform.Find("ShowBox/Shadow").gameObject.SetActive(false);
        transform.Find("Buttons/AdButton").gameObject.SetActive(false);
        transform.Find("Buttons/ExitButton").gameObject.SetActive(false);
        openningBox = false;
        if (multipleBoxes != null && multipleBoxes.Count > 0) {
            multipleBoxes.RemoveAt(0);
            if (multipleBoxes.Count > 0)
                SetRewardBoxAnimation(multipleBoxes[0].ToArray());
        }
    }
}
