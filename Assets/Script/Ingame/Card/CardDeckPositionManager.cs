using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckPositionManager : MonoBehaviour
{
    Transform slot_1;
    Transform slot_2;
    private bool firstDraw = true;
    private int cardNum = 0;
    List<GameObject> cardList;
    List<GameObject> firstDrawList;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardSpawnPos;
    [SerializeField] Transform firstDrawWindow;
    
    // Start is called before the first frame update
    void Start()
    {
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        slot_1 = transform.GetChild(0);
        slot_2 = transform.GetChild(1);
        //if(Screen.height > 1920.0f)
        //    slot_1.transform.localScale = slot_2.transform.localScale = new Vector3(1920.0f / Screen.height, 1920.0f / Screen.height, 1);
        slot_2.gameObject.SetActive(true);
        ChangeSlotHeight(0.9f);
        //PlayMangement.instance.socketHandler.OnReceiveSocketMessage.AddListener(() => AddCard());
    }

    public IEnumerator FirstDraw() {
        GameObject card = Instantiate(cardPrefab, cardSpawnPos);
        card.transform.SetParent(firstDrawWindow);
        card.SetActive(true);


        //string cardID = "ac1000" + Random.Range(1, 5);
        string cardID = PlayMangement.instance.socketHandler.gameState.players.human.CardsId[firstDrawList.Count];
        card.GetComponent<CardHandler>().DrawCard(cardID, true);
        AddAbilityInCardPrefab(cardID, ref card);

        iTween.MoveTo(card, firstDrawWindow.GetChild(firstDrawList.Count).position, 0.5f);
        firstDrawList.Add(card);
        card.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(0.5f);
        card.transform.Find("ChangeButton").gameObject.SetActive(true);
        if (firstDrawList.Count == 4) {
            yield return new WaitForSeconds(0.5f);
            firstDrawWindow.parent.Find("FinishButton").gameObject.SetActive(true);
        }
    }

    public void FirstAdditionalCardDraw() {
        GameObject card = Instantiate(cardPrefab);
        card.transform.SetParent(firstDrawWindow.parent);
        firstDrawList.Add(card);
        card.SetActive(true);
        string cardID = "ac10001";
        card.GetComponent<CardHandler>().DrawCard(cardID);
        card.transform.localPosition = new Vector3(0, 0, 0);
        card.transform.localScale = new Vector3(1.7f, 1.7f, 1);

        AddAbilityInCardPrefab(cardID, ref card);
    }

    /// <summary>
    /// 카드 특수효과 컴포넌트 추가
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="card"></param>
    public void AddAbilityInCardPrefab(string cardID, ref GameObject card) {
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
        if (cardDataPackage.data.ContainsKey(cardID)) {
            CardData cardData = cardDataPackage.data[cardID];
            foreach (var skill in cardData.skills) {
                foreach (var effect in skill.effects) {
                    var newComp = card.AddComponent(System.Type.GetType("SkillModules.Ability_" + effect.method));
                    if(newComp != null) {
                        ((Ability)newComp).InitData(skill);
                        ((Ability)newComp).isPlayer = true;
                    }
                }
            }
        }
    }

    public void FirstDrawCardChange() {
        foreach (GameObject cards in firstDrawList) {
            cards.transform.Find("ChangeButton").gameObject.SetActive(false);
        }
        StartCoroutine(DrawChangedCards());
        firstDrawWindow.parent.Find("FinishButton").GetComponent<Button>().enabled = false;
        firstDrawWindow.parent.Find("FinishButton").GetComponent<Image>().enabled = false;
        firstDrawWindow.parent.Find("FinishButton").GetChild(0).gameObject.SetActive(false);
        firstDrawWindow.parent.Find("FinishButton").gameObject.SetActive(false);
    }

    IEnumerator DrawChangedCards() {
        firstDrawWindow.parent.gameObject.GetComponent<Image>().enabled = false;
        while (firstDrawList.Count != 0) {
            yield return new WaitForSeconds(0.2f);
            AddCard(firstDrawList[0]);
            firstDrawList.RemoveAt(0);
        }
        yield return new WaitForSeconds(0.5f);
        AddCard();
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(GameObject.Find("GameManager"), "EndTurn");
    }

    public void AddCard(GameObject cardobj = null) {
        GameObject card;
        if (cardobj == null) {
            card = Instantiate(cardPrefab, cardSpawnPos);
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
            card.GetComponent<CardHandler>().RedrawCard();
            card.transform.localScale = new Vector3(1, 1, 1);
            if (target != null) {
                StartCoroutine(SendCardToHand(card, target));
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
        card.GetComponent<CardHandler>().RedrawCard();
        card.transform.localScale = new Vector3(1, 1, 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot_2.GetComponent<RectTransform>());
        if (target != null)
            StartCoroutine(SendCardToHand(card, target));
    }

    IEnumerator SendCardToHand(GameObject card, Transform target) {
        if (!firstDraw) {
            card.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);
            iTween.MoveTo(card, firstDrawWindow.position, 0.4f);
            yield return new WaitForSeconds(0.5f);
        }

        iTween.MoveTo(card, target.position, 0.3f);
        yield return new WaitForSeconds(0.3f);
        card.transform.SetParent(target);
        card.GetComponent<CardHandler>().RedrawCard();
        card.transform.localScale = new Vector3(1, 1, 1);
        card.GetComponent<CardHandler>().DisableCard();
        CardListManager csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
        csm.AddCardInfo(card.GetComponent<CardHandler>().cardData, card.GetComponent<CardHandler>().cardID);
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
                    slot_2.GetChild(2).SetAsFirstSibling();
                    slot_2.GetChild(4).SetParent(slot_1);
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
}
