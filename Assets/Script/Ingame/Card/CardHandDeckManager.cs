using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHandDeckManager : MonoBehaviour {
    Transform slot_1;
    Transform slot_2;
    private bool firstDraw = true;
    private int cardNum = 0;
    public bool isDrawing = false;
    List<GameObject> cardList;
    List<GameObject> firstDrawList;
    [SerializeField] GameObject unitCardPrefab;
    [SerializeField] GameObject magicCardPrefab;
    [SerializeField] public Transform cardSpawnPos;
    [SerializeField] Transform firstDrawParent;

    void Start() {
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        slot_1 = transform.GetChild(0);
        slot_2 = transform.GetChild(1);
        slot_2.gameObject.SetActive(true);
        ChangeSlotHeight(0.9f);
    }

    /// <summary>
    /// 게임 시작시 멀리건 실행 함수
    /// </summary>
    /// <returns></returns>
    public IEnumerator FirstDraw() {
        bool race = PlayMangement.instance.player.isHuman;
        SocketFormat.Card socketCard = PlayMangement.instance.socketHandler.gameState.players.myPlayer(race).FirstCards[firstDrawList.Count];
        GameObject card;
        if (socketCard.type == "unit")
            card = Instantiate(unitCardPrefab, cardSpawnPos);
        else {
            card = Instantiate(magicCardPrefab, cardSpawnPos);
        }
            
        card.transform.SetParent(firstDrawParent);
        card.SetActive(true);
        card.transform.rotation = new Quaternion(0, 0, 540, card.transform.rotation.w);
        card.GetComponent<CardHandler>().DrawCard(socketCard.id, socketCard.itemId, true);

        if(socketCard.type == "magic") {
            card.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = socketCard.name;
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

    void AddMagicAttribute(ref GameObject card) {
        var cardData = card.GetComponent<CardHandler>().cardData;
        foreach (dataModules.Skill skill in cardData.skills) {
            foreach (var effect in skill.effects) {
                var newComp = card.AddComponent(System.Type.GetType("SkillModules.MagicalCasting_" + effect.method));
                if (newComp == null) {
                    Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                }
                else {
                    //((SkillModules.MagicalCasting)newComp).InitData(skill, true);
                }
            }
        }
    }


    /// <summary>
    /// 멀리건 완료 버튼 클릭 함수
    /// </summary>
    public void FirstDrawCardChange() {
        foreach (GameObject cards in firstDrawList) 
            cards.transform.Find("ChangeButton").gameObject.SetActive(false);
        StartCoroutine(DrawChangedCards());
        firstDrawParent.GetChild(4).gameObject.SetActive(false);
        firstDrawParent.parent.Find("FinishButton").GetComponent<Button>().enabled = false;
        firstDrawParent.parent.Find("FinishButton").GetComponent<Image>().enabled = false;
        firstDrawParent.parent.Find("FinishButton").GetChild(0).gameObject.SetActive(false);
        firstDrawParent.parent.Find("FinishButton").gameObject.SetActive(false);
    }

    /// <summary>
    /// 멀리건 완료시 실행 함수(선택한 카드들 드로우)
    /// </summary>
    /// <returns></returns>
    IEnumerator DrawChangedCards() {
        firstDrawParent.parent.gameObject.GetComponent<Image>().enabled = false;        
        PlayMangement.instance.socketHandler.MulliganEnd();
        while (firstDrawList.Count != 0) {
            yield return new WaitForSeconds(0.2f);
            AddCard(firstDrawList[0]);
            firstDrawList.RemoveAt(0);
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
        firstDrawParent.parent.Find("First_OrcPlay").gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        firstDrawParent.parent.Find("First_OrcPlay").gameObject.SetActive(false);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void AddCard(GameObject cardobj = null, SocketFormat.Card cardData = null) {
        GameObject card;

        if (cardobj == null) {
            if (cardData.type == "unit")
                card = Instantiate(unitCardPrefab, cardSpawnPos);
            else
                card = Instantiate(magicCardPrefab, cardSpawnPos);
            string id;
            int itemId = -1;
            if(cardData == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData.id;
                itemId = cardData.itemId;
            }
            card.GetComponent<CardHandler>().DrawCard(id, itemId);

            if(cardData.type == "magic") {
                card.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
                AddMagicAttribute(ref card);
            }
        }
        else
            card = cardobj;
        card.SetActive(true);
        cardNum++;
        if (cardNum == 11) {
            Debug.Log("Card Number Out Of Range!!");
            return;
        }
        Transform target;
        if (firstDraw) {
            target = slot_1.GetChild(cardNum - 1);
            if (cardNum == 4) target = slot_2.GetChild(0);
            if (cardNum == 5) {
                target = slot_2.GetChild(1);
                firstDraw = false;
            }
            cardList.Add(card);
            card.transform.Find("GlowEffect").GetComponent<Image>().enabled = false;
            if (target != null) {
                isDrawing = false;
                card.transform.SetParent(target);
                StartCoroutine(SendCardToHand(card));
            }
            return;
        }
        if (cardNum < 5) {
            slot_1.GetChild(cardNum - 1).gameObject.SetActive(true);
            target = slot_1.GetChild(cardNum - 1);
        }
        else {
            switch (cardNum) {
                case 5:
                    slot_2.gameObject.SetActive(true);
                    ChangeSlotHeight(0.9f);
                    slot_2.GetChild(0).gameObject.SetActive(true);
                    slot_1.GetChild(3).GetChild(0).SetParent(slot_2.GetChild(0));
                    slot_2.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, 0);
                    slot_1.GetChild(3).gameObject.SetActive(false);
                    slot_2.GetChild(1).gameObject.SetActive(true);
                    target = slot_2.GetChild(1);
                    break;
                case 6:
                    slot_2.GetChild(2).gameObject.SetActive(true);
                    target = slot_2.GetChild(2);
                    break;
                case 7:
                    slot_1.GetChild(3).gameObject.SetActive(true);
                    slot_2.GetChild(0).GetChild(0).SetParent(slot_1.GetChild(3));
                    slot_1.GetChild(3).GetChild(0).localPosition = new Vector3(0, 0, 0);
                    slot_2.GetChild(0).gameObject.SetActive(false);
                    slot_2.GetChild(0).SetAsLastSibling();
                    slot_2.GetChild(2).gameObject.SetActive(true);
                    target = slot_2.GetChild(2);
                    break;
                case 8:
                    slot_2.GetChild(3).gameObject.SetActive(true);
                    target = slot_2.GetChild(3);
                    break;
                case 9:
                    ChangeSlotHeight(0.79f);
                    slot_1.GetChild(4).gameObject.SetActive(true);
                    slot_2.GetChild(0).GetChild(0).SetParent(slot_1.GetChild(4));
                    slot_1.GetChild(4).GetChild(0).localPosition = new Vector3(0, 0, 0);
                    slot_2.GetChild(0).gameObject.SetActive(false);
                    slot_2.GetChild(0).SetAsLastSibling();
                    slot_2.GetChild(3).gameObject.SetActive(true);
                    target = slot_2.GetChild(3);
                    break;
                case 10:
                    slot_2.GetChild(4).gameObject.SetActive(true);
                    target = slot_2.GetChild(4);
                    break;
                default:
                    target = null;
                    break;
            }
        }
        cardList.Add(card);
        card.transform.SetParent(target);
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_2.GetComponent<RectTransform>());
        if (target != null) {
            isDrawing = true;
            StartCoroutine(SendCardToHand(card));
        }
    }

    public IEnumerator AddMultipleCard(SocketFormat.Card[] cardData) {
        List<GameObject> cards = new List<GameObject>();
        //List<Transform> targets = new List<Transform>();        
        for (int i = cardNum; i < cardData.Length; i++) {
            GameObject card;
            Transform target;
            if (cardData[i].type == "unit")
                card = Instantiate(unitCardPrefab, cardSpawnPos);
            else
                card = Instantiate(magicCardPrefab, cardSpawnPos);
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
                card.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = cardData[i].name;
                AddMagicAttribute(ref card);
            }

            card.SetActive(true);
            cardNum++;
            if (cardNum == 11) {
                Debug.Log("Card Number Out Of Range!!");
            }
            if (cardNum < 5) {
                slot_1.GetChild(cardNum - 1).gameObject.SetActive(true);
                target = slot_1.GetChild(cardNum - 1);
            }
            else {
                switch (cardNum) {
                    case 5:
                        slot_2.gameObject.SetActive(true);
                        ChangeSlotHeight(0.9f);
                        slot_2.GetChild(0).gameObject.SetActive(true);
                        slot_1.GetChild(3).GetChild(0).SetParent(slot_2.GetChild(0));
                        slot_2.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, 0);
                        slot_1.GetChild(3).gameObject.SetActive(false);
                        slot_2.GetChild(1).gameObject.SetActive(true);
                        target = slot_2.GetChild(1);
                        break;
                    case 6:
                        slot_2.GetChild(2).gameObject.SetActive(true);
                        target = slot_2.GetChild(2);
                        break;
                    case 7:
                        slot_1.GetChild(3).gameObject.SetActive(true);
                        slot_2.GetChild(0).GetChild(0).SetParent(slot_1.GetChild(3));
                        slot_1.GetChild(3).GetChild(0).localPosition = new Vector3(0, 0, 0);
                        slot_2.GetChild(0).gameObject.SetActive(false);
                        slot_2.GetChild(0).SetAsLastSibling();
                        slot_2.GetChild(2).gameObject.SetActive(true);
                        target = slot_2.GetChild(2);
                        break;
                    case 8:
                        slot_2.GetChild(3).gameObject.SetActive(true);
                        target = slot_2.GetChild(3);
                        break;
                    case 9:
                        ChangeSlotHeight(0.79f);
                        slot_1.GetChild(4).gameObject.SetActive(true);
                        slot_2.GetChild(0).GetChild(0).SetParent(slot_1.GetChild(4));
                        slot_1.GetChild(4).GetChild(0).localPosition = new Vector3(0, 0, 0);
                        slot_2.GetChild(0).gameObject.SetActive(false);
                        slot_2.GetChild(0).SetAsLastSibling();
                        slot_2.GetChild(3).gameObject.SetActive(true);
                        target = slot_2.GetChild(3);
                        break;
                    case 10:
                        slot_2.GetChild(4).gameObject.SetActive(true);
                        target = slot_2.GetChild(4);
                        break;
                    default:
                        target = null;
                        break;
                }
            }
            cards.Add(card);
            //targets.Add(target);
            card.transform.SetParent(target);
        }
        for (int i = 0; i < cards.Count; i++) {
            cardList.Add(cards[i]);
            LayoutRebuilder.ForceRebuildLayoutImmediate(slot_1.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(slot_2.GetComponent<RectTransform>());
            isDrawing = true;
            StartCoroutine(SendCardToHand(cards[i]));
            yield return new WaitForSeconds(0.5f);
        }
    }


    IEnumerator SendCardToHand(GameObject card) {
        PlayMangement.movingCard = card;
        if (!firstDraw) {
            card.transform.rotation = new Quaternion(0, 0, 180, card.transform.rotation.w);
            iTween.MoveTo(card, firstDrawParent.position, 0.4f);
            iTween.RotateTo(card, new Vector3(0, 0, 0), 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
        
        iTween.MoveTo(card, iTween.Hash("x", card.transform.parent.position.x, "y", card.transform.parent.position.y, "time" ,0.5f, "easetype" , iTween.EaseType.easeWeakOutBack));
        iTween.ScaleTo(card, new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
        yield return new WaitForSeconds(0.5f);
        card.GetComponent<CardHandler>().DisableCard();
        if (PlayMangement.instance.player.getPlayerTurn) {
            if (!PlayMangement.instance.player.isHuman && card.GetComponent<CardHandler>().cardData.type == "unit")
                card.GetComponent<CardHandler>().DisableCard();
            else
                card.GetComponent<CardHandler>().ActivateCard();
            
        }
        card.GetComponent<CardHandler>().FIRSTDRAW = false;
        CardListManager csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
        csm.AddCardInfo(card.GetComponent<CardHandler>().cardData, card.GetComponent<CardHandler>().cardID);
        card.transform.localPosition = new Vector3(0, 0, 0);
        if (PlayMangement.movingCard == card) {
            PlayMangement.movingCard = null;
            InitCardPosition();
        }
    }

    private void InitCardPosition() {
        foreach(GameObject card in cardList) {
            card.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void DestroyCard(int index) {
        cardNum--;
        if (cardNum == -1) {
            Debug.Log("Card Number Out Of Range!!");
            return;
        }
        cardList[index].transform.parent.SetAsLastSibling();
        switch (cardNum) {
            case 9:
                if (index < 5) {
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_1.GetChild(4).SetParent(slot_2);
                }
                break;
            case 8:
                if (index > 4) {
                    slot_1.GetChild(4).SetParent(slot_2);
                    slot_2.GetChild(5).SetAsFirstSibling();
                    slot_2.GetChild(5).SetParent(slot_1);
                }
                break;
            case 7:
                if (index < 4) {
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_1.GetChild(4).SetParent(slot_2);
                    slot_1.GetChild(3).SetAsLastSibling();
                }
                ChangeSlotHeight(0.9f);
                break;
            case 6:
                if (index > 3) {
                    slot_1.GetChild(3).SetParent(slot_2);
                    slot_2.GetChild(4).SetParent(slot_1);
                    slot_2.GetChild(4).SetAsFirstSibling();
                }
                break;
            case 5:
                if (index < 3) {
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_1.GetChild(2).SetParent(slot_2);
                    slot_1.GetChild(2).SetAsLastSibling();
                    slot_1.GetChild(2).SetAsLastSibling();
                }
                break;
            case 4:
                if (index < 3) {
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_1.GetChild(2).SetParent(slot_2);
                    slot_1.GetChild(2).SetParent(slot_2);
                    slot_1.GetChild(2).SetAsLastSibling();
                }
                else {
                    slot_2.GetChild(0).SetParent(slot_1);
                    slot_1.GetChild(3).SetParent(slot_2);
                    slot_1.GetChild(3).SetAsLastSibling();
                }
                slot_2.gameObject.SetActive(false);
                ChangeSlotHeight(1.0f);
                break;
            default:
                break;
        }
        cardList[index].transform.parent.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_2.GetComponent<RectTransform>());
        Destroy(cardList[index]);
        cardList.RemoveAt(index);
        CardListManager csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
        csm.RemoveCardInfo(index);
    }

    void ChangeSlotHeight(float rate) {
        transform.GetComponent<RectTransform>().localScale = new Vector2(rate, rate);
    }

    public void RedrawCallback(string ID, int itemID = -1, bool first = false) {
        SocketFormat.GameState state = PlayMangement.instance.socketHandler.gameState;
        SocketFormat.Card[] cards = state.players.myPlayer(PlayMangement.instance.player.isHuman).deck.handCards;
        SocketFormat.Card newCard = state.players.myPlayer(PlayMangement.instance.player.isHuman).newCard;
        int index = -1;
        for(int i = 0; i < firstDrawList.Count; i++) {
            bool sameCard = false;
            for(int j = 0; j < cards.Length; j++) {
                if(cards[j].itemId == firstDrawList[i].GetComponent<CardHandler>().itemID) {
                    sameCard = true;
                    break;
                }
            }
            if(!sameCard) index = i;
        }
        GameObject beforeCardObject = firstDrawList[index];

        GameObject card;
        if (newCard.type == "unit")
            card = Instantiate(unitCardPrefab, firstDrawParent);
        else
            card = Instantiate(magicCardPrefab, firstDrawParent);
        string id;
        int itemId = -1;
        id = newCard.id;
        itemId = newCard.itemId;
        card.GetComponent<CardHandler>().DrawCard(id, itemId);
        GameObject infoList = GameObject.Find("CardInfoList");
        infoList.GetComponent<CardListManager>().AddMulliganCardInfo(card.GetComponent<CardHandler>().cardData, id);
        Transform firstcardinfolist = firstDrawParent.parent.Find("FirstCardInfoList");
        DestroyImmediate(firstcardinfolist.GetChild(index).gameObject);
        firstcardinfolist.GetChild(3).SetSiblingIndex(index);
        card.transform.position = beforeCardObject.transform.position;
        card.transform.SetSiblingIndex(index + 5);
        card.transform.localScale = beforeCardObject.transform.localScale;
        Destroy(beforeCardObject);
        firstDrawList[index] = card;
        card.SetActive(true);

        if (newCard.type == "magic") {
            card.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = newCard.name;
            AddMagicAttribute(ref card);
        }
    }
}
