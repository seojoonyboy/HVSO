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
        if (transform.Find("HumanSelect").GetChild(0).gameObject.activeSelf) return;
        transform.Find("HumanSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("HeroSpines/HumanSpines").gameObject.SetActive(true);
        transform.Find("HeroSpines/OrcSpines").gameObject.SetActive(false);
        isHuman = true;
        SetHeroInfo("h10001");
    }

    public void SetOrcHeroes() {
        if (transform.Find("OrcSelect").GetChild(0).gameObject.activeSelf) return;
        transform.Find("HumanSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("HeroSpines/HumanSpines").gameObject.SetActive(false);
        transform.Find("HeroSpines/OrcSpines").gameObject.SetActive(true);
        isHuman = false;
        SetHeroInfo("h10002");
    }

    public void SetHeroInfo(string heroId) {
        HeroInventory heroData = AccountManager.Instance.myHeroInventories[heroId];
        selectedHeroId = heroId;
        

        transform.Find("HeroInfo/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;
        transform.Find("HeroInfo/Class1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[0]];
        transform.Find("HeroInfo/Class2").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[1]];
        transform.Find("HeroCards/Cards/Card1").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[0].cardId, isHuman);
        transform.Find("HeroCards/Cards/Card2").GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[1].cardId, isHuman);
        transform.Find("HeroCards/Card1Info/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].name;
        transform.Find("HeroCards/Card1Info/CardText").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[0].skills[0].desc;
        transform.Find("HeroCards/Card2Info/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].name;
        transform.Find("HeroCards/Card2Info/CardText").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.heroCards[1].skills[0].desc;
    }

    public void OpenTemplateDeckCanvas() {
        templateDeckCanvas.gameObject.SetActive(true);
        templateDeckCanvas.SetTemplateNewDecks(selectedHeroId, isHuman);
        gameObject.SetActive(false);
        hudController.SetBackButton(() => ExitTemplateCanvas());
    }

    public void ExitTemplateCanvas() {
        gameObject.SetActive(true);
        templateDeckCanvas.gameObject.SetActive(false);
        templateDeckCanvas.CancelSelectDeck();
        hudController.SetBackButton(() => deckSettingManager.ExitHeroSelect());
    }
}
    