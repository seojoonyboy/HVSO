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

    private void Awake() {
        heroInfoWindow = this;
        init();
        gameObject.SetActive(false);
    }

    private void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, HeroModified);
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
        transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        transform.Find("HeroDialog/Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        transform.Find("HeroSpines").GetChild(0).gameObject.SetActive(false);
        Transform heroSpine = transform.Find("HeroSpines/" + hero.id);
        for (int i = 0; i < 3; i++)
            transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(false);
        if (!accountManager.myHeroInventories.ContainsKey(heroId)) {
            transform.Find("HeroSpines/lock").gameObject.SetActive(true);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
        }
        else {
            dataModules.HeroInventory heroData = accountManager.myHeroInventories[heroId];
            transform.Find("HeroSpines/lock").gameObject.SetActive(false);
            heroSpine.GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1);
            if(heroData.tier == 0) {
                transform.Find("HeroSpines/lock").gameObject.SetActive(true);
                heroSpine.GetComponent<SkeletonGraphic>().color = new Color(0.35f, 0.35f, 0.35f);
                
                transform.Find("HeroLevel/Exp/Value").GetComponent<Image>().fillAmount = 0;
                transform.Find("HeroLevel/Exp/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = "0/30";
            }
            else {
                for (int i = 0; i < heroData.tier; i++)
                    transform.Find("HeroLevel/Stars").GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
            float fillExp = (float)heroData.piece / heroData.next_level.piece;
            if (fillExp >= 1) {
                transform.Find("HeroLevel/Exp").gameObject.SetActive(false);
                transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(true);
            }
            else {
                transform.Find("HeroLevel/Exp").gameObject.SetActive(true);
                transform.Find("HeroLevel/TierUpBtn").gameObject.SetActive(false);
                transform.Find("HeroLevel/Exp/Value").GetComponent<Image>().fillAmount = (float)heroData.piece / heroData.next_level.piece;
                transform.Find("HeroLevel/Exp/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.piece + "/" + heroData.next_level.piece;
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
        SetHeroDialog(hero.flavorText, hero.camp == "human");
        EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);

        OpenClassWindow();
    }

    public void TierUpHero() {
        if (tierUpHero) return;
        transform.Find("block").gameObject.SetActive(true);
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        tierUpHero = true;
        accountManager.RequestHeroTierUp(heroId);
    }

    private void HeroModified(Enum Event_Type, Component Sender, object Param) {
        accountManager.RequestInventories();
        StartCoroutine(HeroMakingAni());
    }

    IEnumerator HeroMakingAni() {
        yield return new WaitForSeconds(0.5f);
        if(accountManager.myHeroInventories[heroId].camp == "human")
            CardDictionaryManager.cardDictionaryManager.RefreshHumanHero();
        else
            CardDictionaryManager.cardDictionaryManager.RefreshOrcHero();
        SetHeroInfoWindow(heroId);
        transform.Find("block").gameObject.SetActive(false);
        tierUpHero = false;
    }

    public void OpenClassWindow() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("Buttons/ClassBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("Buttons/SkillBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/AbillityBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("ClassInfo").gameObject.SetActive(true);
        transform.Find("SkillInfo").gameObject.SetActive(false);
    }

    public void OpenSkillWindow() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.Find("Buttons/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("Buttons/SkillBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("Buttons/AbillityBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("ClassInfo").gameObject.SetActive(false);
        transform.Find("SkillInfo").gameObject.SetActive(true);
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
