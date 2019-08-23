using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCardsHandler : MonoBehaviour {
    [SerializeField] List<GameObject> heroCards;
    [SerializeField] Transform cardStorage;
    // Start is called before the first frame update
    void Start() {
        heroCards = new List<GameObject>();
    }

    public void AddCard(GameObject self) {
        heroCards.Add(self);
    }

    public GameObject GetOppositeCard(GameObject self) {
        return heroCards.Find(x => x != self);
    }

    public void OffOppositeCard(GameObject self) {
        GameObject target = GetOppositeCard(self);
        target.SetActive(false);
    }

    public void OnOppositeCard(GameObject self) {
        GameObject target = GetOppositeCard(self);
        target.SetActive(true);
    }

    public void ToggleAllCards(bool isOn = true) {
        foreach(GameObject card in heroCards) {
            if (card.activeSelf != isOn) card.SetActive(isOn);
        }
    }

    public void RemoveOppositeCard(GameObject self) {
        GameObject target = heroCards.Find(x => x != self);
        if(target.GetComponent<MagicDragHandler>().cardData.camp == "human") {
            target.transform.SetParent(cardStorage.Find("HumanHeroCards"));
        }
        else {
            target.transform.SetParent(cardStorage.Find("OrcHeroCards"));
        }
        target.transform.localPosition = Vector3.zero;
        //transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public void RemoveCard(GameObject self) {
        heroCards.Remove(self);
        if (self.GetComponent<MagicDragHandler>().cardData.camp == "human") {
            self.transform.SetParent(cardStorage.Find("HumanHeroCards"));
        }
        else {
            self.transform.SetParent(cardStorage.Find("OrcHeroCards"));
        }
        self.transform.localPosition = Vector3.zero;
    }

    public void ClearList() {
        foreach(GameObject card in heroCards) {
            card.SetActive(false);
        }
        heroCards.Clear();
    }
}
