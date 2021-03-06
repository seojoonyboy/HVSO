using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SocketFormat;

public class CardHandManager : MonoBehaviour {
    
    TMPro.TextMeshProUGUI handCardNum;
    [SerializeField] protected Transform cardStorage;
    [SerializeField] protected Transform showPos;
    [SerializeField] public Transform firstDrawParent;
    [SerializeField] public Transform cardSpawnPos;
    [SerializeField] Text cardNumValue;
    public bool isMultiple = false;
    public bool firstDraw = true;
    public bool socketDone = false;
    private bool cardDestroyed = false;
    protected List<GameObject> cardList = new List<GameObject>();
    protected List<GameObject> firstDrawList;
    protected List<int> cardPosition;

    protected CardListManager clm;

    public GameObject cardSlot;

    public int cardNum {
        get { return cardList.Count; }
    }


    // Start is called before the first frame update
    void Start() {
        firstDraw = true;
        firstDrawList = new List<GameObject>();
        clm = PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        if(ScenarioGameManagment.instance != null) cardDestroyed = true;
        SetUpDrawPosition(430, 125, 0);
        //handCardNum = transform.parent.Find("PlayerCardNum/Value").GetComponent<TMPro.TextMeshProUGUI>();
    }


    protected void SetUpDrawPosition(int cardPos, int distance, int num) {
        if (num > 9) return;
        if (cardPosition == null) cardPosition = new List<int>();
        cardPosition.Add(cardPos);
        if (num < 3) {
            cardPos -= distance;
            distance = 125;
        }
        else if (num == 3) {
            cardPos -= 190;
            distance = 250;
        }
        else {
            cardPos -= distance;
            distance = 250;
        }
        SetUpDrawPosition(cardPos, distance, ++num);
    }



    public IEnumerator RecoverCards(bool isHuman, Card[] cards) {
        foreach (var item in cards) {
            GameObject card;
            if (item.type == "unit")
                card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
            else {
                card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
            }
            card.GetComponent<CardHandler>().DrawCard(item.id, item.itemId);
            
            yield return new WaitForSeconds(1.0f);
        }
    }

