using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectController : MonoBehaviour
{
    [SerializeField] TemplateMenu templateDeckCanvas;
    [SerializeField] HUDController hudController;
    [SerializeField] DeckSettingManager deckSettingManager;
    public string selectedHeroId;
    bool isHuman;
    public void SetHumanHeroes() {
        if (transform.Find("RaceSelect/HumanSelect").GetChild(0).gameObject.activeSelf) return;
        transform.Find("RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("HeroSpines/HumanSpines").gameObject.SetActive(true);
        transform.Find("HeroSpines/OrcSpines").gameObject.SetActive(false);
        isHuman = true;
        SetHeroInfo("h10001");
        OpenClassInfo();
    }

    public void SetOrcHeroes() {
        if (transform.Find("RaceSelect/OrcSelect").GetChild(0).gameObject.activeSelf) return;
        transform.Find("RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("HeroSpines/HumanSpines").gameObject.SetActive(false);
        transform.Find("HeroSpines/OrcSpines").gameObject.SetActive(true);
        isHuman = false;
        SetHeroInfo("h10002");
        OpenClassInfo();
    }

    public void SetHeroInfo(string heroId) {
        HeroInventory heroData = AccountManager.Instance.myHeroInventories[heroId];
        selectedHeroId = heroId;

        Transform classWindow = transform.Find("HeroInfo/ClassWindow");
        Transform skillWindow = transform.Find("HeroInfo/SkillWindow");

        transform.Find("HeroInfo/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;

        classWindow.Find("Class1/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[0]];
        classWindow.Find("Class1/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].name;
        classWindow.Find("Class1/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].info;
        classWindow.Find("Class2/ClassImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[1]];
        classWindow.Find("Class2/ClassName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].name;
        classWindow.Find("Class2/ClassInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.classInfo[heroData.heroClasses[0]].info;

        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[0].cardId, isHuman);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].skills[0].desc;
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[1].cardId, isHuman);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].skills[0].desc;
    }

    public void OpenTemplateDeckCanvas() {
        templateDeckCanvas.gameObject.SetActive(true);
        templateDeckCanvas.SetTemplateNewDecks(selectedHeroId, isHuman);
        gameObject.SetActive(false);
        hudController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
        hudController.SetBackButton(() => ExitTemplateCanvas());
    }

    public void ExitTemplateCanvas() {
        gameObject.SetActive(true);
        templateDeckCanvas.gameObject.SetActive(false);
        templateDeckCanvas.CancelSelectDeck();
        hudController.SetHeader(HUDController.Type.HIDE);
    }

    public void OpenClassInfo() {
        transform.Find("HeroInfo/ClassWindow").gameObject.SetActive(true);
        transform.Find("HeroInfo/ClassBtn/UnSelected").gameObject.SetActive(false);
        transform.Find("HeroInfo/SkillWindow").gameObject.SetActive(false);
        transform.Find("HeroInfo/SkillBtn/UnSelected").gameObject.SetActive(true);

    }

    public void OpenSkillInfo() {
        transform.Find("HeroInfo/ClassWindow").gameObject.SetActive(false);
        transform.Find("HeroInfo/ClassBtn/UnSelected").gameObject.SetActive(true);
        transform.Find("HeroInfo/SkillWindow").gameObject.SetActive(true);
        transform.Find("HeroInfo/SkillBtn/UnSelected").gameObject.SetActive(false);
    }
}
