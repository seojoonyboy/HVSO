using dataModules;
using UnityEngine;

public class BattleReadyDeckHandler : DeckHandler {
    private Deck _deck;
    [SerializeField] private GameObject frontEffect, glow;
    
    public override void SetNewDeck(Deck deck) {
        _deck = deck;
        base.SetNewDeck(deck);
    }
    
    public override void OpenDeckButton() { }

    private void OnDisable() { }
}