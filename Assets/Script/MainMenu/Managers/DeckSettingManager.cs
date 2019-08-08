using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] Canvas humanTemplateCanvas;
    [SerializeField] Canvas orcTemplateCanvas;
    [SerializeField] GameObject cardDictionaryCanvas;
    [SerializeField] Transform humanDeckList;
    [SerializeField] Transform orcDeckList;
    [SerializeField] Transform pageButtons;
    [SerializeField] TMPro.TextMeshProUGUI humanDeckNum;
    [SerializeField] TMPro.TextMeshProUGUI orcDeckNum;
    [SerializeField] HUDController hudController;

    public MenuSceneController menuSceneController;

    MyDecksLoader decksLoader;

    int humanDeckPage = 0;
    int humanCurrentPage = 0;
    int orcDeckPage = 0;
    int orcCurrentPage = 0;

    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnLoadFinished.AddListener(() => { OnDecksInfoLoaded(); });
    }

    public void ClickNewDeck(DeckHandler deck) {
        Canvas templateCanvas;
        if (deck.gameObject.name == "HumanEditDeck") {
            templateCanvas = humanTemplateCanvas;
            templateCanvas.GetComponent<TemplateMenu>().ChangeHeroID("h10001");
        }
        else {
            templateCanvas = orcTemplateCanvas;
            templateCanvas.GetComponent<TemplateMenu>().ChangeHeroID("h10002");
        }
        templateCanvas.gameObject.SetActive(true);
        
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            templateCanvas.GetComponent<TemplateMenu>().ReturnToMenu();
            hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        });
    }

    public void OpenCardDictionary() {
        cardDictionaryCanvas.gameObject.SetActive(true);
        cardDictionaryCanvas.GetComponent<CardDictionaryManager>().SetToOrcCards();

        hudController.SetBackButtonMsg("도감화면");

        hudController.SetBackButton(() => {
            cardDictionaryCanvas.GetComponent<CardDictionaryManager>().CloseDictionaryCanvas();
            hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        });
    }

    public void SetPlayerDecks() {
        int humanDeckCount = 0;
        int orcDeckCount = 0;
        humanDeckPage = 0;
        orcDeckPage = 0;
        humanCurrentPage = 1;
        orcCurrentPage = 1;
        int humanCustomDecks = AccountManager.Instance.humanDecks.Count;
        int orcCustomDecks = AccountManager.Instance.orcDecks.Count;
        InitDecks();

        if (humanCustomDecks > 0) {
            for (int i = 0; i < humanCustomDecks; i++) {
                humanDeckList.GetChild(humanDeckPage).GetChild(humanDeckCount).gameObject.SetActive(true);
                humanDeckList.GetChild(humanDeckPage).GetChild(humanDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.humanDecks[i]);
                humanDeckCount++;
                if (humanDeckCount == 3) {
                    humanDeckPage++;
                    humanDeckCount = 0;
                }
                if (humanDeckPage < 3)
                    humanDeckList.GetChild(humanDeckPage).GetChild(humanDeckCount).gameObject.SetActive(true);
            }
            if (humanDeckPage == 3)
                humanDeckPage--;
        }
        if (orcCustomDecks > 0) {
            for (int i = 0; i < orcCustomDecks; i++) {
                orcDeckList.GetChild(orcDeckPage).GetChild(orcDeckCount).gameObject.SetActive(true);
                orcDeckList.GetChild(orcDeckPage).GetChild(orcDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.orcDecks[i]);
                orcDeckCount++;
                if (orcDeckCount == 3) {
                    orcDeckPage++;
                    orcDeckCount = 0;
                }
                if (orcDeckPage < 3)
                    orcDeckList.GetChild(orcDeckPage).GetChild(orcDeckCount).gameObject.SetActive(true);
            }
            if (orcDeckPage == 3)
                orcDeckPage--;
        }
        humanDeckNum.text = "1/" + (humanDeckPage + 1).ToString();
        orcDeckNum.text = "1/" + (orcDeckPage + 1).ToString();
        pageButtons.Find("Human/Left").gameObject.SetActive(false);
        if (humanDeckPage > 0)
            pageButtons.Find("Human/Right").gameObject.SetActive(true);
        else
            pageButtons.Find("Human/Right").gameObject.SetActive(false);
        pageButtons.Find("Orc/Left").gameObject.SetActive(false);
        if (orcDeckPage > 0)
            pageButtons.Find("Orc/Right").gameObject.SetActive(true);
        else
            pageButtons.Find("Orc/Right").gameObject.SetActive(false);
    }

    private void InitDecks() {
        for (int i = 0; i < 3; i++) {
            humanDeckList.GetChild(i).gameObject.SetActive(false);
            orcDeckList.GetChild(i).gameObject.SetActive(false);
            for(int j = 0; j < 3; j++) {
                humanDeckList.GetChild(i).GetChild(j).gameObject.SetActive(false);
                humanDeckList.GetChild(i).GetChild(j).Find("DeckInfo").gameObject.SetActive(false);
                humanDeckList.GetChild(i).GetChild(j).Find("DeckInfo/EditButtons").gameObject.SetActive(false);
                orcDeckList.GetChild(i).GetChild(j).gameObject.SetActive(false);
                orcDeckList.GetChild(i).GetChild(j).Find("DeckInfo").gameObject.SetActive(false);
                orcDeckList.GetChild(i).GetChild(j).Find("DeckInfo/EditButtons").gameObject.SetActive(false);
            }
        }
        humanDeckList.GetChild(0).gameObject.SetActive(true);
        orcDeckList.GetChild(0).gameObject.SetActive(true);
        humanDeckList.GetChild(0).GetChild(0).gameObject.SetActive(true);
        orcDeckList.GetChild(0).GetChild(0).gameObject.SetActive(true);
    }

    public void NextPageBtn(bool isHuman) {
        if (isHuman) {
            humanDeckList.GetChild(humanCurrentPage - 1).gameObject.SetActive(false);
            humanDeckList.GetChild(humanCurrentPage).gameObject.SetActive(true);
            humanCurrentPage++;
            if (humanCurrentPage == 3 || !humanDeckList.GetChild(humanCurrentPage).GetChild(0).gameObject.activeSelf)
                pageButtons.Find("Human/Right").gameObject.SetActive(false);
            pageButtons.Find("Human/Left").gameObject.SetActive(true);
            humanDeckNum.text = humanCurrentPage.ToString() + "/" + (humanDeckPage + 1).ToString();
        }
        else {
            orcDeckList.GetChild(orcCurrentPage - 1).gameObject.SetActive(false);
            orcDeckList.GetChild(orcCurrentPage).gameObject.SetActive(true);
            orcCurrentPage++;
            if (orcCurrentPage == 3 || !orcDeckList.GetChild(orcCurrentPage).GetChild(0).gameObject.activeSelf)
                pageButtons.Find("Orc/Right").gameObject.SetActive(false);
            pageButtons.Find("Orc/Left").gameObject.SetActive(true);
            orcDeckNum.text = orcCurrentPage.ToString() + "/" + (orcDeckPage + 1).ToString();
        }
    }

    public void PrevPageBtn(bool isHuman) {
        if (isHuman) {
            humanDeckList.GetChild(humanCurrentPage - 1).gameObject.SetActive(false);
            humanDeckList.GetChild(humanCurrentPage - 2).gameObject.SetActive(true);
            humanCurrentPage--;
            if (humanCurrentPage == 1 || !humanDeckList.GetChild(humanCurrentPage - 1).GetChild(0).gameObject.activeSelf)
                pageButtons.Find("Human/Left").gameObject.SetActive(false);
            pageButtons.Find("Human/Right").gameObject.SetActive(true);
            humanDeckNum.text = humanCurrentPage.ToString() + "/" + (humanDeckPage + 1).ToString();
        }
        else {
            orcDeckList.GetChild(orcCurrentPage - 1).gameObject.SetActive(false);
            orcDeckList.GetChild(orcCurrentPage - 2).gameObject.SetActive(true);
            orcCurrentPage--;
            if (orcCurrentPage == 1 || !orcDeckList.GetChild(orcCurrentPage-1).GetChild(0).gameObject.activeSelf)
                pageButtons.Find("Orc/Left").gameObject.SetActive(false);
            pageButtons.Find("Orc/Right").gameObject.SetActive(true);
            orcDeckNum.text = orcCurrentPage.ToString() + "/" + (orcDeckPage + 1).ToString();
        }
    }

    void OnDecksInfoLoaded() {
        SetPlayerDecks();
    }
}
