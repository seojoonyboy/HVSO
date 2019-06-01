using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;

public partial class MagicDragHandler : CardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Transform selectedLine;

    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        startPos = transform.parent.position;
        PlayMangement.instance.player.isPicking.Value = true;

        //CardDropManager.Instance.ShowDropableSlot(cardData);
        CardDropManager.Instance.BeginCheckLines();

        var abilities = GetComponents<Ability>();
        foreach (Ability ability in abilities) ability.OnBeginDrag();

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, cardScreenPos.z);
        //CheckHighlight();

        CheckLineHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        iTween.MoveTo(gameObject, startPos, 0.3f);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if(!isDropable) {
            highlighted = false;
            CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else {
            var abilities = GetComponents<Ability>();
            foreach (Ability ability in abilities) ability.OnEndDrag();

            selectedLine = highlightedLine;

            object[] parms = new object[] { true, gameObject };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);

            if (GetComponents<Ability>() == null) UseCard();
        }
        CardDropManager.Instance.HideDropableSlot();
        OffLineHighlight();
    }

    public void AttributeUsed(MonoBehaviour behaviour) {
        DestroyImmediate(behaviour);
        Debug.Log(behaviour);
        if (GetComponent<Ability>() == null) {
            Debug.Log("카드 사용 끝남");
            UseCard();
        }
    }

    public void UseCard() {
        int cardIndex = 0;
        if (transform.parent.parent.name == "CardSlot_1")
            cardIndex = transform.parent.GetSiblingIndex();
        else {
            Transform slot1 = transform.parent.parent.parent.GetChild(0);
            for (int i = 0; i < 5; i++) {
                if (slot1.GetChild(i).gameObject.activeSelf)
                    cardIndex++;
            }
            cardIndex += transform.parent.GetSiblingIndex();
        }

        PlayMangement.instance.player.isPicking.Value = false;
        PlayMangement.instance.player.resource.Value -= cardData.cost;

        PlayMangement.instance.player.cdpm.DestroyCard(cardIndex);
    }
}