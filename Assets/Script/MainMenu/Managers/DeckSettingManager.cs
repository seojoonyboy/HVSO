using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] Canvas humanTemplateCanvas;
    [SerializeField] Canvas orcTemplateCanvas;
    public void ClickNewDeck(DeckHandler deck) {
        //deck.DECKID
        if (deck.gameObject.name == "HumanEditDeck")
            humanTemplateCanvas.gameObject.SetActive(true);
        else
            orcTemplateCanvas.gameObject.SetActive(true);
    }
}
