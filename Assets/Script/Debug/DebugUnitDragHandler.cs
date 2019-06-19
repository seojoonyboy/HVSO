using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;

public class DebugUnitDragHandler : DebugCardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {


    public void OnBeginDrag(PointerEventData eventData) {
        if (PlayMangement.instance.player.dragCard) return;
        DebugCardInfoOnDrag.instance.SetPreviewUnit(cardData.cardId);
        if (cardData.skills.Length != 0)
            DebugCardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition, cardData.skills[0].desc);
        else
            DebugCardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition);
        beforeDragParent = transform.parent;
        transform.SetParent(DebugManagement.Instance.cardDragCanvas);
        itsDragging = gameObject;
        blockButton = DebugManagement.Instance.player.dragCard = true;
        PlayMangement.instance.player.isPicking.Value = true;

        CardDropManager.Instance.ShowDropableSlot(cardData);

        object[] parms = new object[] { true, gameObject };
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
        if (gameObject != itsDragging) return;
        transform.SetParent(beforeDragParent);
        CheckLocation(true);
        iTween.MoveTo(gameObject, beforeDragParent.position, 0.3f);
        iTween.ScaleTo(gameObject, new Vector3(1, 1, 1), 0.3f);
        blockButton = DebugManagement.Instance.player.dragCard = false;
        DebugManagement.Instance.player.isPicking.Value = false;
        if (!isDropable) {
            highlighted = false;
            DebugCardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else {
            GameObject unitPref = DebugCardDropManager.Instance.DropUnit(gameObject, CheckSlot());
            if (unitPref != null) {
                if (unitPref.GetComponent<DebugUnit>().unit.name == "방패병") {
                    Debug.Log("방패병!!!!");
                    unitPref.AddComponent<TmpBuff>();
                }
                else {
                    foreach (dataModules.Skill skill in cardData.skills) {
                        foreach (var effect in skill.effects) {
                            var newComp = unitPref.AddComponent(Type.GetType("SkillModules.UnitAbility_" + effect.method));
                            if (newComp == null) {
                                Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                            }
                            else {
                                ((Ability)newComp).InitData(skill, true);
                            }
                        }
                    }
                }
                object[] parms = new object[] { true, unitPref };
            }
        }

        CardDropManager.Instance.HideDropableSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
    }

}
