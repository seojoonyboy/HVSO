using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using System;
using System.Linq;
using Fbl_UIModule;

public class MenuHeroInfo : MonoBehaviour
{    
    private AccountManager accountManager;
    private Fbl_Translator translator;

    public static MenuHeroInfo heroInfoWindow;
    bool tierUpHero = false;
    string heroId;

    GameObject teirUpModal;
    dataModules.HeroInventory heroData;
    int nowTier;
    bool realInventory = false;
    private void Awake() {
        heroInfoWindow = this;
        init();
        gameObject.SetActive(false);
    }

    private void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, HeroModified);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, StartAni);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, HeroModified);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, StartAni);
    }

    private void init() {
        accountManager = AccountManager.Instance;
        translator = accountManager.GetComponent<Fbl_Translator>();
    }

    // Start is called before the first frame update
    public void SetHeroInfoWindow(string heroId) {
        this.heroId = heroId;
        if (accountManager == null) init();
        heroData = new dataModules.HeroInventory();
        foreach (dataModules.HeroInventory heroes in accountManager.allHeroes) {
            if (heroes.id == heroId) {
                heroData = heroes;
                break;
            }
        }
        transform.Find("Image/Human").gameObject.SetActive(heroData.camp == "human");
        transform.Find("Image/Orc").gameObject.SetActive(!(heroData.camp == "human"));
        transform.Find("HeroDialog/Name").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;
        transform.Find("HeroSpines").GetChild(0).gameObject.SetActive(false);
        int myHeroTier = 0;
        Transform heroSpine = transform.Find("HeroSpines/" + heroData.id);
        SliderAssetController bottomSlider = transform.Find("HeroLevel/Exp").GetComponent<SliderAssetController>();
        
        var heroLevelUiSet = transform.Find("HeroLvUISet").GetComponent<HeroLevelUISet>();
        
        for (int i = 0; i < 3; i++)
            transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(false);
        
        //영웅을 조각까지도 갖고 있지 않은 경우
        if (!accountManager.myHeroInventories.ContainsKey(heroId)) {
            transform.Find("HeroSpines/lock").gameObject.SetActive(true);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
            transform.Find("HeroLevel/Exp").gameObject.SetActive(true);
            transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
            bottomSlider.textOn = true;
            bottomSlider.SetSliderAmount(0, 10);

            heroData = accountManager.allHeroes.Find(x => x.id == heroId);
            string heroName = translator.GetLocalizedText("Hero", "hero_pc_" + heroData.id + "_name");
            heroLevelUiSet.InitExpGage(
                30,
                0,
                heroName,
                heroData.lv
            );
        }
        //영웅을 갖고 있는 경우
        else {
            dataModules.HeroInventory myHeroData = accountManager.myHeroInventories[heroId];
            myHeroTier = myHeroData.tier;
            transform.Find("HeroSpines/lock").gameObject.SetActive(false);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1);
            nowTier = myHeroData.tier;

            //갖고 있으나 조각 상태인 경우
            if (nowTier == 0) {
                transform.Find("HeroSpines/lock").gameObject.SetActive(true);
                heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
                
                heroLevelUiSet.InitExpGage(
                    30,
                    30,
                    myHeroData.name, 
                    myHeroData.lv,
                    true
                );
            }
            else {
                for (int i = 0; i < nowTier; i++)
                    transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(true);
                
                heroLevelUiSet.InitExpGage(
                    myHeroData.exp + myHeroData.nextExp,
                    myHeroData.exp,
                    myHeroData.name, 
                    myHeroData.lv,
                    true
                );
            }

            if (myHeroData.nextTier != null) {
                float fillExp = (float)myHeroData.piece / myHeroData.nextTier.piece;
                if (fillExp >= 1) {
                    transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(true);
                    SkeletonGraphic upgradeSpine = transform.Find("HeroLevel/TierUpBtn/UpgradeSpine").GetComponent<SkeletonGraphic>();
                    upgradeSpine.Initialize(false);
                    upgradeSpine.Update(0);
                    upgradeSpine.AnimationState.SetAnimation(0, "animation", true);
                    bottomSlider.textOn = false;
                    bottomSlider.SetSliderAmount(1, 1);
                }
                else {
                    transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
                    bottomSlider.textOn = true;
                    bottomSlider.SetSliderAmount(myHeroData.piece, myHeroData.nextTier.piece);
                }
            }
            else {                
                transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
                bottomSlider.textOn = true;
                bottomSlider.SetSliderAmount(1, 1);
                bottomSlider.transform.Find("Slider/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = "MAX";
            }
            
        }
        heroSpine.gameObject.SetActive(true);
        heroSpine.SetAsFirstSibling();

        Transform classWindow = transform.Find("ClassInfo");
        string class1 = heroData.heroClasses[0];
        string class2 = heroData.heroClasses[1];
        classWindow.Find("Class1/ClassImg").GetComponent<Image>().sprite = accountManager.resource.classImage[class1];
        string convertKey = string.Format("class_{0}_name", class1);
        classWindow.Find("Class1/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertKey);
        string convertInfo = string.Format("class_{0}_txt", class1);
        classWindow.Find("Class1/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertInfo);
        
        classWindow.Find("Class2/ClassImg").GetComponent<Image>().sprite = accountManager.resource.classImage[class2];
        convertKey = string.Format("class_{0}_name", class2);
        classWindow.Find("Class2/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertKey);
        convertInfo = string.Format("class_{0}_txt", class2);
        classWindow.Find("Class2/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertInfo);

        Transform skillWindow = transform.Find("SkillInfo");
        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[0].id);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(heroData.heroCards[0].skills.desc);
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[1].id);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(heroData.heroCards[1].skills.desc);

        Transform abilityWindow = transform.Find("AbilityInfo");
        abilityWindow.Find("Ability1/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[0];
        abilityWindow.Find("Ability2/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[1];
        //for (int i = 0; i < 2; i++) {
        //    string traitKey = GetTraitKey(heroData.traitText[i]);
        //    if (i == 0)
        //        abilityWindow.Find("Ability1/AbilityImg").GetComponent<Image>().sprite = accountManager.resource.traitIcons[traitKey];
        //    else
        //        abilityWindow.Find("Ability2/AbilityImg").GetComponent<Image>().sprite = accountManager.resource.traitIcons[traitKey];
        //}
        abilityWindow.Find("Ability1/AbilityImg/Block").gameObject.SetActive(myHeroTier < 2);
        abilityWindow.Find("Ability2/AbilityImg/Block").gameObject.SetActive(myHeroTier < 3);

        SetHeroDialog(heroData.flavorText, heroData.camp == "human");
        EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);

        OpenClassWindow();
    }

    string GetTraitKey(string trait) {
        string traitKey = "";
        switch (trait) {
            case "최대체력 +2":
                traitKey = "health_max_2";
                break;
            case "최대 실드 개수 +1":
                traitKey = "shield_max_1";
                break;
            case "실드게이지 충전량 최소치 +1":
                traitKey = "shield_min_charge_1";
                break;
            case "마법 페이즈에 자원 1 획득":
                traitKey = "magic_phase_1";
                break;
            case "마법의 주문 공격력 +1":
                traitKey = "magic_power_1";
                break;
            case "실드게이지 충전량 최대치 +1":
                traitKey = "shield_max_charge_1";
                break;
            case "tool 카드 사용 비용 -1":
                traitKey = "toolcard_use_1";
                break;
        }
        return traitKey;
    }

    public void ClickHeroTierUp() {
        int cost = accountManager.myHeroInventories[heroId].nextTier.crystal;
        if (accountManager.userResource.crystal >= cost) {
            transform.Find("HeroLevel/TierUpBtn/UpgradeSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation2", false);
            teirUpModal = Modal.instantiate("마나 수정이 " + cost.ToString() + "개 소모됩니다. 진행 하시겠습니까?", Modal.Type.YESNO, TierUpHero, CancelTierUp);
            EscapeKeyController.escapeKeyCtrl.AddEscape(CancelTierUp);
        }
        else {
            int lack = cost - accountManager.userResource.crystal;
            teirUpModal = Modal.instantiate("마나 수정이 " + lack.ToString() + "개 부족합니다.", Modal.Type.CHECK);
            EscapeKeyController.escapeKeyCtrl.AddEscape(CancelTierUp);
        }
    }

    public void CancelTierUp() {
        DestroyImmediate(teirUpModal, true);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CancelTierUp);
        transform.Find("HeroLevel/TierUpBtn/UpgradeSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", true);
    }

    public void TierUpHero() {
        if (tierUpHero) return;
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        tierUpHero = true;
        StartCoroutine(SetUpTeirUp());
        accountManager.RequestHeroTierUp(heroId);


    }

    IEnumerator SetUpTeirUp() {
        transform.parent.Find("BackButtonArea/BackButton").gameObject.SetActive(false);
        transform.Find("TierUpField").gameObject.SetActive(true);
        transform.Find("TierUpField/Name/NameText").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;

        /* 영웅 특성 이미지 설정
        if (nowTier > 0)
            transform.Find("TierUpField/Ability/AbilityImg").GetComponent<Image>().sprite = accountManager.resource.traitIcons[GetTraitKey(heroData.traitText[nowTier - 1])];
            */

        transform.Find("BackGround").GetComponent<Image>().sprite = accountManager.resource.campBackgrounds["hero_tier" + (nowTier + 1).ToString()];
        Color effectColor = new Color();
        switch (nowTier) {
            case 0:
                effectColor = new Color(0.56f, 1, 0.31f);
                break;
            case 1:
                effectColor = new Color(0.16f, 0.78f, 1);
                break;
            case 2:
                effectColor = new Color(1, 0.31f, 0.98f);
                break;
        }
        transform.Find("BackGround/SpinEffect/ColorLight").GetComponent<Image>().color = effectColor;
        transform.Find("BackGround").gameObject.SetActive(true);
        for (int i = 0; i < nowTier; i++)
            transform.Find("TierUpField/Stars").GetChild(i).Find("Star").gameObject.SetActive(true);
        GameObject heroSpines = transform.Find("HeroSpines").gameObject;
        heroSpines.transform.SetSiblingIndex(transform.childCount - 2);
        iTween.MoveTo(heroSpines, iTween.Hash("y", 0, "islocal", true, "time", 0.8f));
        iTween.ScaleTo(heroSpines, iTween.Hash("x", 1.2f, "y", 1.2f, "islocal", true, "time", 0.8f));
        Image[] colors = transform.Find("TierUpField").GetComponentsInChildren<Image>();
        float colorA = 0;
        while (colorA < 1.0f) {
            colorA += 4.0f * Time.deltaTime;
            for (int i = 0; i < colors.Length; i++) {
                colors[i].color = new Color(1, 1, 1, colorA);
                transform.Find("BackGround").GetComponent<Image>().color = new Color(1, 1, 1, colorA);
            }
            yield return null;
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseHeroTierUp);
    }

    private void HeroModified(Enum Event_Type, Component Sender, object Param) {
        accountManager.RequestInventories();
        accountManager.RequestUserInfo();
        realInventory = true;
    }

    public void StartAni(Enum Event_Type, Component Sender, object Param) {
        if (realInventory && gameObject.activeSelf) {
            StartCoroutine(HeroMakingAni());
            AccountManager.Instance.RequestAchievementInfo();
            realInventory = false;
        }
    }

    IEnumerator HeroMakingAni() {
        yield return new WaitForSeconds(0.5f);
        transform.Find("TierUpField/LevelUp").gameObject.SetActive(true);
        dataModules.HeroInventory heroData = accountManager.myHeroInventories[heroId];
        //if (heroData.camp == "human")
        //    CardDictionaryManager.cardDictionaryManager.RefreshHumanHero();
        //else
        //    CardDictionaryManager.cardDictionaryManager.RefreshOrcHero();
        SkeletonGraphic spine = transform.Find("TierUpField/Stars").GetChild(nowTier).Find("StartSpine").GetComponent<SkeletonGraphic>();
        spine.Initialize(true);
        spine.Update(0);
        spine.AnimationState.SetAnimation(0, "animation", false);
        SetHeroInfoWindow(heroId);
        yield return new WaitForSeconds(0.3f);
        transform.Find("TierUpField/Stars").GetChild(nowTier - 1).Find("Star").gameObject.SetActive(true);
        transform.Find("TierUpField/Name").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        /*  영웅 특성 획득 애니메이션
        if (heroData.tier > 1) {
            transform.Find("TierUpField/Ability").gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
        */
        transform.Find("TierUpField/Button").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        tierUpHero = false;
    }

    public void CloseHeroTierUp() {
        if (tierUpHero) return;
        transform.parent.Find("BackButtonArea/BackButton").gameObject.SetActive(false);
        Transform heroSpines = transform.Find("HeroSpines");
        heroSpines.SetSiblingIndex(2);
        heroSpines.localScale = Vector3.one;
        heroSpines.localPosition = new Vector3(0, 193, 0);
        transform.Find("TierUpField").GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
        transform.Find("BackGround").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        transform.Find("BackGround").gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
            transform.Find("TierUpField/Stars").GetChild(i).Find("Star").gameObject.SetActive(false);
        transform.Find("TierUpField").gameObject.SetActive(false);
        transform.Find("TierUpField/LevelUp").gameObject.SetActive(false);
        transform.Find("TierUpField/Name").gameObject.SetActive(false);
        transform.Find("TierUpField/Ability").gameObject.SetActive(false);
        transform.Find("TierUpField/Button").gameObject.SetActive(false);
        if (heroData.camp == "human")
            CardDictionaryManager.cardDictionaryManager.RefreshHumanHero();
        else
            CardDictionaryManager.cardDictionaryManager.RefreshOrcHero();
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseHeroTierUp);
    }

    public void OpenClassWindow() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("Buttons/ClassBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("Buttons/SkillBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/AbilityBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("ClassInfo").gameObject.SetActive(true);
        transform.Find("SkillInfo").gameObject.SetActive(false);
        transform.Find("AbilityInfo").gameObject.SetActive(false);
    }

    public void OpenSkillWindow() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("Buttons/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/SkillBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("Buttons/AbilityBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("ClassInfo").gameObject.SetActive(false);
        transform.Find("SkillInfo").gameObject.SetActive(true);
        transform.Find("AbilityInfo").gameObject.SetActive(false);
    }

    public void OpenAbilityWindow() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("Buttons/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/SkillBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/AbilityBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("ClassInfo").gameObject.SetActive(false);
        transform.Find("SkillInfo").gameObject.SetActive(false);
        transform.Find("AbilityInfo").gameObject.SetActive(true);
    }

    public void SetHeroDialog(string dialog, bool isHuman) {
        Transform dialogWindow = transform.Find("HeroDialog");
        string[] separatingStrings = { "<title>", "</title>", "<desc>", "</desc>" };
        string[] heroText = dialog.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
        dialogWindow.Find("OrcBackground").gameObject.SetActive(!isHuman);
        dialogWindow.Find("HumanBackground").gameObject.SetActive(isHuman);
        dialogWindow.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = heroText[0];
        dialogWindow.Find("Dialog").GetComponent<TMPro.TextMeshProUGUI>().text = heroText[1];
    }

    public void OpenHeroDialog(bool open) {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("HeroDialog").gameObject.SetActive(open);
    }   
}
