using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class CardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Vector3 startPos;
    public GameObject unit;
    public GameObject skeleton;
    private bool blockButton = false;
    public bool firstDraw = false;
    public bool changeSelected = false;
    CardListManager csm;
    Animator cssAni;
    private string cardID;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    public void Start() {

    }

    public void DrawCard(string ID, bool first = false) {
        cardDataPackage = AccountManager.Instance.cardPackage;
        cardID = ID;

        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
            skeleton = Resources.Load<GameObject>("Sprite/" + cardID + "/Skeleton_" + cardID);
        }
        else
            Debug.Log("NoData");

        transform.Find("CardContent/Health").Find("Text").GetComponent<Text>().text = cardData.hp.ToString();
        transform.Find("CardContent/attack").Find("Text").GetComponent<Text>().text = cardData.attack.ToString();
        transform.Find("CardContent/Cost").Find("Text").GetComponent<Text>().text = cardData.cost.ToString();

        if (first) firstDraw = true;
    }


    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw) return;
        blockButton = true;
        startPos = transform.position;
        PlayMangement.instance.player.isPicking.Value = true;
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        transform.position = startPos;
        blockButton = false;
        PlayMangement.instance.player.isPicking.Value = false;
    }

    public void OpenCardInfoList() {
        if (firstDraw) {
            return;
        }
        if (!blockButton) {
            if (transform.parent.name == "CardSlot_1") {
                csm.OpenCardList(transform.GetSiblingIndex());
            }
            else {
                csm.OpenCardList(GameObject.Find("CardSlot_1").transform.childCount + transform.GetSiblingIndex());
            }
        }
    }

    public void RedrawSelf() {
        if (cardID == null)
            DrawCard("ac10001");
        else
            DrawCard(cardID);
        firstDraw = false;
        transform.Find("CardContent/ChangeButton").gameObject.SetActive(false);
        csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
        csm.AddCardInfo(cardData);
    }

    public void RedrawCard() {
        DrawCard("ac10008");
        transform.Find("CardContent/ChangeButton").gameObject.SetActive(false);
    }

    public void DisableCard() {
        gameObject.transform.GetChild(0).Find("Portrait").GetComponent<Image>().color = Color.gray;
        gameObject.transform.GetChild(0).Find("attack").GetComponent<Image>().color = Color.gray;
        gameObject.transform.GetChild(0).Find("Health").GetComponent<Image>().color = Color.gray;
        gameObject.transform.GetChild(0).Find("Cost").GetComponent<Image>().color = Color.gray;
    }

    public void ActivateCard() {
        gameObject.transform.GetChild(0).Find("Portrait").GetComponent<Image>().color = Color.white;
        gameObject.transform.GetChild(0).Find("attack").GetComponent<Image>().color = Color.white;
        gameObject.transform.GetChild(0).Find("Health").GetComponent<Image>().color = Color.white;
        gameObject.transform.GetChild(0).Find("Cost").GetComponent<Image>().color = Color.white;
    }


}
