using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;
using System.Linq;

public partial class UnitDragHandler : CardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public void OnBeginDrag(PointerEventData eventData) {
        if (!PlayMangement.dragable) return;
        if (firstDraw || PlayMangement.instance.isMulligan) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        CardInfoOnDrag.instance.SetPreviewUnit(cardData.cardId);
        if (cardData.skills.Length != 0)
            CardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition, cardData.skills[0].desc);
        else
            CardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition);
        beforeDragParent = transform.parent;
        transform.SetParent(PlayMangement.instance.cardDragCanvas);
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        PlayMangement.instance.player.isPicking.Value = true;

        CardDropManager.Instance.ShowDropableSlot(cardData);

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (gameObject != itsDragging) return;
        if (!PlayMangement.dragable) {
            OnEndDrag(null);
            return;
        }
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cardScreenPos = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, 0);
        transform.position = cardScreenPos;
        CheckLocation();
        CardInfoOnDrag.instance.SetInfoPosOnDrag(transform.localPosition);
        CheckHighlight();
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        transform.SetParent(beforeDragParent);
        CheckLocation(true);
        iTween.MoveTo(gameObject, beforeDragParent.position, 0.3f);
        iTween.ScaleTo(gameObject, new Vector3(1, 1, 1), 0.3f);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if (!isDropable) {
            highlighted = false;
            CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else if(isMyTurn()) {
            GameObject unitPref = CardDropManager.Instance.DropUnit(gameObject, CheckSlot());
            if (unitPref != null) {
                var cardData = GetComponent<CardHandler>().cardData;

                SkillModules.SkillHandler skillHandler = new SkillModules.SkillHandler();
                skillHandler.Initialize(cardData.skills, unitPref, true);
                unitPref.GetComponent<PlaceMonster>().skillHandler = skillHandler;

                object[] parms = new object[] { true, unitPref };
                PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
                PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
            }
        }

        CardDropManager.Instance.HideDropableSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
    }
}
