using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] Canvas humanTemplateCanvas;
    [SerializeField] Canvas orcTemplateCanvas;
    [SerializeField] Canvas cardDictionaryCanvas;
    [SerializeField] Transform humanDeckList;
    [SerializeField] Transform orcDeckList;
    [SerializeField] TMPro.TextMeshProUGUI humanDeckNum;
    [SerializeField] TMPro.TextMeshProUGUI orcDeckNum;
    public MenuSceneController menuSceneController;

    MyDecksLoader decksLoader;

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
    }

    public void OpenCardDictionary() {
        cardDictionaryCanvas.gameObject.SetActive(true);
        cardDictionaryCanvas.GetComponent<CardDictionaryManager>().SetToOrcCards();
    }

    public void SetPlayerDecks() {
        int humanDeckCount = 0;
        int orcDeckCount = 0;
        int humanCustomDecks = AccountManager.Instance.humanDecks.Count;
        int orcCustomDecks = AccountManager.Instance.orcDecks.Count;
        humanDeckList.GetChild(humanDeckCount).gameObject.SetActive(true);
        orcDeckList.GetChild(orcDeckCount).gameObject.SetActive(true);
        if (humanCustomDecks > 0) {
            for (int i = 0; i < humanCustomDecks; i++) {
                humanDeckList.GetChild(humanDeckCount).gameObject.SetActive(true);
                humanDeckList.GetChild(humanDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.humanDecks[i]);
                humanDeckCount++;
                if(humanDeckCount < 3)
                    humanDeckList.GetChild(humanDeckCount).gameObject.SetActive(true);
            }
        }
        if (orcCustomDecks > 0) {
            for (int i = 0; i < orcCustomDecks; i++) {
                orcDeckList.GetChild(orcDeckCount).gameObject.SetActive(true);
                orcDeckList.GetChild(orcDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.orcDecks[i]);
                orcDeckCount++;
                if (orcDeckCount < 3)
                    orcDeckList.GetChild(orcDeckCount).gameObject.SetActive(true);
            }
        }
        humanDeckNum.text = humanDeckCount.ToString() + "/3";
        orcDeckNum.text = orcDeckCount.ToString() + "/3";

    }

    void OnDecksInfoLoaded() {
        SetPlayerDecks();
    }
}
