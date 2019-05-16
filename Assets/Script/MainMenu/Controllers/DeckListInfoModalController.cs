using dataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckListInfoModalController : MonoBehaviour {
    [SerializeField] Transform parent;
    [SerializeField] DeckListController deckListController;

    AccountManager accountManager;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void OnEnable() {
        accountManager = AccountManager.Instance;

        CreateUnlockedList();
        CreateLockedList();
    }

    void CreateUnlockedList() {
        if (deckListController.selectedDeck == null) return;
        try {
            int id = deckListController.selectedDeck.GetComponent<IntergerIndex>().Id;
            var deck = accountManager.myDecks[id];
        }
        catch (Exception ex) {
            if(ex is NullReferenceException || ex is ArgumentException) {

            }
        }
        
    }

    void CreateLockedList() {

    }
}
