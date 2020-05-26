using dataModules;
using UnityEngine;

public class BattleReadyDeckHandler : DeckHandler {
    private Deck _deck;
    [SerializeField] private GameObject frontEffect, glow;
    
    public override void SetNewDeck(Deck deck) {
        _deck = deck;
        base.SetNewDeck(deck);
        
        Transform deckObject = transform.GetChild(0);
        GameObject frontEffect = Instantiate(this.frontEffect, deckObject);
        frontEffect.name = "FrontEffect";
        frontEffect.transform.SetAsLastSibling();
        GameObject glow = Instantiate(this.glow, deckObject);
        glow.name = "Glow";
        glow.transform.SetAsFirstSibling();
    }
    
    public override void OpenDeckButton() { }

    private void OnDisable() {
        GameObject frontEffect = transform.GetChild(0).Find("FrontEffect").gameObject;
        GameObject glow = transform.GetChild(0).Find("Glow").gameObject;
        if (frontEffect) { frontEffect.SetActive(false); }
        if (glow) { glow.SetActive(false); }
    }
}