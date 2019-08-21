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
        StartDragCard();
        CardInfoOnDrag.instance.SetPreviewUnit(cardData.cardId);
        if (cardData.skills.Length != 0)
            CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition, cardData.skills[0].desc);
        else
            CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition);
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        PlayMangement.instance.player.isPicking.Value = true;
        CardDropManager.Instance.ShowDropableSlot(cardData);
        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (!PlayMangement.dragable) {
            OnEndDrag(null);
            return;
        }
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (gameObject != itsDragging) return;
        OnDragCard();
        CheckLocation();
        CardInfoOnDrag.instance.SetInfoPosOnDrag(mouseLocalPos.localPosition, true);
        CheckHighlight();
    }

    public void OnEndDrag(PointerEventData eventData) {
        EffectSystem.Instance.IncreaseFadeAlpha();
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        CheckLocation(true);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        cardUsed = false;
        if (!isDropable) {
            highlighted = false;
            CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else if(isMyTurn(false)) {
            Transform slot = CheckSlot();
            if (slot != null && slot.childCount <= 1)
                StartCoroutine(SummonUnit(slot));
        }
        handManager.transform.SetParent(mouseXPos.parent);
        if (!cardUsed) {
            transform.localScale = new Vector3(1, 1, 1);
            transform.localPosition = new Vector3(0, 0, 0);
            StartCoroutine(handManager.SortHandPosition());
        }
        CardDropManager.Instance.HideDropableSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
    }

    IEnumerator SummonUnit(Transform slot) {
        PlayMangement.dragable = false;

        //yield return PlayMangement.instance.cardHandManager.ShowUsedCard(transform.parent.GetSiblingIndex(), gameObject);
        GameObject unitPref = CardDropManager.Instance.DropUnit(gameObject, slot);
        if (unitPref != null) {
            var cardData = GetComponent<CardHandler>().cardData;

            SkillModules.SkillHandler skillHandler = new SkillModules.SkillHandler();
            skillHandler.Initialize(cardData.skills, unitPref, true);
            unitPref.GetComponent<PlaceMonster>().skillHandler = skillHandler;

            object[] parms = new object[] { true, unitPref };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
        }
        PlayMangement.dragable = true;
        yield return 0;
    }
}
