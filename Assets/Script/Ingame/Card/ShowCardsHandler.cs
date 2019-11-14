using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        //드래그 비활성화
        //버튼 컴포넌트 추가
        foreach(GameObject card in cards) {
            heroCards.Add(card);
        }

        if (heroCards.Count != 2) return;

        string[] poses = new string[] { "Left", "Right" };
        for(int i=0; i<heroCards.Count; i++) {
            MagicDragHandler handler = heroCards[i].GetComponent<MagicDragHandler>();
            string camp = handler.cardData.camp;
            heroCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject tmp = heroCards[i];
            heroCards[i].GetComponent<Button>().onClick.AddListener(() => OnClick(tmp));
            heroCards[i].GetComponent<MagicDragHandler>().enabled = false;

            transform
                .Find(poses[i])
                .Find("Desc")
                .gameObject
                .SetActive(true);

            transform
                .Find(poses[i])
                .Find("Desc/Text")
                .GetComponent<TextMeshProUGUI>()
                .text = AccountManager.Instance.GetComponent<Translator>().DialogSetRichText(desc[i]);
        }

        ToggleDragGuideUI(false);
        ToggleClickGuideUI(true);

        ToggleBg(true);
        ToggleCancelBtn(false);

        hideShowBtn.SetActive(true);
    }

    /// <summary>
    /// 영웅 카드 하나 클릭함
    /// </summary>
    /// <param name="selectedObject">선택된 카드 gameobject</param>
    public void OnClick(GameObject selectedObject) {
        StartCoroutine(ProceedSelect(selectedObject));
    }

    IEnumerator ProceedSelect(GameObject selectedObject) {
        RectTransform rect = selectedObject.GetComponent<RectTransform>();
        iTween.MoveTo(selectedObject, transform.Find("SelectedPos").position, 0.3f);
        OffOppositeCard(selectedObject);
        ToggleDescUI(false);
        ToggleCancelBtn(true);
        ToggleClickGuideUI(false);

        yield return new WaitForSeconds(0.4f);

        ToggleDragGuideUI(true);
        

        ToggleBg(false);
        
        selectedObject.GetComponent<MagicDragHandler>().enabled = true;
        selectedObject.GetComponent<Button>().enabled = false;
    }

    //카드를 사용함
    public void FinishPlay(GameObject activatedCard, bool isToHand = false) {
        GameObject oppositeCard = GetOppositeCard(activatedCard);
        if(oppositeCard != null) {
            oppositeCard
            .GetComponent<CardHandler>()
            .heroCardActivate = false;

            RemoveCard(oppositeCard);
        }

        //스킬을 사용한 경우
        if (!isToHand) {
            RemoveCard(activatedCard);
        }
        //핸드로 가져온 경우
        else {
            activatedCard.SetActive(true);
        }

        ToggleDragGuideUI(false);
        ToggleClickGuideUI(false);
        ToggleBg(false);
        ToggleDescUI(false);
        ToggleCancelBtn(false);

        heroCards.Clear();
    }

    //드래그를 시작함
    public void Selecting(GameObject activatedCard) {
        OffOppositeCard(activatedCard);

        ToggleBg(false);
        ToggleDescUI(false);
        ToggleDragGuideUI(false);
    }

    public void CancelSelecting() {
        StartCoroutine(ProceedCancel());
    }

    IEnumerator ProceedCancel() {
        GameObject selectedCard = GetSelectedCard();
        if(selectedCard.transform.parent.name == "Left") {
            iTween.MoveTo(selectedCard, transform.Find("Left").position, 0.3f);
        }
        else {
            iTween.MoveTo(selectedCard, transform.Find("Right").position, 0.3f);
        }
        ToggleDragGuideUI(false);
        ToggleClickGuideUI(true);

        ToggleBg(true);
        ToggleDescUI(true);
        ToggleCancelBtn(false);

        yield return new WaitForSeconds(0.4f);
        
        ToggleAllCards(true);

        selectedCard.GetComponent<MagicDragHandler>().enabled = false;
        selectedCard.GetComponent<Button>().enabled = true;
    }

    public GameObject GetSelectedCard() {
        return heroCards.Find(x => x.activeSelf);
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
            card.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
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
    public void HideUI() {
        ToggleAllCards(false);
        transform.Find("DragGuide").gameObject.SetActive(false);
        BgImg.SetActive(false);
    }

    public void ToggleBg(bool toggle = true) {
        BgImg.SetActive(toggle);
    }

    public void ToggleDragGuideUI(bool toggle = true) {
        transform.Find("DragGuide").gameObject.SetActive(toggle);
    }

    public void ToggleClickGuideUI(bool toggle = true) {
        transform.Find("ClickGuide").gameObject.SetActive(toggle);
    }

    public void ToggleDescUI(bool toggle = true) {
        transform.Find("Left/Desc").gameObject.SetActive(toggle);
        transform.Find("Right/Desc").gameObject.SetActive(toggle);
    }

    public void ToggleCancelBtn(bool toggle = true) {
        hideShowBtn.SetActive(toggle);
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
