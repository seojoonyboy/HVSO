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

        SetAlert(transform.Find("InnerCanvas/HeroSpines/HumanSpines/Content"));
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

        SetAlert(transform.Find("InnerCanvas/HeroSpines/OrcSpines/Content"));
    }

    private void SetAlert(Transform content) {
        NewAlertManager alertManager = NewAlertManager.Instance;
        var unlockConditionList = alertManager.GetUnlockCondionsList();
        var deck_edit_conditions = unlockConditionList.FindAll(x => x.Contains(NewAlertManager.ButtonName.DECK_EDIT.ToString()));
        foreach(Transform child in content) {
            if(deck_edit_conditions != null) {
                if(deck_edit_conditions.Exists(x => x.Contains(selectedHeroId))){
                    if(child.Find("alert") == null) {
                        GameObject alert = Instantiate(alertManager.alertPref);
                        alert.transform.SetParent(child);
                        alert.name = "alert";
                        alert.transform.SetAsLastSibling();
                        RectTransform rect = alert.GetComponent<RectTransform>();
                        rect.anchorMin = new Vector2(1, 1);
                        rect.anchorMax = new Vector2(1, 1);
                        rect.offsetMax = new Vector2(-20f, 0);
                        rect.offsetMin = new Vector2(-20f, 0);
                    }
                }
                else {
                    if (child.Find("alert") != null) Destroy(child.Find("alert").gameObject);
                }
            }
            else {
                if (child.Find("alert") != null) Destroy(child.Find("alert").gameObject);
            }
        }
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
        int myHeroTier = 0;
        if (AccountManager.Instance.myHeroInventories.ContainsKey(heroId)) {
            myHeroTier = AccountManager.Instance.myHeroInventories[heroId].tier;
            for (int i = 1; i < 4; i++)
                transform.Find("InnerCanvas/HeroLevel").GetChild(i).GetChild(0).gameObject.SetActive(i - 1 < AccountManager.Instance.myHeroInventories[heroId].tier);
        }
        else
            for (int i = 1; i < 4; i++)
                transform.Find("InnerCanvas/HeroLevel").GetChild(i).GetChild(0).gameObject.SetActive(false);

        Transform classWindow = transform.Find("InnerCanvas/HeroInfo/ClassWindow");
        Transform skillWindow = transform.Find("InnerCanvas/HeroInfo/SkillWindow");
        Transform abilityWindow = transform.Find("InnerCanvas/HeroInfo/AbilityWindow");
        

        transform.Find("InnerCanvas/HeroInfo/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;

        string class1 = heroData.heroClasses[0];
        string class2 = heroData.heroClasses[1];
        classWindow.Find("Class1/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[class1];
        string convertKey = string.Format("class_{0}_name", class1);
        classWindow.Find("Class1/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertKey);
        string convertInfo = string.Format("class_{0}_txt", class1);
        classWindow.Find("Class1/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertInfo);

        classWindow.Find("Class2/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[class2];
        convertKey = string.Format("class_{0}_name", class2);
        classWindow.Find("Class2/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertKey);
        convertInfo = string.Format("class_{0}_txt", class2);
        classWindow.Find("Class2/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Class", convertInfo);

        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[0].cardId, isHuman);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().DialogSetRichText(heroData.heroCards[0].skills.desc);
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[1].cardId, isHuman);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().DialogSetRichText(heroData.heroCards[1].skills.desc);

        abilityWindow.Find("Ability1/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[0];
        abilityWindow.Find("Ability2/AbilityInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.traitText[1];
        //for (int i = 0; i < 2; i++) {
        //    string traitKey = "";
        //    switch (heroData.traitText[i]) {
        //        case "???????????? +2":
        //            traitKey = "health_max_2";
        //            break;
        //        case "?????? ?????? ?????? +1":
        //            traitKey = "shield_max_1";
        //            break;
        //        case "??????????????? ????????? ????????? +1":
        //            traitKey = "shield_min_charge_1";
        //            break;
        //        case "?????? ???????????? ?????? 1 ??????":
        //            traitKey = "magic_phase_1";
        //            break;
        //        case "????????? ?????? ????????? +1":
        //            traitKey = "magic_power_1";
        //            break;
        //        case "??????????????? ????????? ????????? +1":
        //            traitKey = "shield_max_charge_1";
        //            break;
        //        case "tool ?????? ?????? ?????? -1":
        //            traitKey = "toolcard_use_1";
        //            break;
        //    }
        //    if (i == 0)
        //        abilityWindow.Find("Ability1/AbilityImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.traitIcons[traitKey];
        //    else
        //        abilityWindow.Find("Ability2/AbilityImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.traitIcons[traitKey];
        //}
        abilityWindow.Find("Ability1/AbilityImg/Block").gameObject.SetActive(myHeroTier < 2);
        abilityWindow.Find("Ability2/AbilityImg/Block").gameObject.SetActive(myHeroTier < 3);
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

        NewAlertManager.Instance.CheckRemovable(NewAlertManager.ButtonName.DECK_EDIT, selectedHeroId);
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
