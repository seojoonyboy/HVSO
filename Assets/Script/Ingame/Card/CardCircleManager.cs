using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardCircleManager : MonoBehaviour {
    public int cardNum = 0;
    [SerializeField] Transform cardStorage;
    [SerializeField] Transform showPos;
    [SerializeField] protected Transform firstDrawParent;
    [SerializeField] public Transform cardSpawnPos;
    public bool isMultiple = false;
    public bool firstDraw = true;
    protected List<GameObject> cardList;
    protected List<GameObject> firstDrawList;
    protected CardListManager clm;



    // Update is called once per frame
    private void Start() {
        firstDraw = true;
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        clm = PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
    }

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
            AddMagicAttribute(ref card);
        }

        iTween.MoveTo(card, firstDrawParent.GetChild(firstDrawList.Count).position, 0.5f);
        iTween.RotateTo(card, new Vector3(0, 0, 0), 0.5f);
        firstDrawList.Add(card);
        card.transform.localScale = new Vector3(1.15f, 1.15f, 1);
        yield return new WaitForSeconds(0.5f);
        card.transform.Find("ChangeButton").gameObject.SetActive(true);
        if (firstDrawList.Count == 4) {
            yield return new WaitForSeconds(0.5f);
            firstDrawParent.parent.Find("FinishButton").gameObject.SetActive(true);
        }
    }

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
    }

    public Transform GetcardStorage() {
        return cardStorage;
    }

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

    public void AddCard(GameObject cardobj = null, SocketFormat.Card cardData = null) {
        if (cardNum + 1 == 11) return;
        PlayMangement.dragable = false;
        GameObject card;
        if (cardobj == null) {
            if (cardData.type == "unit")
                card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
            else
                card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
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
            if (cardNum + 1 == 5) {
                AddInfoToList(card);
                firstDraw = false;
            }
            card.transform.Find("GlowEffect").GetComponent<Image>().enabled = false;
        }
        else
            AddInfoToList(card);
        Transform cardTransform = card.transform;
        Transform cardPos = transform.GetChild(cardNum).GetChild(0);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        cardTransform.gameObject.SetActive(true);
        cardTransform.SetParent(transform.GetChild(cardNum));
        cardNum++;
        cardList.Add(card);
        StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos));
    }


    public IEnumerator DrawHeroCard(SocketFormat.Card cardData = null) {
        GameObject card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        card.transform.SetParent(showPos.transform);
        card.SetActive(true);
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.DrawHeroCard(cardData);
        iTween.MoveTo(card, showPos.position, 0.4f);
        iTween.RotateTo(card, iTween.Hash("rotation", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
        yield return StartCoroutine(handler.ActiveHeroCard());
    }

    public void AddHeroCard(GameObject cardobj) {
        if (cardNum + 1 == 11) return;
        PlayMangement.dragable = false;
        AddInfoToList(cardobj.transform.Find("CardInfoWindow").gameObject, false, true);
        Transform cardTransform = cardobj.transform;
        Transform cardPos = transform.GetChild(cardNum).GetChild(0);
        cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
        cardTransform.localScale = new Vector3(1, 1, 1);
        cardNum++;
        cardList.Add(cardobj);
        StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos, true));
    }

    public IEnumerator SendMultipleCard(SocketFormat.Card[] cardData) {
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

            if (cardData[i].type == "magic") {
                card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData[i].name;
                AddMagicAttribute(ref card);
            }
            AddInfoToList(card); 
            Transform cardTransform = card.transform;
            Transform cardPos = transform.GetChild(cardNum).GetChild(0);
            cardTransform.GetComponent<CardHandler>().CARDINDEX = cardNum;
            cardTransform.gameObject.SetActive(true);
            cardTransform.SetParent(transform.GetChild(cardNum));
            cardNum++;
            cardList.Add(card);
            StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos));
            yield return new WaitForSeconds(0.3f);
            if (i == cardData.Length - 1 || cardNum == 10)
                isMultiple = false;
        }
    }


    IEnumerator SendCardToHand(GameObject card, Transform pos, bool isHero = false) {
        CardHandler handler = card.GetComponent<CardHandler>();
        handler.DisableCard();
        if (!firstDraw) {
            if (!isHero) {
                iTween.MoveTo(card, showPos.position, 0.4f);
                iTween.RotateTo(card, iTween.Hash("rotation", new Vector3(0, 0, 0), "islocal", true, "time", 0.4f));
            }
            iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
            iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
            yield return new WaitForSeconds(0.4f);
            if(isHero)
                card.transform.SetParent(transform.GetChild(cardNum - 1));
        }
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 4500, 0), "islocal", true, "time", 0.4f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.4f);
        yield return new WaitForSeconds(0.4f);
        card.transform.rotation = pos.rotation;
        if (cardNum > 2 && !isMultiple && !firstDraw)
            iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 2) * 4), 0.4f);
        yield return new WaitForSeconds(0.5f);
        if (PlayMangement.instance.player.getPlayerTurn) {
            if (!PlayMangement.instance.player.isHuman && handler.cardData.type == "unit")
                handler.DisableCard();
            else
                handler.ActivateCard();
        }
        handler.FIRSTDRAW = false;
        PlayMangement.dragable = true;
    }

    public void DestroyCard(int index) {
        StartCoroutine(RemoveCardToStorage(index));
    }

    public void DestroyCard(GameObject card) {
        PlayMangement.dragable = false;
        card.transform.SetParent(cardStorage.Find("MagicCards"));
        card.SetActive(false);
        card.transform.localPosition = Vector3.zero;
        card.transform.rotation = card.transform.parent.rotation;
    }

    private IEnumerator RemoveCardToStorage(int index) {
        PlayMangement.dragable = false;
        Transform removeCard = transform.GetChild(index).GetChild(1);
        iTween.RotateTo(transform.GetChild(index).gameObject, iTween.Hash("rotation", new Vector3(0, 0, -4 * (cardNum - 1)), "islocal", true, "time", 0.2f));
        transform.GetChild(index).SetSiblingIndex(cardNum - 1);
        removeCard.SetParent(cardStorage);
        removeCard.gameObject.SetActive(false);
        removeCard.eulerAngles = Vector3.zero;
        removeCard.localPosition = Vector3.zero;
        cardList.RemoveAt(index);
        if (removeCard.name == "MagicCard")
            clm.RemoveCardInfo(index);
        else
            clm.AddFeildUnitInfo(index, PlayMangement.instance.unitNum - 1);
        cardNum--;
        for (int i = index; i < cardNum; i++) {
            transform.GetChild(i).GetChild(1).GetComponent<CardHandler>().CARDINDEX = i;
            iTween.RotateTo(transform.GetChild(i).gameObject, iTween.Hash("rotation", new Vector3(0, 0, -4 * i), "islocal", true, "time", 0.2f));
        }
        StartCoroutine(SortCircleAngle());
        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator ShowUsedMagicCard(int index = 100, GameObject card = null) {
        if (index == 100) {
            clm.SetEnemyMagicCardInfo(card.GetComponent<CardHandler>().cardData);
        }
        else
            clm.OpenCardInfo(index, true);
        yield return new WaitForSeconds(1.5f);
        if(index == 100) {
            Transform infoWindow = clm.StandbyInfo.GetChild(0);
            infoWindow.gameObject.SetActive(false);
            infoWindow.localScale = new Vector3(1, 1, 1);
        }
        
    }

    public IEnumerator SortCircleAngle() {
        PlayMangement.dragable = false;
        Debug.Log(transform.rotation.eulerAngles.z);
        if (transform.rotation.eulerAngles.z > 300 && transform.rotation.eulerAngles.z < 360) {
            if (cardNum < 3)
                iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 2) * 4), 0.2f);
            else
                iTween.RotateTo(gameObject, new Vector3(0, 0, 4), 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
        else if (transform.rotation.eulerAngles.z < 4 && cardNum > 2) {
            iTween.RotateTo(gameObject, new Vector3(0, 0, 4), 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
        else if (transform.rotation.eulerAngles.z < 60 && transform.rotation.eulerAngles.z > (cardNum - 2) * 4 && cardNum > 0) {
            if (cardNum > 1)
                iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 2) * 4), 0.2f);
            else
                iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
        PlayMangement.dragable = true;
    }

    IEnumerator DrawChangedCards() {
        firstDrawParent.parent.gameObject.GetComponent<Image>().enabled = false;
        PlayMangement.instance.socketHandler.MulliganEnd();
        int index = 0;
        while (index < 4) {
            yield return new WaitForSeconds(0.2f);
            AddCard(firstDrawList[index]);
            index++;
        }
        yield return new WaitForSeconds(0.5f);
        yield return PlayMangement.instance.socketHandler.WaitGetCard();

        //영웅카드 뽑기
        bool isHuman = PlayMangement.instance.player.isHuman;
        SocketFormat.Card cardData = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).newCard;
        AddCard(null, cardData);


        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(GameObject.Find("GameManager"), "EndTurn");
        PlayMangement.instance.isMulligan = false;
        firstDrawParent.gameObject.SetActive(false);
        GameObject firstOrcTurnObj = firstDrawParent.parent.Find("First_OrcPlay").gameObject;
        firstOrcTurnObj.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        firstOrcTurnObj.SetActive(false);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

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
