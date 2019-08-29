using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHandManager : MonoBehaviour {
    public int cardNum = 0;
    TMPro.TextMeshProUGUI handCardNum;
    [SerializeField] Transform cardStorage;
    [SerializeField] Transform showPos;
    [SerializeField] protected Transform firstDrawParent;
    [SerializeField] public Transform cardSpawnPos;
    public bool isMultiple = false;
    public bool firstDraw = true;
    private bool cardDestroyed = false;
    protected List<GameObject> cardList;
    protected List<GameObject> firstDrawList;
    protected CardListManager clm;

    // Start is called before the first frame update
    void Start() {
        firstDraw = true;
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        clm = PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        //handCardNum = transform.parent.Find("PlayerCardNum/Value").GetComponent<TMPro.TextMeshProUGUI>();
    }

    //멀리건 실행 코루틴(교체 가능한 카드 4장 드로우)
    public IEnumerator FirstDraw() {
        bool race = PlayMangement.instance.player.isHuman;
        SocketFormat.Card socketCard = PlayMangement.instance.socketHandler.gameState.players.myPlayer(race).FirstCards[firstDrawList.Count];
        GameObject card;
        if (socketCard.type == "unit")
            card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
        else {
            card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        }

        card.transform.SetParent(firstDrawParent);
        card.SetActive(true);
        card.GetComponent<CardHandler>().DrawCard(socketCard.id, socketCard.itemId, true);

        if (socketCard.type == "magic") {
            card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = socketCard.name;
            AddMagicAttribute(ref card);
        }

        iTween.MoveTo(card, firstDrawParent.GetChild(firstDrawList.Count).position, 0.5f);
        iTween.RotateTo(card, new Vector3(0, 0, 0), 0.5f);
        firstDrawList.Add(card);
        //card.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        yield return new WaitForSeconds(0.5f);
        card.transform.Find("ChangeButton").gameObject.SetActive(true);
        if (firstDrawList.Count == 4) {
            yield return new WaitForSeconds(0.5f);
            firstDrawParent.parent.Find("FinishButton").gameObject.SetActive(true);
        }
    }

    //멀리건 종료 버튼 클릭 함수
    public void FirstDrawCardChange() {
        foreach (GameObject cards in firstDrawList) {
            cards.transform.Find("ChangeButton").gameObject.SetActive(false);
        }
        AddInfoToList(null, true);
        StartCoroutine(DrawChangedCards());
        firstDrawParent.GetChild(4).gameObject.SetActive(false);
        Transform finBtn = firstDrawParent.parent.Find("FinishButton");
        finBtn.GetComponent<Button>().enabled = false;
        finBtn.GetComponent<Image>().enabled = false;
        finBtn.GetChild(0).gameObject.SetActive(false);
        finBtn.gameObject.SetActive(false);
        PlayMangement.instance.resultManager.SetResultWindow("lose", PlayMangement.instance.player.isHuman);
    }

    //카드 저장고 반환 함수
    public Transform GetcardStorage() {
        return cardStorage;
    }


    /// <summary>
    /// CardListManager에서 카드 정보를 추가하는 함수 호출
    /// </summary>
    /// <param name="card"></param> 정보를 추가할 카드 오브젝트
    /// <param name="isMulligan"></param> 멀리건일시 true
    /// <param name="isHero"></param> 영웅카드일시 true
    public void AddInfoToList(GameObject card, bool isMulligan = false, bool isHero = false) {
        if (isHero) {
            clm.AddHeroCardInfo(card);
            return;
        }
        if (isMulligan)
            clm.SendMulliganInfo();
        else {
            CardHandler handler = card.GetComponent<CardHandler>();
            clm.AddCardInfo(handler.cardData);
        }
    }

    /// <summary>
    /// 핸드에 카드를 추가하는 함수
    /// </summary>
    /// <param name="cardobj"></param> 멀리건일 경우 미리 뽑은 카드들을 사용
    /// <param name="cardData"></param>
    public void AddCard(GameObject cardobj = null, SocketFormat.Card cardData = null) {
        if (cardNum + 1 == 11) return;
        GameObject card;
        if (cardobj == null) {
            if (cardData.type == "unit")
                card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
            else
                card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
            if (cardData.isHeroCard) {
                if (PlayMangement.instance.player.isHuman)
                    card = cardStorage.Find("HumanHeroCards").GetChild(0).gameObject;
                else
                    card = cardStorage.Find("OrcHeroCards").GetChild(0).gameObject;
            }
            string id;
            int itemId = -1;
            if (cardData == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData.id;
                itemId = cardData.itemId;
            }
            card.GetComponent<CardHandler>().DrawCard(id, itemId);

            if (cardData.type == "magic") {
                card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
                AddMagicAttribute(ref card);
            }
        }
        else
            card = cardobj;
        if (firstDraw) {
            card.GetComponent<CardHandler>().DisableCard();
        }
        else {
            AddInfoToList(card);
            card.transform.Find("ChangeButton").gameObject.SetActive(false);
        }
        Transform cardTransform = card.transform;
        Transform cardPos = transform.GetChild(cardNum);
        cardPos.gameObject.SetActive(true);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        cardTransform.gameObject.SetActive(true);
        cardNum++;
        cardList.Add(card);
        StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos));
    }

    /// <summary>
    /// 2장 이상의 카드 핸드에 추가
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public IEnumerator AddMultipleCard(SocketFormat.Card[] cardData) {
        yield return new WaitWhile(() => cardDestroyed == false);
        isMultiple = true;
        for (int i = cardNum; i < cardData.Length; i++) {
            if (cardNum + 1 == 11) break;
            PlayMangement.dragable = false;
            GameObject card;
            if (cardData[i].type == "unit")
                card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
            else
                card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
            string id;
            int itemId = -1;
            if (cardData[i] == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData[i].id;
                itemId = cardData[i].itemId;
            }
            card.GetComponent<CardHandler>().DrawCard(id, itemId);
            card.transform.Find("ChangeButton").gameObject.SetActive(false);
            if (cardData[i].type == "magic") {
                card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData[i].name;
                AddMagicAttribute(ref card);
            }
            AddInfoToList(card);
            Transform cardTransform = card.transform;
            Transform cardPos = transform.GetChild(cardNum);
            cardPos.gameObject.SetActive(true);
            cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
            cardTransform.gameObject.SetActive(true);
            cardNum++;
            cardList.Add(card);
            if (cardNum == cardData.Length)
                StartCoroutine(SendMultipleCardToHand(cardTransform.gameObject, cardPos, true));
            else
                StartCoroutine(SendMultipleCardToHand(cardTransform.gameObject, cardPos));
            yield return new WaitForSeconds(0.5f);
            if (i == cardData.Length - 1 || cardNum == 10) {
                isMultiple = false;
                cardDestroyed = false;
            }
        }
    }

    /// <summary>
    /// 쉴드 발동시 영웅카드 드로우
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public IEnumerator DrawHeroCard(SocketFormat.Card[] cards = null) {
        GameObject leftCard, rightCard;

        if (PlayMangement.instance.player.isHuman) {
            leftCard = cardStorage.Find("HumanHeroCards").GetChild(0).gameObject;
            rightCard = cardStorage.Find("HumanHeroCards").GetChild(1).gameObject;
        }
        else {
            leftCard = cardStorage.Find("OrcHeroCards").GetChild(0).gameObject;
            rightCard = cardStorage.Find("OrcHeroCards").GetChild(1).gameObject;
        }
        leftCard.transform.SetParent(showPos.Find("Left"));
        rightCard.transform.SetParent(showPos.Find("Right"));

        showPos.Find("HeroCardGuide").gameObject.SetActive(true);

        leftCard.SetActive(true);
        rightCard.SetActive(true);

        ShowCardsHandler showCardsHandler = showPos.GetComponent<ShowCardsHandler>();
        showCardsHandler.AddCards(
            new GameObject[] { leftCard, rightCard },
            new string[] { cards[0].skills[0].desc, cards[1].skills[0].desc }
        );

        CardHandler handler = leftCard.GetComponent<CardHandler>();
        handler.DrawHeroCard(cards[0]);
        handler = rightCard.GetComponent<CardHandler>();
        handler.DrawHeroCard(cards[1]);

        iTween.MoveTo(leftCard, showPos.Find("Left").position, 0.4f);
        iTween.RotateTo(leftCard, iTween.Hash("z", 0, "islocal", true, "time", 0.4f));
        yield return new WaitForSeconds(0.5f);
        iTween.MoveTo(rightCard, showPos.Find("Right").position, 0.4f);
        iTween.RotateTo(rightCard, iTween.Hash("z", 0, "islocal", true, "time", 0.4f));

        yield return StartCoroutine(handler.ActiveHeroCard());
    }

    /// <summary>
    /// 쉴드 발동으로 뽑은 영웅카드 핸드에 추가
    /// </summary>
    /// <param name="cardobj"></param>
    public void AddHeroCard(GameObject cardobj) {
        if (cardNum + 1 == 11) return;
        PlayMangement.dragable = false;
        AddInfoToList(cardobj.transform.Find("CardInfoWindow").gameObject, false, true);
        Transform cardTransform = cardobj.transform;
        Transform cardPos = transform.GetChild(cardNum);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        PlayMangement.instance.socketHandler.KeepHeroCard(cardTransform.GetComponent<CardHandler>().itemID);
        cardTransform.localScale = new Vector3(1, 1, 1);
        cardNum++;
        cardList.Add(cardobj);
        cardPos.gameObject.SetActive(true);
        StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos, true));
    }

    /// <summary>
    /// 핸드에 카드 추가되는 애니메이션
    /// </summary>
    /// <param name="card"></param> 카드 오브젝트
    /// <param name="pos"></param>
    /// <param name="isHero"></param>
    /// <returns></returns>
    IEnumerator SendCardToHand(GameObject card, Transform pos, bool isHero = false) {
        PlayMangement.dragable = false;
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.DisableCard();
        //handCardNum.text = cardNum.ToString();
        if (!firstDraw) {
            if (!isHero) {
                iTween.MoveTo(card, showPos.position, 0.4f);
                iTween.RotateTo(card, pos.eulerAngles, 0.4f);
            }
            iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
            //iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
            yield return new WaitForSeconds(0.4f);
        }
        if (cardNum > 2 && !isMultiple && !firstDraw) {
            iTween.MoveTo(transform.gameObject, iTween.Hash("x", -250 * (cardNum - 4), "islocal", true, "time", 0.2f));
            yield return new WaitForSeconds(0.3f);
        }
        card.transform.SetParent(pos);
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.4f);
        yield return new WaitForSeconds(0.5f);
        if (PlayMangement.instance.currentTurn == "BATTLE") {
            handler.DisableCard();
        }
        else if (!PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "SECRET") {
            if (handler.cardData.type == "unit")
                handler.DisableCard();
            else
                handler.ActivateCard();
        }
        else if (PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "HUMAN")
            handler.ActivateCard();
        else
            handler.DisableCard();
        handler.FIRSTDRAW = false;
        if (!isMultiple && !firstDraw)
            yield return SortHandPosition();
        if (PlayMangement.instance.currentTurn != "BATTLE")
            PlayMangement.dragable = true;
    }

    /// <summary>
    /// 2장 이상의 카드가 핸드에 카드 추가되는 애니메이션
    /// </summary>
    /// <param name="card"></param> 카드 오브젝트
    /// <param name="pos"></param>
    /// <param name="isHero"></param>
    /// <returns></returns>
    IEnumerator SendMultipleCardToHand(GameObject card, Transform pos, bool isLast = false) {
        PlayMangement.dragable = false;
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.DisableCard();
        //handCardNum.text = cardNum.ToString();
        iTween.MoveTo(card, showPos.position, 0.4f);
        iTween.RotateTo(card, pos.eulerAngles, 0.4f);
        iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
        yield return new WaitForSeconds(0.4f);
        if (cardNum > 2 && isLast) {
            iTween.MoveTo(transform.gameObject, iTween.Hash("x", -250 * (cardNum - 4), "islocal", true, "time", 0.2f));
            yield return new WaitForSeconds(0.3f);
        }
        card.transform.SetParent(pos);
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.4f);
        yield return new WaitForSeconds(0.5f);
        if (PlayMangement.instance.currentTurn == "BATTLE") {
            handler.DisableCard();
        }
        else if (!PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "SECRET") {
            if (handler.cardData.type == "unit")
                handler.DisableCard();
            else
                handler.ActivateCard();
        }
        else if (PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "HUMAN")
            handler.ActivateCard();
        else
            handler.DisableCard();
        handler.FIRSTDRAW = false;
        if (isLast)
            yield return SortHandPosition();
        if (PlayMangement.instance.currentTurn != "BATTLE")
            PlayMangement.dragable = true;
    }

    /// <summary>
    /// 카드 제거 함수
    /// </summary>
    /// <param name="index"></param>
    public void DestroyCard(int index) {
        cardDestroyed = false;
        StartCoroutine(RemoveCardToStorage(index));
    }

    /// <summary>
    /// 적이 카드 사용시 카드 표기 및 폐기
    /// </summary>
    /// <param name="card"></param>
    public void DestroyCard(GameObject card) {
        PlayMangement.dragable = false;
        if (card.name == "MagicCard")
            card.transform.SetParent(cardStorage.Find("MagicCards"));
        else if (card.name == "UnitCard")
            card.transform.SetParent(cardStorage.Find("UnitCards"));
        else {
            if (PlayMangement.instance.player.isHuman)
                card.transform.SetParent(cardStorage.Find("HumanHeroCards"));
            else
                card.transform.SetParent(cardStorage.Find("OrcHeroCards"));
        }
        card.SetActive(false);
        card.transform.localPosition = Vector3.zero;
        card.transform.rotation = card.transform.parent.rotation;
        if (PlayMangement.instance.currentTurn != "BATTLE")
            PlayMangement.dragable = true;
    }

    /// <summary>
    /// 쉴드 발동 영웅카드 즉시 사용시 카드 제거 함수
    /// </summary>
    /// <param name="card"></param>
    public void DestroyUsedHeroCard(Transform card) {
        Transform infoWindow = card.Find("CardInfoWindow");
        infoWindow.SetParent(clm.transform.Find("InfoStandby"));
        infoWindow.localScale = new Vector3(1, 1, 1);
        infoWindow.rotation = infoWindow.parent.rotation;
        infoWindow.gameObject.SetActive(false);
        if (PlayMangement.instance.player.isHuman)
            card.transform.SetParent(cardStorage.Find("HumanHeroCards"));
        else
            card.transform.SetParent(cardStorage.Find("OrcHeroCards"));
        card.localPosition = Vector3.zero;
        card.localScale = new Vector3(1, 1, 1);
        card.rotation = card.transform.parent.rotation;
        card.gameObject.SetActive(false);
    }

    /// <summary>
    /// 사용한 카드 CardStorage로 반환
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private IEnumerator RemoveCardToStorage(int index) {
        PlayMangement.dragable = false;
        Transform removeCard = transform.GetChild(index).GetChild(0);
        transform.GetChild(index).gameObject.SetActive(false);
        transform.GetChild(index).SetAsLastSibling();
        if (removeCard.name == "UnitCard") {
            removeCard.SetParent(cardStorage.Find("UnitCards"));
            clm.AddFeildUnitInfo(index, PlayMangement.instance.unitNum - 1);
        }
        else {
            if (removeCard.name == "MagicCard")
                removeCard.SetParent(cardStorage.Find("MagicCards"));
            else {
                if (PlayMangement.instance.player.isHuman)
                    removeCard.SetParent(cardStorage.Find("HumanHeroCards"));
                else
                    removeCard.SetParent(cardStorage.Find("OrcHeroCards"));
            }
            clm.RemoveCardInfo(index);
        }
        removeCard.gameObject.SetActive(false);
        removeCard.eulerAngles = Vector3.zero;
        removeCard.localPosition = Vector3.zero;
        cardList.RemoveAt(index);
        cardNum--;
        //handCardNum.text = cardNum.ToString();
        for (int i = index; i < cardNum; i++) {
            transform.GetChild(i).GetChild(0).GetComponent<CardHandler>().CARDINDEX = i;
        }
        StartCoroutine(SortHandPosition());
        cardDestroyed = true;
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// 마법카드 사용시 카드 정보 화면 표시
    /// </summary>
    /// <param name="index"></param>
    /// <param name="card"></param>
    /// <returns></returns>
    public IEnumerator ShowUsedCard(int index = 100, GameObject card = null) {
        // if (index == 100) {
        //     clm.SetEnemyMagicCardInfo(card.GetComponent<CardHandler>().cardData);
        // }
        // else
        //     clm.OpenCardInfo(index, true);
        //yield return new WaitForSeconds(1.5f);
        // if (index != 100 && transform.GetChild(index).GetChild(0).GetComponent<MagicDragHandler>().skillHandler.TargetSelectExist())
        //     clm.HandCardInfo.GetChild(index).gameObject.SetActive(false);
        // if (index == 100) {
        //     Transform infoWindow = clm.StandbyInfo.GetChild(0);
        //     infoWindow.gameObject.SetActive(false);
        //     infoWindow.localScale = new Vector3(1, 1, 1);
        // }

        if(card == null) yield break;
        PlayMangement.instance.OnBlockPanel(null);
        Transform parent = card.transform.parent;
        card.transform.SetParent(GetComponentInParent<Canvas>().transform);
        card.transform.localScale = Vector3.one;
        if(index == 100)
            card.transform.position = new Vector3(0f, 50f, 0f);
        else
            card.transform.position = new Vector3(0f, -50f, 0f);
        iTween.RotateTo(card, Vector3.zero, 0.5f);
        iTween.MoveTo(card, iTween.Hash(
            "x", 0,
            "y", 0,
            "time", 0.5f,
            "easetype", iTween.EaseType.easeWeakOutBack));
        //card.transform.position = Vector3.zero;
        //card.transform.rotation = Quaternion.identity;
        CardHandler handler = card.GetComponent<CardHandler>();
        SetUsedCardInfo(ref card);
        yield return new WaitForSeconds(0.25f);

        if (card.GetComponent<UnitDragHandler>() != null && card.GetComponent<UnitDragHandler>().cardData.attributes.Length > 0 && card.GetComponent<UnitDragHandler>().cardData.attributes[0] == "ambush")
            CardInfoOnDrag.instance.SetCardDragInfo(null, new Vector3(0, 5, 0), null);
        else
            CardInfoOnDrag.instance.SetCardDragInfo(null, new Vector3(0, 5, 0), handler.cardData.skills.Length != 0 ? handler.cardData.skills[0].desc : null);
        
        yield return new WaitForSeconds(2.0f);
        
        card.transform.SetParent(parent);
        CardInfoOnDrag.instance.OffCardDragInfo();
        PlayMangement.instance.OffBlockPanel();
    }

    public void SetUsedCardInfo(ref GameObject card) {
        card.transform.localScale = Vector3.one;
        CardData cardData = card.GetComponent<CardHandler>().cardData;

        Image portrait = card.transform.Find("Portrait").GetComponent<Image>();
        TMPro.TextMeshProUGUI cost = card.transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>();
        
        Image skillIcon = null;
        bool isUnit = card.GetComponent<UnitDragHandler>() != null;
        if (isUnit) {
            TMPro.TextMeshProUGUI hp = card.transform.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI atk = card.transform.Find("attack/Text").GetComponent<TMPro.TextMeshProUGUI>();

            if (cardData.attributes.Length > 0 && cardData.attributes[0] == "ambush") {
                hp.text = "?";
                atk.text = "?";
                cost.text = "?";
                skillIcon = card.transform.Find("SkillIcon").GetComponent<Image>();
            }
            else {
                hp.text = cardData.hp.ToString();
                atk.text = cardData.attack.ToString();
                cost.text = cardData.cost.ToString();
                skillIcon = card.transform.Find("SkillIcon").GetComponent<Image>();
            }
        }

        if (isUnit == true && (cardData.attributes.Length > 0 && cardData.attributes[0] == "ambush")) {
            portrait.sprite = AccountManager.Instance.resource.cardPortraite["unknown"];
        }
        else
            portrait.sprite = AccountManager.Instance.resource.cardPortraite[cardData.cardId];

        if (cardData.attributes.Length == 0 && cardData.attackTypes.Length == 0 && isUnit) skillIcon.gameObject.SetActive(false);

        if (cardData.attributes.Length != 0 && isUnit)
            if (AccountManager.Instance.resource.skillIcons.ContainsKey(cardData.attributes[0])) {
                skillIcon.sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0]];
            }

        if (cardData.attackTypes.Length != 0 && isUnit)
            if (AccountManager.Instance.resource.skillIcons.ContainsKey(cardData.attackTypes[0])) {
                skillIcon.sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
            }

        if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0 && isUnit)
            skillIcon.sprite = AccountManager.Instance.resource.skillIcons["complex"];
        if(!cardData.hero_chk)
            card.transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
    }

    public IEnumerator SortHandPosition() {
        switch (cardNum) {
            case 1:
                iTween.MoveTo(gameObject, iTween.Hash("x", 430, "islocal", true, "time", 0.1f));
                break;
            case 2:
                iTween.MoveTo(gameObject, iTween.Hash("x", 305, "islocal", true, "time", 0.1f));
                break;
            case 3:
                iTween.MoveTo(gameObject, iTween.Hash("x", 180, "islocal", true, "time", 0.1f));
                break;
            case 4:
                iTween.MoveTo(gameObject, iTween.Hash("x", 60, "islocal", true, "time", 0.1f));
                break;
            case 5:
                if (transform.localPosition.x < 0)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -130, "islocal", true, "time", 0.1f));
                break;
            case 6:
                if (transform.localPosition.x < -380)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -380, "islocal", true, "time", 0.1f));
                break;
            case 7:
                if (transform.localPosition.x < -625)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -625, "islocal", true, "time", 0.1f));
                break;
            case 8:
                if (transform.localPosition.x < -870)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -870, "islocal", true, "time", 0.1f));
                break;
            case 9:
                if (transform.localPosition.x < -1120)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -1120, "islocal", true, "time", 0.1f));
                break;
            case 10:
                if (transform.localPosition.x < -1210)
                    iTween.MoveTo(gameObject, iTween.Hash("x", -1365, "islocal", true, "time", 0.1f));
                break;
        }
        if (cardNum > 4 && transform.localPosition.x > 0)
            iTween.MoveTo(gameObject, iTween.Hash("x", -0, "islocal", true, "time", 0.1f));
        yield return new WaitForSeconds(0.1f);
        //if (PlayMangement.instance.currentTurn != "BATTLE")
        //    PlayMangement.dragable = true;
    }

    /// <summary>
    /// 멀리건 종료시 카드 핸드에 추가 및 멀리건 영웅카드 드로우
    /// </summary>
    /// <returns></returns>
    IEnumerator DrawChangedCards() {
        firstDrawParent.parent.gameObject.GetComponent<Image>().enabled = false;
        PlayMangement.instance.socketHandler.MulliganEnd();
        int index = 0;
        PlayMangement.dragable = false;
        while (index < 4) {
            yield return new WaitForSeconds(0.2f);
            AddCard(firstDrawList[index]);
            index++;
        }
        yield return new WaitForSeconds(0.5f);
        firstDraw = false;
        #region 영웅카드 드로우 주석처리
        /* yield return PlayMangement.instance.socketHandler.WaitGetCard();

        //영웅카드 뽑기
        bool isHuman = PlayMangement.instance.player.isHuman;
        SocketFormat.Card cardData = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).newCard;
        AddCard(null, cardData);


        yield return new WaitForSeconds(3.0f);*/
        #endregion
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this);
        //CustomEvent.Trigger(GameObject.Find("GameManager"), "EndTurn");
        PlayMangement.instance.isMulligan = false;
        firstDrawParent.gameObject.SetActive(false);
        GameObject firstOrcTurnObj = firstDrawParent.parent.Find("First_OrcPlay").gameObject;
        firstOrcTurnObj.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        firstOrcTurnObj.SetActive(false);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public IEnumerator EditorSkipMulligan() {
        PlayMangement.instance.isMulligan = false;
        firstDrawParent.gameObject.SetActive(false);
        yield return new WaitForSeconds(3.0f);
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this);
        //CustomEvent.Trigger(GameObject.Find("GameManager"), "EndTurn");
        GameObject firstOrcTurnObj = firstDrawParent.parent.Find("First_OrcPlay").gameObject;
        firstOrcTurnObj.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        firstOrcTurnObj.SetActive(false);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    /// <summary>
    /// 카드 교체 함수
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="itemID"></param>
    /// <param name="first"></param>
    public void RedrawCallback(string ID, int itemID = -1, bool first = false) {
        SocketFormat.GameState state = PlayMangement.instance.socketHandler.gameState;
        SocketFormat.Card[] cards = state.players.myPlayer(PlayMangement.instance.player.isHuman).deck.handCards;
        SocketFormat.Card newCard = state.players.myPlayer(PlayMangement.instance.player.isHuman).newCard;
        int index = -1;
        for (int i = 0; i < firstDrawList.Count; i++) {
            bool sameCard = false;
            for (int j = 0; j < cards.Length; j++) {
                if (cards[j].itemId == firstDrawList[i].GetComponent<CardHandler>().itemID) {
                    sameCard = true;
                    break;
                }
            }
            if (!sameCard) index = i;
        }
        GameObject beforeCardObject = firstDrawList[index];

        GameObject card;
        if (newCard.type == "unit")
            card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
        else
            card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        string id;
        int itemId = -1;
        id = newCard.id;
        itemId = newCard.itemId;
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.FIRSTDRAW = true;
        handler.DrawCard(id, itemId);
        clm.AddMulliganCardInfo(handler.cardData, id, index);
        card.transform.SetParent(firstDrawParent);
        card.transform.SetSiblingIndex(index + 5);
        card.transform.position = beforeCardObject.transform.position;
        card.transform.localScale = beforeCardObject.transform.localScale;
        card.transform.rotation = beforeCardObject.transform.rotation;
        if (beforeCardObject.name == "UnitCard")
            beforeCardObject.transform.SetParent(cardStorage.Find("UnitCards"));
        else
            beforeCardObject.transform.SetParent(cardStorage.Find("MagicCards"));
        beforeCardObject.transform.localPosition = Vector3.zero;
        beforeCardObject.transform.eulerAngles = Vector3.zero;
        beforeCardObject.SetActive(false);
        firstDrawList[index] = card;
        card.SetActive(true);

        if (newCard.type == "magic") {
            card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = newCard.name;
            AddMagicAttribute(ref card);
        }
    }

    public GameObject InstantiateUnitCard(CardData data, int itemId) {
        GameObject card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
        card.transform.localScale = Vector3.zero;
        card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
        card.GetComponent<CardHandler>().cardData = data;

        return card;
    }

    public GameObject InstantiateMagicCard(CardData data, int itemId) {
        GameObject card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        card.transform.localScale = Vector3.zero;
        card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
        MagicDragHandler magic = card.AddComponent<MagicDragHandler>();
        card.GetComponent<CardHandler>().cardData = data;
        AddMagicAttribute(ref card, false);
        magic.DrawCard(data.cardId, itemId);
        magic.enabled = false;
        return card;
    }


    protected void AddMagicAttribute(ref GameObject card, bool isPlayer = true) {
        var cardData = card.GetComponent<CardHandler>().cardData;

        SkillModules.SkillHandler skillHandler = new SkillModules.SkillHandler();
        skillHandler.Initialize(cardData.skills, card, isPlayer);
        card.GetComponent<MagicDragHandler>().skillHandler = skillHandler;
    }
}
