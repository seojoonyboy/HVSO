using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] Canvas humanTemplateCanvas;
    [SerializeField] Canvas orcTemplateCanvas;
    [SerializeField] Transform humanDeckList;
    [SerializeField] Transform orcDeckList;
    MyDecksLoader decksLoader;

    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnLoadFinished.AddListener(() => { OnDecksInfoLoaded(); });
    }

    public void ClickNewDeck(DeckHandler deck) {
        //deck.DECKID
        if (deck.gameObject.name == "HumanEditDeck")
            humanTemplateCanvas.gameObject.SetActive(true);
        else
            orcTemplateCanvas.gameObject.SetActive(true);
    }

    public void SetPlayerDecks() {
        if (AccountManager.Instance.humanDecks.basicDecks.Count > 0) {
            humanDeckList.GetChild(0).gameObject.SetActive(true);
            humanDeckList.GetChild(0).GetComponent<DeckHandler>().SetDeck(AccountManager.Instance.humanDecks.basicDecks[0]);
            humanDeckList.GetChild(1).gameObject.SetActive(true);
        }
    }

    void OnDecksInfoLoaded() {
        SetPlayerDecks();
    }
}
