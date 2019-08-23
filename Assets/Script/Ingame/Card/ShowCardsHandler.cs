using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCardsHandler : MonoBehaviour {
    [SerializeField] List<GameObject> heroCards;
    [SerializeField] Transform cardStorage;
    [SerializeField] GameObject hideShowBtn;

    // Start is called before the first frame update
    void Start() {
        heroCards = new List<GameObject>();
    }

    public void AddCard(GameObject self) {
        heroCards.Add(self);
        ShowUI();
        hideShowBtn.SetActive(true);
    }

    public void SetDesc(bool isLeft, string desc) {
        if (isLeft) {
            transform.Find("Left/Desc").gameObject.SetActive(true);
            transform.Find("Left/Desc/Text").GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        }
        else {
            transform.Find("Right/Desc").gameObject.SetActive(true);
            transform.Find("Right/Desc/Text").GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        }
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
        target.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);
    }

    public void ToggleAllCards(bool isOn = true) {
        foreach(GameObject card in heroCards) {
            if (card.activeSelf != isOn) {
                card.SetActive(isOn);
            }
            card.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);
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
        target.transform.localPosition = new Vector3(0, 0, 0);
        target.transform.localScale = new Vector3(1, 1, 1);
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
        self.transform.localPosition = new Vector3(0, 0, 0);
        self.transform.localScale = new Vector3(1, 1, 1);
    }

    //실드 이벤트 종료 처리
    public void ClearList() {
        foreach(GameObject card in heroCards) {
            card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            card.transform.localScale = new Vector3(1, 1, 1);
            card.SetActive(false);
        }
        hideShowBtn.SetActive(false);
        transform.Find("Left/Desc").gameObject.SetActive(false);
        transform.Find("Right/Desc").gameObject.SetActive(false);

        heroCards.Clear();
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
