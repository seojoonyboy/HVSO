using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;

public partial class CardHandler : MonoBehaviour {
    public Vector3 startPos;
    public GameObject unit;
    public GameObject skeleton;
    CardListManager csm;
    protected bool blockButton = false;
    protected bool firstDraw = false;
    public bool changeSelected = false;
    Animator cssAni;
    public string cardID;
    protected int _itemID;
    public int itemID {
        get {return _itemID;}
        set {if(value < 0) Debug.Log("something wrong itemId");
             _itemID = value;
        }
    }

    protected bool highlighted = false;
    protected Transform highlightedSlot;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    protected static GameObject itsDragging;

    public bool FIRSTDRAW {
        get { return firstDraw; }
        set { firstDraw = value; }
    }

    public void Awake() {
        csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
    }

    public void DrawCard(string ID, int itemID = -1, bool first = false) {
        cardDataPackage = AccountManager.Instance.cardPackage;
        cardID = ID;
        //cardID = "ac10002";    //테스트 코드
        this.itemID = itemID;
        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
            transform.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[cardID];
            transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
            skeleton = AccountManager.Instance.resource.cardSkeleton[cardID];
        }
        else
            Debug.Log("NoData");
        if (cardData.type == "unit") {
            transform.Find("Health").gameObject.SetActive(true);
            transform.Find("attack").gameObject.SetActive(true);
            transform.Find("Health").Find("Text").GetComponent<Text>().text = cardData.hp.ToString();
            transform.Find("attack").Find("Text").GetComponent<Text>().text = cardData.attack.ToString();
        }
        else {
            transform.Find("Health").gameObject.SetActive(false);
            transform.Find("attack").gameObject.SetActive(false);
        }
        transform.Find("Cost").Find("Text").GetComponent<Text>().text = cardData.cost.ToString();

        if (first) {
            transform.Find("GlowEffect").GetComponent<Image>().enabled = true;
            transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 107.0f / 255.0f);
            csm.AddMulliganCardInfo(cardData, cardID);
            firstDraw = true;
        }
    }


    

    public void CheckHighlight() {
        if (!highlighted) {
            highlightedSlot = CheckSlot();
            if (highlightedSlot != null) {
                highlighted = true;
                transform.Find("GlowEffect").GetComponent<Image>().color = new Color(163.0f / 255.0f, 236.0f / 255.0f, 27.0f / 255.0f);
                CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            }
        }
        else {
            if (highlightedSlot != CheckSlot()) {
                highlighted = false;
                if (PlayMangement.instance.player.getPlayerTurn == true && PlayMangement.instance.player.resource.Value >= cardData.cost)
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 107.0f / 255.0f);
                else
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 1);
                CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
                highlightedSlot = null;
            }
        }
    }

    protected Transform CheckSlot() {
        Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        origin = new Vector3(origin.x, origin.y + 0.3f, origin.z);
        Ray2D ray = new Ray2D(origin, Vector2.zero);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider != null && hit.transform.gameObject.layer == 12) {
            return hit.transform;
        }

        return null;
    }

    public void OpenCardInfoList() {
        if (firstDraw) {
            csm.OpenMulliganCardList(transform.GetSiblingIndex() - 5);
            return;
        }
        if (!blockButton) {
            if (transform.parent.parent.name == "CardSlot_1") {
                csm.OpenCardList(transform.parent.GetSiblingIndex());
            }
            else {
                int cardIndex = 0;
                Transform slot1 = transform.parent.parent.parent.GetChild(0);
                for (int i = 0; i < 5; i++) {
                    if (slot1.GetChild(i).gameObject.activeSelf)
                        cardIndex++;
                }
                cardIndex += transform.parent.GetSiblingIndex();
                csm.OpenCardList(cardIndex);
            }
        }
    }

    public void RedrawButton() {
        CardHandDeckManager handManager = FindObjectOfType<CardHandDeckManager>();
        PlayMangement.instance.socketHandler.HandchangeCallback = handManager.RedrawCallback;
        PlayMangement.instance.socketHandler.ChangeCard(itemID);
    }

    public void DisableCard() {
        transform.Find("GlowEffect").GetComponent<Image>().enabled = false;
        transform.Find("Portrait").GetComponent<Image>().color = Color.gray;
        transform.Find("attack").GetComponent<Image>().color = Color.gray;
        transform.Find("Health").GetComponent<Image>().color = Color.gray;
        transform.Find("Cost").GetComponent<Image>().color = Color.gray;
    }

    public void ActivateCard() {
        if (PlayMangement.instance.player.resource.Value >= cardData.cost) {
            transform.Find("GlowEffect").GetComponent<Image>().enabled = true;
            transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 107.0f / 255.0f);
            transform.Find("Portrait").GetComponent<Image>().color = Color.white;
            transform.Find("attack").GetComponent<Image>().color = Color.white;
            transform.Find("Health").GetComponent<Image>().color = Color.white;
            transform.Find("Cost").GetComponent<Image>().color = Color.white;
        }
        else {
            transform.Find("GlowEffect").GetComponent<Image>().enabled = false;
            transform.Find("Portrait").GetComponent<Image>().color = Color.gray;
            transform.Find("attack").GetComponent<Image>().color = Color.gray;
            transform.Find("Health").GetComponent<Image>().color = Color.gray;
            transform.Find("Cost").GetComponent<Image>().color = Color.gray;
        }
    }
}

