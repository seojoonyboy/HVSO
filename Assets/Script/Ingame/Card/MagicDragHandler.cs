using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;
using System.Linq;
using System.Text;
using UnityEngine.Events;

public partial class MagicDragHandler : CardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public bool isPlayer;
    public SkillHandler skillHandler;

    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw || PlayMangement.instance.isMulligan) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        if (cardData.skills.Length != 0)
            CardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition, cardData.skills[0].desc);
        beforeDragParent = transform.parent;
        transform.SetParent(PlayMangement.instance.cardDragCanvas);
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        PlayMangement.instance.player.isPicking.Value = true;

        //TODO : Filter를 통해(Use Condition) 타겟 표시 추가 제어
        CardDropManager.Instance.ShowMagicalSlot(cardData.skills[0].target.args);

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cardScreenPos = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, 0);
        transform.position = cardScreenPos;
        if (cardData.skills.Length != 0)
            CardInfoOnDrag.instance.SetInfoPosOnDrag(transform.localPosition);

        //TODO : Filter를 통해(Use Condition) 놓을 수 있는 영역 추가 제어
        CheckMagicHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        transform.SetParent(beforeDragParent);
        iTween.MoveTo(gameObject, beforeDragParent.position, 0.3f);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if (!isDropable) {
            highlighted = false;
            CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else {
            if (CheckMagicSlot() != null) {
                //var abilities = GetComponents<MagicalCasting>();
                //foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();

                object[] parms = new object[] { true, gameObject };
                PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);

                //if (GetComponents<Ability>() == null) UseCard();
            }
        }
        CardDropManager.Instance.HideMagicSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
    }

    private void OnDestroy() {
        skillHandler.RemoveTriggerEvent();    
    }
}