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
        int humanBasicDecks = AccountManager.Instance.humanDecks.basicDecks.Count;
        int orcBasicDecks = AccountManager.Instance.orcDecks.basicDecks.Count;
        if (humanBasicDecks > 0) {
            for (int i = 0; i < humanBasicDecks; i++) {
                humanDeckList.GetChild(humanDeckCount).gameObject.SetActive(true);
                humanDeckList.GetChild(humanDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.humanDecks.basicDecks[i]);
                humanDeckCount++;
                humanDeckList.GetChild(humanDeckCount).gameObject.SetActive(true);
            }
        }
        if (orcBasicDecks > 0) {
            for (int i = 0; i < orcBasicDecks; i++) {
                orcDeckList.GetChild(orcDeckCount).gameObject.SetActive(true);
                orcDeckList.GetChild(orcDeckCount).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.orcDecks.basicDecks[i]);
                orcDeckCount++;
                orcDeckList.GetChild(orcDeckCount).gameObject.SetActive(true);
            }
        }
        humanDeckNum.text = humanDeckCount.ToString() + "/4";
        orcDeckNum.text = humanDeckCount.ToString() + "/4";

    }

    void OnDecksInfoLoaded() {
        SetPlayerDecks();
    }
}
