using System;
using System.Collections;
using System.Collections.Generic;
using dataModules;
using UnityEngine;
using UnityEngine.UI;

public class StoryDeckHandler : DeckHandler {
    [SerializeField] ScenarioManager _scenarioManager;
    private Deck _deck;
    public bool isTutorial = false;
    [SerializeField] private GameObject frontEffect, glow;
    
    public override void SetNewDeck(Deck deck) {
        bool isHuman = deck.camp == "human";
        if(string.IsNullOrEmpty(deck.bannerImage)) deck.bannerImage = isHuman ? "deck1001" : "deck1003";
        if (isTutorial) {
            deck.heroId = isHuman ? "h10001" : "h10002";
            deck.name = isHuman ? "Militia" : "Drifting Nomads";
            deck.deckValidate = true;
            deck.totalCardCount = 40;
        }
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

    public override void OpenDeckButton() {
        _scenarioManager.OnDeckSelected(gameObject, _deck, isTutorial);
    }

    private void OnDisable() {
        GameObject frontEffect = transform.GetChild(0).Find("FrontEffect").gameObject;
        GameObject glow = transform.GetChild(0).Find("Glow").gameObject;
        if (frontEffect) { frontEffect.SetActive(false); }
        if (glow) { glow.SetActive(false); }
    }
}