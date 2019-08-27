using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowCardsHandler : MonoBehaviour {
    [SerializeField] List<GameObject> heroCards;
    [SerializeField] Transform cardStorage;
    [SerializeField] GameObject hideShowBtn;
    [SerializeField] CardHandManager cardHandManager;

    // Start is called before the first frame update
    void Start() {
        heroCards = new List<GameObject>();
    }

    public void AddCards(GameObject[] cards, string[] desc) {
        for(int i=0; i<cards.Length; i++) {
            heroCards.Add(cards[i]);
            transform
                .GetChild(i)
                .Find("Desc")
                .gameObject.SetActive(true);
            transform
                .GetChild(i)
                .Find("Desc/Text")
                .GetComponent<TextMeshProUGUI>()
                .text = desc[i];
        }

        ShowUI();
        hideShowBtn.SetActive(true);
    }

    public void FinishPlay(GameObject activatedCard, bool isToHand = false) {
        GameObject oppositeCard = GetOppositeCard(activatedCard);
        if(oppositeCard != null) {
            oppositeCard
            .GetComponent<CardHandler>()
            .heroCardActivate = false;

            RemoveCard(oppositeCard);
        }
        
        if (!isToHand) { RemoveCard(activatedCard); }

        HideUI();
        hideShowBtn.SetActive(false);
    }

    public void Selecting(GameObject activatedCard) {
        GameObject oppositeCard = GetOppositeCard(activatedCard);
        OffOppositeCard(activatedCard);
        HideDesc();
    }

    public GameObject GetOppositeCard(GameObject self) {
        return heroCards.Find(x => x != self);
    }

    public void OffOppositeCard(GameObject self) {
        GameObject target = GetOppositeCard(self);
        target.SetActive(false);
    }

    public void ToggleAllCards(bool isOn = true) {
        foreach(GameObject card in heroCards) {
            if (card.activeSelf != isOn) {
                card.SetActive(isOn);
            }
            card.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);
        }
    }

    public void RemoveCard(GameObject self) {
        self.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (self.GetComponent<MagicDragHandler>().skillHandler.socketDone) {
            cardHandManager.DestroyCard(self);
            heroCards.Remove(self);
        }
    }

    //감추기 버튼 기능
    public void ShowUI() {
        ToggleAllCards();
        transform.Find("HeroCardGuide").gameObject.SetActive(true);
        ShowDesc();
    }

    //감추기 버튼 기능
    public void HideUI() {
        ToggleAllCards(false);
        transform.Find("HeroCardGuide").gameObject.SetActive(false);
        HideDesc();
    }

    public void HideDesc() {
        transform.Find("Left/Desc").gameObject.SetActive(false);
        transform.Find("Right/Desc").gameObject.SetActive(false);
    }

    public void ShowDesc() {
        transform.Find("Left/Desc").gameObject.SetActive(true);
        transform.Find("Right/Desc").gameObject.SetActive(true);
    }
}
