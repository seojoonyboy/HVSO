using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckPositionManager : MonoBehaviour
{
    Transform slot_1;
    Transform slot_2;
    private int cardNum = 0;
    List<GameObject> cardList;
    List<GameObject> firstDrawList;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform firstDrawWindow;
    
    // Start is called before the first frame update
    void Start()
    {
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        slot_1 = transform.GetChild(0);
        slot_2 = transform.GetChild(1);
        slot_2.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FirstCardDraw() {
        GameObject card = Instantiate(cardPrefab);
        firstDrawList.Add(card);
        card.transform.SetParent(firstDrawWindow);
        card.SetActive(true);
        card.GetComponent<CardHandler>().DrawCard("ac10003", true);
        card.transform.localScale = new Vector3(1, 1, 1);
        card.transform.Find("CardContent/ChangeButton").gameObject.SetActive(true);
        if(firstDrawList.Count == 4)
            firstDrawWindow.parent.GetChild(1).gameObject.SetActive(true);
    }

    public void FirstAdditionalCardDraw() {
        GameObject card = Instantiate(cardPrefab);
        card.transform.SetParent(firstDrawWindow.parent);
        firstDrawList.Add(card);
        card.SetActive(true);
        card.GetComponent<CardHandler>().DrawCard("ac10001");
        card.transform.localPosition = new Vector3(0, 0, 0);
        card.transform.localScale = new Vector3(1.7f, 1.7f, 1);
    }

    public void FirstDrawCardChange() {
        firstDrawWindow.parent.GetChild(1).gameObject.SetActive(false);
        StartCoroutine(DrawChangedCards());
    }

    IEnumerator DrawChangedCards() {
        yield return new WaitForSeconds(0.5f);
        FirstAdditionalCardDraw();
        while (firstDrawList.Count != 0) {
            yield return new WaitForSeconds(0.5f);
            //firstDrawList[0].GetComponent<CardHandler>().RedrawSelf();
            AddCard(firstDrawList[0]);
            firstDrawList.RemoveAt(0);
        }
        yield return new WaitForSeconds(0.5f);
        firstDrawWindow.parent.gameObject.SetActive(false);
        CustomEvent.Trigger(GameObject.Find("GameManager"), "EndTurn");
    }

    public void AddCard(GameObject cardobj = null) {
        GameObject card;
        if (cardobj == null)
            card = Instantiate(cardPrefab);
        else
            card = cardobj;
        cardNum++;
        if (cardNum == 11) {
            Debug.Log("Card Number Out Of Range!!");
            return;
        }
        if (cardNum < 5) {
            card.transform.SetParent(slot_1);
        }
        else {
            switch (cardNum) {
                case 5:
                    slot_2.gameObject.SetActive(true);
                    ChangeSlotHeight(0.9f);
                    slot_1.GetChild(3).SetParent(slot_2);
                    card.transform.SetParent(slot_2);
                    break;
                case 7:
                    slot_2.GetChild(2).SetParent(slot_1);
                    card.transform.SetParent(slot_2);
                    break;
                case 9:
                    ChangeSlotHeight(0.79f);
                    slot_2.GetChild(3).SetParent(slot_1);
                    card.transform.SetParent(slot_2);
                    break;
                default:
                    card.transform.SetParent(slot_2);
                    break;
            }
        }
        cardList.Add(card);
        card.GetComponent<CardHandler>().RedrawSelf();
        card.transform.localScale = new Vector3(1, 1, 1);
        card.SetActive(true);
    }

    public void DestroyCard(int index) {
        cardNum--;
        if (cardNum == -1) {
            Debug.Log("Card Number Out Of Range!!");
            return;
        }
        Destroy(cardList[index]);
        cardList.RemoveAt(index);
        switch (cardNum) {
            case 9:
                if (index < 5) slot_2.GetChild(0).SetParent(slot_1);
                break;
            case 8:
                if (index > 4) {
                    slot_1.GetChild(3).SetParent(slot_2);
                    slot_2.GetChild(3).SetAsFirstSibling();
                }
                break;
            case 7:
                if (index < 4) slot_2.GetChild(0).SetParent(slot_1);
                ChangeSlotHeight(0.9f);
                break;
            case 6:
                if (index > 3) {
                    slot_1.GetChild(2).SetParent(slot_2);
                    slot_2.GetChild(2).SetAsFirstSibling();
                }
                break;
            case 5:
                if (index < 3) slot_2.GetChild(0).SetParent(slot_1);
                break;
            case 4:
                if (index < 3) {
                    slot_2.GetChild(1).SetParent(slot_1);
                    slot_2.GetChild(0).SetParent(slot_1);
                }
                else {
                    if(slot_2.GetChild(0))
                        slot_2.GetChild(0).SetParent(slot_1);
                    else
                        slot_2.GetChild(1).SetParent(slot_1);
                }
                slot_2.gameObject.SetActive(false);
                ChangeSlotHeight(1.0f);
                break;
            default:
                break;
        }
    }

    void ChangeSlotHeight(float rate) {
        transform.GetComponent<RectTransform>().localScale = new Vector2(rate, rate);
    }
}
