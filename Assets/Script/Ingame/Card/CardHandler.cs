using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;

public class CardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Vector3 startPos;
    public GameObject unit;
    public GameObject skeleton;
    CardListManager csm;
    private bool blockButton = false;
    public bool firstDraw = false;
    public bool changeSelected = false;
    Animator cssAni;
    public string cardID;
    private int _itemID;
    private int itemID {
        get {return _itemID;}
        set {if(value < 0) Debug.Log("something wrong itemId");
             _itemID = value;
        }
    }

    private bool highlighted = false;
    private Transform highlightedSlot;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    static GameObject itsDragging;

    public void Awake() {
        csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
    }

    public void DrawCard(string ID, int itemID = -1, bool first = false) {
        cardDataPackage = AccountManager.Instance.cardPackage;
        cardID = ID;
        //cardID = "ac10010";    //테스트 코드
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


    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        startPos = transform.parent.position;
        PlayMangement.instance.player.isPicking.Value = true;

        UnitDropManager.Instance.ShowDropableSlot(cardData, true);

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, cardScreenPos.z);
        CheckHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        iTween.MoveTo(gameObject, startPos, 0.3f);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if (PlayMangement.instance.player.getPlayerTurn == true && PlayMangement.instance.player.resource.Value >= cardData.cost) {
            GameObject unitPref = UnitDropManager.Instance.DropUnit(gameObject, CheckSlot());
            foreach (dataModules.Skill skill in cardData.skills) {
                foreach (var effect in skill.effects) {
                    var newComp = unitPref.AddComponent(Type.GetType("SkillModules.Ability_" + effect.method));
                    if (newComp == null) {
                        Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                    }
                    else {
                        ((Ability)newComp).InitData(skill, true);
                    }
                }
            }

            object[] parms = new object[] { true, unitPref };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        }
        else {
            highlighted = false;
            CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            CardDropManager.Instance.HideDropableSlot();
            highlightedSlot = null;
        }

        CardDropManager.Instance.HideDropableSlot();
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

    private Transform CheckSlot() {
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

    public void RedrawCard() {
        if (cardID == null) {
            string id = "ac1000" + UnityEngine.Random.Range(1, 5);
            DrawCard(id);
        }
        else
            DrawCard(cardID);
        firstDraw = false;
        transform.Find("ChangeButton").gameObject.SetActive(false);
    }

    public void RedrawButton() {
        //DrawCard("ac1000" + UnityEngine.Random.Range(1, 5));
        PlayMangement.instance.socketHandler.HandchangeCallback = DrawCard;
        PlayMangement.instance.socketHandler.ChangeCard(itemID);
        transform.Find("ChangeButton").gameObject.SetActive(false);
        transform.Find("GlowEffect").GetComponent<Image>().enabled = false;
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