    //????????? ?????? ?????????(?????? ????????? ?????? 4??? ?????????)
    public virtual IEnumerator FirstDraw() {
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

    //????????? ?????? ?????? ?????? ??????
    public void FirstDrawCardChange() {
        socketDone = true;
        if(ScenarioGameManagment.scenarioInstance == null) {
            OffFirstDrawWindow();
        }
        else {
            if(PlayMangement.chapterData.chapter == 0) StartCoroutine(DrawChangedCards());
            else {
                OffFirstDrawWindow();
                firstDrawParent.gameObject.SetActive(false);
            }
        }
        PlayMangement.instance.SetGameData();
        //PlayMangement.instance.resultManager.SetResultWindow("win", PlayMangement.instance.player.isHuman);
    }

    private void OffFirstDrawWindow() {
        clm.CloseMulliganCardList();
        foreach (GameObject cards in firstDrawList) {
            cards.transform.Find("ChangeButton").gameObject.SetActive(false);
            cards.GetComponent<CardHandler>().DisableCard();
        }
        AddInfoToList(null, true);
        StartCoroutine(DrawChangedCards());
        firstDrawParent.GetChild(4).gameObject.SetActive(false);
        Transform finBtn = firstDrawParent.parent.Find("FinishButton");
        finBtn.GetComponent<Button>().enabled = false;
        finBtn.GetComponent<Image>().enabled = false;
        finBtn.GetChild(0).gameObject.SetActive(false);
        finBtn.gameObject.SetActive(false);
    }
    
    //?????? ????????? ?????? ??????
    public Transform GetcardStorage() {
        return cardStorage;
    }


    /// <summary>
    /// CardListManager?????? ?????? ????????? ???????????? ?????? ??????
    /// </summary>
    /// <param name="card"></param> ????????? ????????? ?????? ????????????
    /// <param name="isMulligan"></param> ??????????????? true
    /// <param name="isHero"></param> ?????????????????? true
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
    /// ????????? ????????? ???????????? ??????
    /// </summary>
    /// <param name="cardobj"></param> ???????????? ?????? ?????? ?????? ???????????? ??????
    /// <param name="cardData"></param>
    public void AddCard(GameObject cardobj = null, SocketFormat.Card cardData = null) {
        //if (cardNum + 1 == 11) return;
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
            string itemId = null;
            if (cardData == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData.id;
                itemId = cardData.itemId;
            }
            card.GetComponent<CardHandler>().DrawCard(id, itemId);

            if (cardData.type == "magic") {
                card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
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
        //Transform cardPos = transform.GetChild(cardNum);
        //cardPos.gameObject.SetActive(true);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        cardTransform.gameObject.SetActive(true);
        cardList.Add(card);
        cardNumValue.text = cardNum.ToString();

        bool isHuman = PlayMangement.instance.player.isHuman;
        PlayMangement.instance.player.remainCardCount = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).deck.deckCardsCount;
        StartCoroutine(SendCardToHand(cardTransform.gameObject));
    }

    /// <summary>
    /// 2??? ????????? ?????? ????????? ??????
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public IEnumerator AddMultipleCard(SocketFormat.Card[] cardData, DequeueCallback callback = null) {
        yield return new WaitWhile(() => cardDestroyed == false);
        isMultiple = true;
        int currentNum = cardNum;
        for (int i = cardNum; i < cardData.Length + currentNum; i++) {
            //if (cardNum + 1 == 11) break;
            PlayMangement.dragable = false;
            GameObject card;
            if (cardData[i-currentNum].type == "unit")
                card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
            else
                card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
            string id;
            string itemId = null;
            if (cardData[i-currentNum] == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData[i-currentNum].id;
                itemId = cardData[i-currentNum].itemId;
            }
            card.GetComponent<CardHandler>().DrawCard(id, itemId);
            card.transform.Find("ChangeButton").gameObject.SetActive(false);
            if (cardData[i-currentNum].type == "magic") {
                card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData[i-currentNum].name;
            }
            AddInfoToList(card);
            Transform cardTransform = card.transform;
            //Transform cardPos = transform.GetChild(cardNum);
            //cardPos.gameObject.SetActive(true);


            cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
            cardTransform.gameObject.SetActive(true);
            cardList.Add(card);
            cardNumValue.text = cardNum.ToString();
            
            if (cardNum == cardData.Length)
                StartCoroutine(SendMultipleCardToHand(cardTransform.gameObject, true));
            else
                StartCoroutine(SendMultipleCardToHand(cardTransform.gameObject));
            yield return new WaitForSeconds(0.5f);
            if (i == cardData.Length - 1 || cardNum == 10) {
                isMultiple = false;
                cardDestroyed = false;
            }
        }
        bool isHuman = PlayMangement.instance.player.isHuman;
        PlayMangement.instance.player.remainCardCount = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).deck.deckCardsCount;
        callback?.Invoke();
    }

    /// <summary>
    /// ?????? ????????? ???????????? ?????????
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
        leftCard.GetComponent<MagicDragHandler>().isPlayer = true;
        rightCard.GetComponent<MagicDragHandler>().isPlayer = true;
        leftCard.transform.SetParent(showPos.Find("Left"));
        rightCard.transform.SetParent(showPos.Find("Right"));

        leftCard.SetActive(true);
        rightCard.SetActive(true);

        CardHandler handler = leftCard.GetComponent<CardHandler>();
        handler.DrawHeroCard(cards[0]);
        handler = rightCard.GetComponent<CardHandler>();
        handler.DrawHeroCard(cards[1]);

        ShowCardsHandler showCardsHandler = showPos.GetComponent<ShowCardsHandler>();
        showCardsHandler.AddCards(
            new GameObject[] { leftCard, rightCard },
            new string[] { cards[0].skills.desc, cards[1].skills.desc }
        );

        var tmp = showPos.Find("Left").position;
        iTween.MoveTo(
            leftCard, 
            new Vector3(tmp.x, tmp.y, 0),
            0.4f);
        iTween.RotateTo(leftCard, iTween.Hash("z", 0, "islocal", true, "time", 0.4f));

        iTween.RotateTo(rightCard, iTween.Hash("z", 0, "islocal", true, "time", 0.1f));

        yield return new WaitForSeconds(0.5f);
        tmp = showPos.Find("Right").position;
        iTween.MoveTo(
            rightCard,
            new Vector3(tmp.x, tmp.y, 0),
            0.6f);

