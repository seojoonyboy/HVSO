using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using System;
using System.Linq;

public class MenuHeroInfo : MonoBehaviour
{
    private AccountManager accountManager;
    private Translator translator;

    public static MenuHeroInfo heroInfoWindow;
    bool tierUpHero = false;
    string heroId;

    GameObject teirUpModal;
    int nowTier;
    private void Awake() {
        heroInfoWindow = this;
        init();
        gameObject.SetActive(false);
    }

    private void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, HeroModified);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, StartAni);
    }

    private void init() {
        accountManager = AccountManager.Instance;
        translator = accountManager.GetComponent<Translator>();
    }

    // Start is called before the first frame update
    public void SetHeroInfoWindow(string heroId) {
        this.heroId = heroId;
        if (accountManager == null) init();
        dataModules.HeroInventory hero = new dataModules.HeroInventory();
        foreach (dataModules.HeroInventory heroes in accountManager.allHeroes) {
            if (heroes.id == heroId) {
                hero = heroes;
                break;
            }
        }
        transform.Find("Image/Human").gameObject.SetActive(hero.camp == "human");
        transform.Find("Image/Orc").gameObject.SetActive(!(hero.camp == "human"));
        transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        transform.Find("HeroDialog/Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        transform.Find("HeroSpines").GetChild(0).gameObject.SetActive(false);
        Transform heroSpine = transform.Find("HeroSpines/" + hero.id);
        for (int i = 0; i < 3; i++)
            transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(false);
        if (!accountManager.myHeroInventories.ContainsKey(heroId)) {
            transform.Find("HeroSpines/lock").gameObject.SetActive(true);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
            transform.Find("HeroLevel/Exp").gameObject.SetActive(true);
            transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
            transform.Find("HeroLevel/Exp/Slider/Value").GetComponent<Image>().fillAmount = 0;
            transform.Find("HeroLevel/Exp/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text ="0/10";
        }
        else {
            dataModules.HeroInventory heroData = accountManager.myHeroInventories[heroId];
            transform.Find("HeroSpines/lock").gameObject.SetActive(false);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1);
            nowTier = heroData.tier;
            if (nowTier == 0) {
                transform.Find("HeroSpines/lock").gameObject.SetActive(true);
                heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
            }
            else {
                for (int i = 0; i < nowTier; i++)
                    transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
            if (heroData.next_level != null) {
                float fillExp = (float)heroData.piece / heroData.next_level.piece;
                if (fillExp >= 1) {
                    transform.Find("HeroLevel/Exp/ValueText").gameObject.SetActive(false);
                    transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(true);
                    transform.Find("HeroLevel/TierUpBtn/UpgradeSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", true);
                    transform.Find("HeroLevel/Exp/Slider/Value").localPosition = new Vector3(-490, 0, 0);
                }
                else {
                    transform.Find("HeroLevel/Exp/ValueText").gameObject.SetActive(true);
                    transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
                    transform.Find("HeroLevel/Exp/Slider/Value").localPosition = new Vector3(-1470 + (980 * fillExp), 0, 0);
                    transform.Find("HeroLevel/Exp/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.piece + "/" + heroData.next_level.piece;
                }
            }
            else {
                transform.Find("HeroLevel/Exp/ValueText").gameObject.SetActive(true);
                transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
                transform.Find("HeroLevel/Exp/Slider/Value").localPosition = new Vector3(-1470, 0, 0);
                transform.Find("HeroLevel/Exp/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = "MAX";
            }
        }
        heroSpine.gameObject.SetActive(true);
        heroSpine.SetAsFirstSibling();

        Transform classWindow = transform.Find("ClassInfo");
        classWindow.Find("Class1/ClassImg").GetComponent<Image>().sprite = accountManager.resource.classImage[hero.heroClasses[0]];
        classWindow.Find("Class1/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroClasses[0];
        classWindow.Find("Class1/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.resource.classInfo[hero.heroClasses[0]].info;
        classWindow.Find("Class2/ClassImg").GetComponent<Image>().sprite = accountManager.resource.classImage[hero.heroClasses[1]];
        classWindow.Find("Class2/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroClasses[1];
        classWindow.Find("Class2/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = accountManager.resource.classInfo[hero.heroClasses[1]].info;

        Transform skillWindow = transform.Find("SkillInfo");
        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[0].id);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(hero.heroCards[0].skills[0].desc);
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[1].id);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(hero.heroCards[1].skills[0].desc);

        Transform abilityWindow = transform.Find("AbilityInfo");
        abilityWindow.Find("Ability1/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = hero.traitText[0];
        abilityWindow.Find("Ability2/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = hero.traitText[1];

        SetHeroDialog(hero.flavorText, hero.camp == "human");
        EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);

        OpenClassWindow();
    }

    public void ClickHeroTierUp() {
        int cost = accountManager.myHeroInventories[heroId].next_level.crystal;
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
        transform.Find("TierUpField").gameObject.SetActive(true);
        for (int i = 0; i < nowTier; i++)
            transform.Find("TierUpField/Stars").GetChild(i).Find("Star").gameObject.SetActive(true);
        GameObject heroSpines = transform.Find("HeroSpines").gameObject;
        heroSpines.transform.SetAsLastSibling();
        iTween.MoveTo(heroSpines, iTween.Hash("y", 0, "islocal", true, "time", 0.8f));
        iTween.ScaleTo(heroSpines, iTween.Hash("x", 1.2f, "y", 1.2f, "islocal", true, "time", 0.8f));
        Image[] colors = transform.Find("TierUpField").GetComponentsInChildren<Image>();
        float colorA = 0;
        while (colorA < 1.0f) {
            colorA += 4.0f * Time.deltaTime;
            for(int i = 0; i < colors.Length; i++) 
                colors[i].color = new Color(1, 1, 1, colorA);
            yield return null;
        }
    }

    private void HeroModified(Enum Event_Type, Component Sender, object Param) {
        accountManager.RequestInventories();
        
    }

    public void StartAni(Enum Event_Type, Component Sender, object Param) {
        StartCoroutine(HeroMakingAni());
    }

    IEnumerator HeroMakingAni() {
        yield return new WaitForSeconds(0.5f);
        dataModules.HeroInventory heroData = accountManager.myHeroInventories[heroId];
        if (heroData.camp == "human")
            CardDictionaryManager.cardDictionaryManager.RefreshHumanHero();
        else
            CardDictionaryManager.cardDictionaryManager.RefreshOrcHero();
        SkeletonGraphic spine = transform.Find("TierUpField/Stars").GetChild(nowTier).Find("StartSpine").GetComponent<SkeletonGraphic>();
        spine.Initialize(true);
        spine.Update(0);
        spine.AnimationState.SetAnimation(0, "animation", false);
        SetHeroInfoWindow(heroId);
        yield return new WaitForSeconds(0.3f);
        transform.Find("TierUpField/Stars").GetChild(nowTier - 1).Find("Star").gameObject.SetActive(true);
        tierUpHero = false;
    }

    public void CloseHeroTierUp() {
        if (tierUpHero) return;
        Transform heroSpines = transform.Find("HeroSpines");
        heroSpines.SetSiblingIndex(2);
        heroSpines.localScale = Vector3.one;
        heroSpines.localPosition = new Vector3(0, 193, 0);
        transform.Find("TierUpField").GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
        for (int i = 0; i < 3; i++)
            transform.Find("TierUpField/Stars").GetChild(i).Find("Star").gameObject.SetActive(false);
        transform.Find("TierUpField").gameObject.SetActive(false);
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
