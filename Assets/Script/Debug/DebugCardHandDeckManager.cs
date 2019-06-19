using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCardHandDeckManager : MonoBehaviour
{
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
    

    void AddMagicAttribute(ref GameObject card) {
        var cardData = card.GetComponent<DebugCardHandler>().cardData;
        foreach (dataModules.Skill skill in cardData.skills) {
            foreach (var effect in skill.effects) {
                var newComp = card.AddComponent(System.Type.GetType("SkillModules.MagicalCasting_" + effect.method));
                if (newComp == null) {
                    Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                }
                else {
                    ((SkillModules.MagicalCasting)newComp).InitData(skill, true);
                }
            }
        }
    }
    
    

    public void AddCard(GameObject cardobj = null, CardData cardData = null) {
        GameObject card;
        DebugManagement.dragable = false;
        if (cardobj == null) {
            if (cardData.type == "unit")
                card = Instantiate(unitCardPrefab, cardSpawnPos);
            else
                card = Instantiate(magicCardPrefab, cardSpawnPos);
            string id;
            int itemId = -1;
            if (cardData == null)
                id = "ac1000" + UnityEngine.Random.Range(1, 10);
            else {
                id = cardData.cardId;
            }
            card.GetComponent<DebugCardHandler>().DrawCard(id, itemId);

            if (cardData.type == "magic") {
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
        DebugManagement.dragable = false;
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
            card.GetComponent<DebugCardHandler>().DrawCard(id, itemId);

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
        if (!firstDraw) {
            AddInfoToList(card);
            card.transform.rotation = new Quaternion(0, 0, 180, card.transform.rotation.w);
            iTween.MoveTo(card, firstDrawParent.position, 0.4f);
            iTween.RotateTo(card, new Vector3(0, 0, 0), 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        iTween.MoveTo(card, iTween.Hash("x", card.transform.parent.position.x, "y", card.transform.parent.position.y, "time", 0.5f, "easetype", iTween.EaseType.easeWeakOutBack));
        iTween.ScaleTo(card, new Vector3(1.0f, 1.0f, 1.0f), 0.2f);

        yield return new WaitForSeconds(0.5f);
        
            if (!DebugManagement.Instance.player.isHuman && card.GetComponent<DebugCardHandler>().cardData.type == "unit")
                card.GetComponent<DebugCardHandler>().DisableCard();
            else
                card.GetComponent<DebugCardHandler>().ActivateCard();

        InitCardPosition();
    }


    public void AddInfoToList(GameObject card, bool isMulligan = false) {
        CardListManager csm = DebugManagement.Instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        if (isMulligan)
            csm.SendMulliganInfo();
        else
            csm.AddCardInfo(card.GetComponent<CardHandler>().cardData, card.GetComponent<CardHandler>().cardID);
    }

    private void InitCardPosition() {
        foreach (GameObject card in cardList) {
            card.transform.localPosition = new Vector3(0, 0, 0);
        }
        DebugManagement.dragable = true;
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
        CardListManager csm = DebugManagement.Instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        csm.RemoveCardInfo(index);
    }

    void ChangeSlotHeight(float rate) {
        transform.GetComponent<RectTransform>().localScale = new Vector2(rate, rate);
    }
   
}