        leftCard.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
        rightCard.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);

        yield return new WaitForSeconds(0.6f);
        showCardsHandler.ToggleAllCards();

        yield return handler.ActiveHeroCard();
    }

    /// <summary>
    /// ?????? ???????????? ?????? ???????????? ????????? ??????
    /// </summary>
    /// <param name="cardobj"></param>
    public void AddHeroCard(GameObject cardobj) {
        //if (cardNum + 1 == 11) return;
        PlayMangement.dragable = false;
        AddInfoToList(cardobj.transform.Find("CardInfoWindow").gameObject, false, true);
        Transform cardTransform = cardobj.transform;
        //Transform cardPos = transform.GetChild(cardNum);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        PlayMangement.instance.socketHandler.KeepHeroCard(cardTransform.GetComponent<CardHandler>().itemID);
        cardTransform.localScale = new Vector3(1, 1, 1);
        cardList.Add(cardobj);
        //cardPos.gameObject.SetActive(true);
        StartCoroutine(SendCardToHand(cardTransform.gameObject, true));
    }

    /// <summary>
    /// ????????? ?????? ???????????? ???????????????
    /// </summary>
    /// <param name="card"></param> ?????? ????????????
    /// <param name="isHero"></param>
    /// <returns></returns>
    IEnumerator SendCardToHand(GameObject card, bool isHero = false) {
        PlayMangement.dragable = false;
        CardHandler handler = card.GetComponent<CardHandler>();
        GameObject slot = FindEmptySlot();
        slot.SetActive(true);
        card.transform.SetParent(slot.transform);
        handler.DisableCard();
        //handCardNum.text = cardNum.ToString();

        if (!firstDraw) {
            if (!isHero) {
                iTween.MoveTo(card, showPos.position, 0.4f);
                iTween.RotateTo(card, slot.transform.eulerAngles, 0.4f);
            }
            iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
            //iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
            yield return new WaitForSeconds(0.4f);
        }
        if (cardNum > 2 && !isMultiple && !firstDraw) {
            iTween.MoveTo(transform.gameObject, iTween.Hash("x", -250 * (cardNum - 4), "islocal", true, "time", 0.2f));
            yield return new WaitForSeconds(0.3f);
        }
        //card.transform.SetParent(slot.transform);
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.4f);
        yield return new WaitForSeconds(0.5f);
        TurnType turn = PlayMangement.instance.currentTurn;
        if (turn == TurnType.BATTLE) {
            handler.DisableCard();
        }
        else if (!PlayMangement.instance.player.isHuman && (turn == TurnType.SECRET || turn == TurnType.ORC)) {
            string cardtype = turn == TurnType.SECRET ? "unit" : "magic";
            if (handler.cardData.type.CompareTo(cardtype) == 0)
                handler.DisableCard();
            else
                handler.ActivateCard();
        }
        else if (PlayMangement.instance.player.isHuman && turn == TurnType.HUMAN)
            handler.ActivateCard();
        else
            handler.DisableCard();
        handler.FIRSTDRAW = false;
        if (!isMultiple && !firstDraw)
            yield return SortDrawPosition();
        if (turn != TurnType.BATTLE)
            PlayMangement.dragable = true;
    }

    /// <summary>
    /// 2??? ????????? ????????? ????????? ?????? ???????????? ???????????????
    /// </summary>
    /// <param name="card"></param> ?????? ????????????
    /// <param name="pos"></param>
    /// <param name="isHero"></param>
    /// <returns></returns>
    IEnumerator SendMultipleCardToHand(GameObject card, bool isLast = false) {
        PlayMangement.dragable = false;
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.DisableCard();
        GameObject slot = FindEmptySlot();
        slot.SetActive(true);
        card.transform.SetParent(slot.transform);
        //handCardNum.text = cardNum.ToString();

        iTween.MoveTo(card, showPos.position, 0.4f);
        iTween.RotateTo(card, slot.transform.eulerAngles, 0.4f);
        iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
        yield return new WaitForSeconds(0.4f);

        if (cardNum > 2 && isLast) {
            iTween.MoveTo(transform.gameObject, iTween.Hash("x", -250 * (cardNum - 4), "islocal", true, "time", 0.2f));
            yield return new WaitForSeconds(0.3f);
        }
        //card.transform.SetParent(slot.transform);
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.4f);
        yield return new WaitForSeconds(0.5f);
        TurnType turn = PlayMangement.instance.currentTurn;
        if (turn == TurnType.BATTLE) {
            handler.DisableCard();
        }
        else if (!PlayMangement.instance.player.isHuman && turn == TurnType.SECRET) {
            if (handler.cardData.type == "unit")
                handler.DisableCard();
            else
                handler.ActivateCard();
        }
        else if (PlayMangement.instance.player.isHuman && turn == TurnType.HUMAN)
            handler.ActivateCard();
        else
            handler.DisableCard();
        handler.FIRSTDRAW = false;
        //if (isLast)
            
        if (turn != TurnType.BATTLE)
            PlayMangement.dragable = true;

        yield return SortDrawPosition();
    }

    /// <summary>
    /// ?????? ?????? ??????
    /// </summary>
    /// <param name="index"></param>
    public void DestroyCard(int index) {
        cardDestroyed = false;
        StartCoroutine(RemoveCardToStorage(index));
    }

    /// <summary>
    /// ?????? ?????? ????????? ?????? ?????? ??? ??????
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
        if (PlayMangement.instance.currentTurn != TurnType.BATTLE)
            PlayMangement.dragable = true;
    }

    /// <summary>
    /// ?????? ?????? ???????????? ?????? ????????? ?????? ?????? ??????
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
    /// ????????? ?????? CardStorage??? ??????
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
        cardNumValue.text = cardNum.ToString();
        //handCardNum.text = cardNum.ToString();
        for (int i = index; i < cardNum; i++) {
            transform.GetChild(i).GetChild(0).GetComponent<CardHandler>().CARDINDEX = i;
        }
        yield return SortDrawPosition();
        cardDestroyed = true;
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// ???????????? ????????? ?????? ?????? ?????? ??????
    /// </summary>
    /// <param name="index"></param>
    /// <param name="card"></param>
    /// <returns></returns>
    public IEnumerator ShowUsedCard(int index = 100, GameObject card = null) {
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

        if (card.GetComponent<UnitDragHandler>() != null && card.GetComponent<UnitDragHandler>().cardData.attributes.Length != 0 && card.GetComponent<UnitDragHandler>().cardData.attributes[0].name == "ambush")
            CardInfoOnDrag.instance.SetCardDragInfo(null, new Vector3(0, 5, 0), null);
        else
            CardInfoOnDrag.instance.SetCardDragInfo(null, new Vector3(0, 5, 0), handler.cardData.skills != null ? handler.cardData.skills.desc : null);
        
        yield return new WaitForSeconds(1.0f);
        
        card.transform.SetParent(parent);
        CardInfoOnDrag.instance.OffCardDragInfo();
        PlayMangement.instance.OffBlockPanel();
    }

    public void SetUsedCardInfo(ref GameObject card) {
        card.transform.localScale = Vector3.one;
        dataModules.CollectionCard cardData = card.GetComponent<CardHandler>().cardData;

        Image portrait = card.transform.Find("Portrait").GetComponent<Image>();
        TMPro.TextMeshProUGUI cost = card.transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>();
        
        Image skillIcon = null;
        bool isUnit = card.GetComponent<UnitDragHandler>() != null;
        if (isUnit) {
            TMPro.TextMeshProUGUI hp = card.transform.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI atk = card.transform.Find("attack/Text").GetComponent<TMPro.TextMeshProUGUI>();

            if (cardData.attributes.Length > 0 && Array.Exists(cardData.attributes, x => x.name == "ambush")) {
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
        if (isUnit == true && (cardData.attributes.Length > 0 && cardData.attributes[0].name == "ambush")) {
            portrait.sprite = AccountManager.Instance.resource.cardPortraite["ac10065"];
        }
        else
            portrait.sprite = AccountManager.Instance.resource.cardPortraite[cardData.id];

        if (cardData.attributes.Length == 0 && isUnit) skillIcon.gameObject.SetActive(false);

        if (cardData.attributes.Length != 0 && isUnit)
            if (skillIcon != null) skillIcon.sprite = AccountManager.Instance.resource.GetSkillIcons(cardData.attributes[0].name);
        
        if(!cardData.isHeroCard)
            card.transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
    }

    public IEnumerator SortHandPosition() {
        //switch (cardNum) {
        //    case 1:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 430, "islocal", true, "time", 0.1f));
        //        break;
        //    case 2:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 305, "islocal", true, "time", 0.1f));
        //        break;
        //    case 3:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 180, "islocal", true, "time", 0.1f));
        //        break;
        //    case 4:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 60, "islocal", true, "time", 0.1f));
        //        break;
        //    case 5:
        //        if (transform.localPosition.x < 0)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -130, "islocal", true, "time", 0.1f));
        //        break;
        //    case 6:
        //        if (transform.localPosition.x < -380)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -380, "islocal", true, "time", 0.1f));
        //        break;
        //    case 7:
        //        if (transform.localPosition.x < -625)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -625, "islocal", true, "time", 0.1f));
        //        break;
        //    case 8:
        //        if (transform.localPosition.x < -870)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -870, "islocal", true, "time", 0.1f));
        //        break;
        //    case 9:
        //        if (transform.localPosition.x < -1120)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -1120, "islocal", true, "time", 0.1f));
        //        break;
        //    case 10:
        //        if (transform.localPosition.x < -1210)
        //            iTween.MoveTo(gameObject, iTween.Hash("x", -1365, "islocal", true, "time", 0.1f));
        //        break;
        //}
        //if (cardNum > 4 && transform.localPosition.x > 0)
        //    iTween.MoveTo(gameObject, iTween.Hash("x", -0, "islocal", true, "time", 0.1f));
        //yield return new WaitForSeconds(0.1f);

        Hashtable hash_set;
        if (cardNum == 0 || cardNum == 1) {
            hash_set = iTween.Hash("x", cardPosition[0], "islocal", true, "time", 0.1f);
            iTween.MoveTo(gameObject, hash_set);
        }
        else {
            if (cardNum > 4) {
                if(transform.localPosition.x < (5 - cardNum) * 250) {
                    hash_set = iTween.Hash("x", cardPosition[cardNum - 1], "islocal", true, "time", 0.1f);
                    iTween.MoveTo(gameObject, hash_set);
                }
            }
            else {
                hash_set = iTween.Hash("x", cardPosition[cardNum - 1], "islocal", true, "time", 0.1f);
                iTween.MoveTo(gameObject, hash_set);
            }
        }


        if (cardNum > 4 && transform.localPosition.x > 0)
            iTween.MoveTo(gameObject, iTween.Hash("x", -0, "islocal", true, "time", 0.1f));
        yield return new WaitForSeconds(0.1f);

    }

    public IEnumerator SortDrawPosition() {
        Hashtable hash_set;
        if (cardNum == 0 || cardNum == 1)
            hash_set = iTween.Hash("x", cardPosition[0], "islocal", true, "time", 0.1f);
        else
            hash_set = iTween.Hash("x", cardPosition[cardNum - 1], "islocal", true, "time", 0.1f);

        iTween.MoveTo(gameObject, hash_set);


        //if (cardNum > 4 && transform.localPosition.x > 0)
        //    iTween.MoveTo(gameObject, iTween.Hash("x", -0, "islocal", true, "time", 0.1f));
        yield return new WaitForSeconds(0.1f);

        //switch (cardNum) {
        //    case 1:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 430, "islocal", true, "time", 0.1f));
        //        break;
        //    case 2:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 305, "islocal", true, "time", 0.1f));
        //        break;
        //    case 3:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 180, "islocal", true, "time", 0.1f));
        //        break;
        //    case 4:
        //        iTween.MoveTo(gameObject, iTween.Hash("x", 60, "islocal", true, "time", 0.1f));
        //        break;
        //    case 5:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -130, "islocal", true, "time", 0.1f));
        //        break;
        //    case 6:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -380, "islocal", true, "time", 0.1f));
        //        break;
        //    case 7:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -625, "islocal", true, "time", 0.1f));
        //        break;
        //    case 8:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -870, "islocal", true, "time", 0.1f));
        //        break;
        //    case 9:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -1120, "islocal", true, "time", 0.1f));
        //        break;
        //    case 10:                
        //        iTween.MoveTo(gameObject, iTween.Hash("x", -1365, "islocal", true, "time", 0.1f));
        //        break;
        //}


    }



    /// <summary>
    /// ????????? ????????? ?????? ????????? ?????? ??? ????????? ???????????? ?????????
    /// </summary>
    /// <returns></returns>
    IEnumerator DrawChangedCards() {
        firstDrawParent.parent.gameObject.GetComponent<Image>().enabled = false;
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.turn_over);        
        if(ScenarioGameManagment.scenarioInstance == null) {
            int index = 0;
            PlayMangement.dragable = false;
            if (ScenarioGameManagment.scenarioInstance != null)
                yield return new WaitUntil(() => ScenarioGameManagment.scenarioInstance.stopFirstCard == false);
            while (index < 4) {
                yield return new WaitForSeconds(0.2f);
                AddCard(firstDrawList[index]);
                index++;
            }
            yield return new WaitForSeconds(0.5f);
        }
        else {
            if (PlayMangement.chapterData.chapter == 0) {
                bool isHuman = PlayMangement.instance.player.isHuman;
                StartCoroutine(PlayMangement.instance.StoryDrawEnemyCard());
                yield return AddMultipleCard(PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).deck.handCards);
            }
            else {
                int index = 0;
                PlayMangement.dragable = false;
                if (ScenarioGameManagment.scenarioInstance != null)
                    yield return new WaitUntil(() => ScenarioGameManagment.scenarioInstance.stopFirstCard == false);
                while (index < 4) {
                    yield return new WaitForSeconds(0.2f);
                    AddCard(firstDrawList[index]);
                    index++;
                }
                yield return new WaitForSeconds(0.5f);
            }
           
        }
        firstDraw = false;
        
        PlayMangement.instance.isMulligan = false;
        firstDrawParent.gameObject.SetActive(false);
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_MULIGUN_CARD, this);

    }

    public IEnumerator EditorSkipMulligan() {
        PlayMangement.instance.isMulligan = false;
        firstDrawParent.gameObject.SetActive(false);
        yield break;
    }

    /// <summary>
    /// ?????? ?????? ??????
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="itemID"></param>
    /// <param name="first"></param>
    public void RedrawCallback(string ID, string itemID = null, bool first = false) {
        SocketFormat.GameState state = PlayMangement.instance.socketHandler.gameState;
        SocketFormat.Card[] cards = state.players.myPlayer(PlayMangement.instance.player.isHuman).deck.handCards;
        SocketFormat.Card newCard = state.players.myPlayer(PlayMangement.instance.player.isHuman).newCard;
        int index = -1;
        for (int i = 0; i < firstDrawList.Count; i++) {
            bool sameCard = false;
            for (int j = 0; j < cards.Length; j++) {
                if (cards[j].itemId.CompareTo(firstDrawList[i].GetComponent<CardHandler>().itemID) == 0) {
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
        string itemId = null;
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
        }
    }

    public GameObject InstantiateUnitCard(dataModules.CollectionCard data, string itemId) {
        GameObject card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
        card.transform.localScale = Vector3.zero;
        card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
        card.GetComponent<CardHandler>().cardData = data;

        return card;
    }

    public GameObject InstantiateMagicCard(dataModules.CollectionCard data, string itemId) {
        GameObject card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        card.transform.localScale = Vector3.zero;
        card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
        MagicDragHandler magic = card.AddComponent<MagicDragHandler>();
        card.GetComponent<CardHandler>().cardData = data;
        magic.DrawCard(data.id, itemId);
        magic.enabled = false;
        return card;
    }

    public GameObject FindCardWithItemId(string itemId) {
        GameObject card;
        card = showPos.GetComponent<ShowCardsHandler>().GetHeroCard;
        card = (card != null && card.GetComponent<CardHandler>().itemID == itemId) ? card : null; 
        card = (card == null) ? cardList.Find(x => x.GetComponent<CardHandler>().itemID == itemId) : card;
        return card != null ? card : null;
    }

    protected GameObject FindEmptySlot() {
        int cardSlotPos = cardNum - 1;
        if (cardSlotPos < transform.childCount && transform.GetChild(cardSlotPos).childCount == 0) return transform.GetChild(cardSlotPos).gameObject;

        //for (int i = cardSlotPos; i < transform.childCount; i++) {
        //    if (transform.GetChild(i).childCount == 0) return transform.GetChild(i).gameObject;
        //}

        GameObject newSlot = Instantiate(cardSlot, transform);
        newSlot.transform.SetAsLastSibling();
        cardPosition.Add(cardPosition[cardPosition.Count - 1] - 250);
        return newSlot;
    }
}
