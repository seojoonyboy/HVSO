using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;

public class HeroSelectController : MonoBehaviour
{
    [SerializeField] TemplateMenu templateDeckCanvas;
    [SerializeField] HUDController hudController;
    [SerializeField] DeckSettingManager deckSettingManager;
    [SerializeField] HorizontalScrollSnap humanHeroScroll;
    [SerializeField] HorizontalScrollSnap orcHeroScroll;

    public string selectedHeroId;
    bool isHuman;
    public void SetHumanHeroes() {
        transform.Find("InnerCanvas/RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("InnerCanvas/RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroSpines/HumanSpines").gameObject.SetActive(true);
        SetHeroSpine(transform.Find("InnerCanvas/HeroSpines/HumanSpines/Content"));
        transform.Find("InnerCanvas/HeroSpines/OrcSpines").gameObject.SetActive(false);
        transform.Find("InnerCanvas/BackgroundImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["human"];
        isHuman = true;
        humanHeroScroll.GoToScreen(0);
        SetHeroInfo(0, true);
        OpenClassInfo();
    }

    public void SetOrcHeroes() {
        transform.Find("InnerCanvas/RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("InnerCanvas/RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroSpines/HumanSpines").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroSpines/OrcSpines").gameObject.SetActive(true);
        SetHeroSpine(transform.Find("InnerCanvas/HeroSpines/OrcSpines/Content"));
        transform.Find("InnerCanvas/BackgroundImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["orc"];
        isHuman = false;
        orcHeroScroll.GoToScreen(0);        
        SetHeroInfo(0, false);
        OpenClassInfo();
    }

    void SetHeroSpine(Transform spineParent) {
        for(int i = 0; i < spineParent.childCount; i++) {
            if (!AccountManager.Instance.myHeroInventories.ContainsKey(spineParent.GetChild(i).name)) {
                spineParent.GetChild(i).Find("Locked").gameObject.SetActive(true);
                spineParent.GetChild(i).GetChild(0).GetComponent<SkeletonGraphic>().color = new Color(0.3f, 0.3f, 0.3f);
            }
            else {
                spineParent.GetChild(i).Find("Locked").gameObject.SetActive(false);
                spineParent.GetChild(i).GetChild(0).GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1);
                if(AccountManager.Instance.myHeroInventories[spineParent.GetChild(i).name].tier == 0) {
                    spineParent.GetChild(i).Find("Locked").gameObject.SetActive(true);
                    spineParent.GetChild(i).GetChild(0).GetComponent<SkeletonGraphic>().color = new Color(0.3f, 0.3f, 0.3f);
                }
            }
        }
    }

    public void SetHeroInfo(int heroIndex, bool isHuman) {
        string heroId;
        if (isHuman)
            heroId = humanHeroScroll.transform.Find("Content").GetChild(heroIndex).name;
        else
            heroId = orcHeroScroll.transform.Find("Content").GetChild(heroIndex).name;
        HeroInventory heroData = new HeroInventory();
        foreach (dataModules.HeroInventory hero in AccountManager.Instance.allHeroes) {
            if (hero.id == heroId) {
                heroData = hero;
                break;
            }
        }
        selectedHeroId = heroId;
        if (AccountManager.Instance.myHeroInventories.ContainsKey(heroId)) {
            for (int i = 0; i < 3; i++)
                transform.Find("InnerCanvas/HeroLevel").GetChild(i).GetChild(0).gameObject.SetActive(i < AccountManager.Instance.myHeroInventories[heroId].tier);
        }
        else
            for (int i = 0; i < 3; i++)
                transform.Find("InnerCanvas/HeroLevel").GetChild(i).GetChild(0).gameObject.SetActive(false);

        Transform classWindow = transform.Find("InnerCanvas/HeroInfo/ClassWindow");
        Transform skillWindow = transform.Find("InnerCanvas/HeroInfo/SkillWindow");
        Transform abilityWindow = transform.Find("InnerCanvas/HeroInfo/AbilityWindow");
        

        transform.Find("InnerCanvas/HeroInfo/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;

        classWindow.Find("Class1/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[0]];
        classWindow.Find("Class1/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].name;
        classWindow.Find("Class1/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].info;
        classWindow.Find("Class2/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[1]];
        classWindow.Find("Class2/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[1]].name;
        classWindow.Find("Class2/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[1]].info;

        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[0].cardId, isHuman);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Translator>().DialogSetRichText(heroData.heroCards[0].skills[0].desc);
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[1].cardId, isHuman);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Translator>().DialogSetRichText(heroData.heroCards[1].skills[0].desc);

        abilityWindow.Find("Ability1/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[0];
        abilityWindow.Find("Ability2/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[1];
        for (int i = 0; i < 2; i++) {
            string traitKey = "";
            switch (heroData.traitText[i]) {
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
            if (i == 0)
                abilityWindow.Find("Ability1/AbilityImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.traitIcons[traitKey];
            else
                abilityWindow.Find("Ability2/AbilityImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.traitIcons[traitKey];
        }

        transform.Find("InnerCanvas/OpenTemplateButton").gameObject.SetActive(AccountManager.Instance.myHeroInventories.ContainsKey(heroId) 
            && AccountManager.Instance.myHeroInventories[heroId].tier > 0);
    }

    public void OpenTemplateDeckCanvas() {
        templateDeckCanvas.gameObject.SetActive(true);
        templateDeckCanvas.SetTemplateNewDecks(selectedHeroId, isHuman);
        gameObject.SetActive(false);
        hudController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
        hudController.SetBackButton(() => ExitTemplateCanvas_Edit());
        EscapeKeyController.escapeKeyCtrl.AddEscape(ExitTemplateCanvas_Edit);
    }

    public void ExitTemplateCanvas_Edit() {
        gameObject.SetActive(true);
        templateDeckCanvas.gameObject.SetActive(false);
        templateDeckCanvas.CancelSelectDeck();
        hudController.SetHeader(HUDController.Type.HIDE);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(ExitTemplateCanvas_Edit);
    }

    public void OpenClassInfo() {
        transform.Find("InnerCanvas/HeroInfo/ClassWindow").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/ClassBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/SkillWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/SkillBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/AbilityWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/AbilityBtn/UnSelected").gameObject.SetActive(true);
        

    }

    public void OpenSkillInfo() {
        transform.Find("InnerCanvas/HeroInfo/ClassWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/SkillWindow").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/SkillBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/AbilityWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/AbilityBtn/UnSelected").gameObject.SetActive(true);
    }

    public void OpenAbillityInfo() {
        transform.Find("InnerCanvas/HeroInfo/ClassWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/SkillWindow").gameObject.SetActive(false);
        transform.Find("InnerCanvas/HeroInfo/SkillBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/AbilityWindow").gameObject.SetActive(true);
        transform.Find("InnerCanvas/HeroInfo/AbilityBtn/UnSelected").gameObject.SetActive(false);
    }



    public void ScrollHeros(bool isHuman) {
        if (isHuman)
            SetHeroInfo(humanHeroScroll.CurrentPage, true);
        else
            SetHeroInfo(orcHeroScroll.CurrentPage, false);
    }
}
