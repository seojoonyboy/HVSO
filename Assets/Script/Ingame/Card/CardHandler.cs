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
    private bool blockButton = false;
    public bool firstDraw = false;
    public bool changeSelected = false;
    CardListManager csm;
    Animator cssAni;
    private string cardID;

    private bool highlighted = false;
    private Transform highlightedSlot;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    static GameObject itsDragging;

    public void Start() {

    }

    public void DrawCard(string ID, bool first = false) {
        cardDataPackage = AccountManager.Instance.cardPackage;
        cardID = ID;

        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
            transform.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[cardID];
            skeleton = Resources.Load<GameObject>("Skeleton/Skeleton_" + cardID);
        }
        else
            Debug.Log("NoData");

        transform.Find("Health").Find("Text").GetComponent<Text>().text = cardData.hp.ToString();
        transform.Find("attack").Find("Text").GetComponent<Text>().text = cardData.attack.ToString();
        transform.Find("Cost").Find("Text").GetComponent<Text>().text = cardData.cost.ToString();

        if (first) firstDraw = true;
    }


    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.drawCard) return;
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.drawCard = true;
        startPos = transform.parent.position;
        PlayMangement.instance.player.isPicking.Value = true;

        var abilities = GetComponents<Ability>();
        bool isNormalDropableSlot = true;
        foreach(Ability ability in abilities) {
            ability.BeginCardPlay();
            if (ability.isChangeDropableSlot) {
                isNormalDropableSlot = false;
            }
        }
        if (isNormalDropableSlot) {
            UnitDropManager.Instance.ShowDropableSlot(cardData, true);
        }
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
        blockButton = PlayMangement.instance.player.drawCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if (PlayMangement.instance.player.getPlayerTurn == true && PlayMangement.instance.player.resource.Value >= cardData.cost)
            UnitDropManager.Instance.DropUnit(gameObject, CheckSlot());
        else {
            highlighted = false;
            UnitDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            UnitDropManager.Instance.HideDropableSlot();
            highlightedSlot = null;
        }

        UnitDropManager.Instance.HideDropableSlot();
    }

    public void CheckHighlight() {
        if (!highlighted) {
            highlightedSlot = CheckSlot();
            if (highlightedSlot != null) {
                highlighted = true;
                transform.Find("GlowEffect").GetComponent<Image>().color = new Color(163.0f / 255.0f, 236.0f / 255.0f, 27.0f / 255.0f);
                UnitDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            }
        }
        else {
            if (highlightedSlot != CheckSlot()) {
                highlighted = false;
                if (PlayMangement.instance.player.getPlayerTurn == true && PlayMangement.instance.player.resource.Value >= cardData.cost)
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 107.0f / 255.0f);
                else
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 1);
                UnitDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
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
            return;
        }
        if (!blockButton) {
            if (transform.parent.parent.name == "CardSlot_1") {
                csm.OpenCardList(transform.GetSiblingIndex());
            }
            else {
                csm.OpenCardList(GameObject.Find("CardSlot_1").transform.childCount + transform.parent.GetSiblingIndex());
            }
        }
    }

    public void RedrawCard() {
        if (cardID == null) {
            string id = "ac1000" + UnityEngine.Random.Range(1, 5);
            DrawCard(id);


            CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
            if (cardDataPackage.data.ContainsKey(id)) {
                CardData cardData = cardDataPackage.data[id];
                foreach (var skill in cardData.skills) {
                    foreach (var effect in skill.effects) {
                        gameObject.AddComponent(System.Type.GetType("SkillModules.Ability_" + effect.method));
                    }
                }
            }
        }
        else
            DrawCard(cardID);
        firstDraw = false;
        transform.Find("ChangeButton").gameObject.SetActive(false);
        csm = GameObject.Find("Canvas").transform.Find("CardInfoList").GetComponent<CardListManager>();
        csm.AddCardInfo(cardData);
    }

    public void RedrawButton() {
        
        DrawCard("ac1000" + UnityEngine.Random.Range(1, 5));
        transform.Find("ChangeButton").gameObject.SetActive(false);
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
