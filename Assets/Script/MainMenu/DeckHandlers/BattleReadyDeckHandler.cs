using System;
using System.Collections;
using dataModules;
using UnityEngine;

public class BattleReadyDeckHandler : DeckHandler {
    private Deck _deck;
    [SerializeField] private GameObject frontEffect, glow;
    
    public override void SetNewDeck(Deck deck) {
        _deck = deck;
        base.SetNewDeck(deck);
    }

    private void OnEnable() {
        if (AccountManager.Instance.myHeroInventories.ContainsKey(_deck.heroId)) {
            int heroTier = AccountManager.Instance.myHeroInventories[_deck.heroId].tier;
            Transform heroTierTranform = transform.GetChild(0).Find("HeroInfo/HeroTier");
            for (int i = 0; i < 3; i++) {
                heroTierTranform.GetChild(i).GetChild(0).gameObject.SetActive(i < heroTier);
            }
        }
    }

    public override void OpenDeckButton() { }

    private void OnDisable() { }
}