using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowCardsHandler : MonoBehaviour {
    [SerializeField] List<GameObject> heroCards;
    [SerializeField] Transform cardStorage;
    [SerializeField] public GameObject hideShowBtn;
    [SerializeField] CardHandManager cardHandManager;
    [SerializeField] GameObject BgImg;
    public Transform timerPos;

    // Start is called before the first frame update
    void Start() {
        heroCards = new List<GameObject>();
    }

#if UNITY_EDITOR
    void Update() {
        if (heroCards != null && heroCards.Count ==2 && Input.GetKeyDown(KeyCode.Q)) {
            TimeoutShowCards();
        }    
    }
#endif

    public void AddCards(GameObject[] cards, string[] desc) {
        //TODO : i값 대조 말고 다른 방법으로....
        for(int i=0; i<cards.Length; i++) {
            if(i == 0) {
                transform
                    .Find("Left/Desc")
                    .gameObject.SetActive(true);
                transform
                    .Find("Left/Desc/Text")
                    .GetComponent<TextMeshProUGUI>()
                    .text = desc[i];
            }
            else if(i == 1) {
                transform
                    .Find("Right/Desc")
                    .gameObject.SetActive(true);
                transform
                    .Find("Right/Desc/Text")
                    .GetComponent<TextMeshProUGUI>()
                    .text = desc[i];
            }
            cards[i]
                .transform
                .Find("GlowEffect")
                .gameObject.SetActive(true);
            heroCards.Add(cards[i]);
        }
        ShowUI();
        hideShowBtn.SetActive(true);

        if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial) {
            hideShowBtn.SetActive(false);
            cards[1]
                .transform
                .Find("GlowEffect")
                .gameObject.SetActive(false);
        }
    }

    public void FinishPlay(GameObject activatedCard, bool isToHand = false) {
        GameObject oppositeCard = GetOppositeCard(activatedCard);
        if(oppositeCard != null) {
            oppositeCard
            .GetComponent<CardHandler>()
            .heroCardActivate = false;

            RemoveCard(oppositeCard);
        }
        
        if (!isToHand) {
            RemoveCard(activatedCard);

            transform.Find("HeroCardGuide").gameObject.SetActive(false);
            BgImg.SetActive(false);
            HideDesc();
        }
        else {
            HideUI();
            activatedCard.SetActive(true);
        }

        heroCards.Clear();
        hideShowBtn.SetActive(false);
    }

    public void Selecting(GameObject activatedCard) {
        GameObject oppositeCard = GetOppositeCard(activatedCard);
        OffOppositeCard(activatedCard);
        BgImg.SetActive(false);
        HideDesc();
    }

    public void CancelSelecting() {
        ToggleAllCards();
        BgImg.SetActive(true);
        ShowDesc();
    }

    public GameObject GetOppositeCard(GameObject self) {
        return heroCards.Find(x => x != self);
    }

    public void OffOppositeCard(GameObject self) {
        GameObject target = GetOppositeCard(self);
        target.SetActive(false);
    }

    public void ToggleAllCards(bool isOn = true) {
        transform.SetAsLastSibling();
        foreach (GameObject card in heroCards) {
            if (card.activeSelf != isOn) {
                card.SetActive(isOn);
            }
            card.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
            card.transform.localPosition = new Vector3(0.0f, -260f, 0.0f);
        }
    }

    public void RemoveCard(GameObject self) {
        self.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (self.GetComponent<MagicDragHandler>().skillHandler.socketDone) {
            cardHandManager.DestroyCard(self);
        }
        else {
            self.transform.localPosition = new Vector3(4000f, 0);
        }
    }

    //감추기 버튼 기능
    public void ShowUI() {
        ToggleAllCards();
        transform.Find("HeroCardGuide").gameObject.SetActive(true);
        BgImg.SetActive(true);
        ShowDesc();
    }

    //감추기 버튼 기능
    public void HideUI() {
        ToggleAllCards(false);
        transform.Find("HeroCardGuide").gameObject.SetActive(false);
        BgImg.SetActive(false);
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

    //시간초과에 의한 강제 랜덤 선택 및 핸드 추가 처리
    public void TimeoutShowCards() {
        if (heroCards.Count != 2) return;

        int rndIndex = Random.Range(0, 1);
        try {
            var selectedCard = heroCards[rndIndex];
            selectedCard.GetComponent<MagicDragHandler>().ForceToHandHeroCards();
            selectedCard.GetComponent<MagicDragHandler>().OnEndDrag(null);
        }
        catch(System.Exception ex) {
            Logger.LogError(ex.ToString());
        }

        CardInfoOnDrag.instance.OffCardDragInfo();
        PlayMangement.instance.player.ConsumeShieldStack();
        ToggleAllCards();
    }
}
