using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
//using SkillModules;
using System.Linq;

public partial class UnitDragHandler : CardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public void OnBeginDrag(PointerEventData eventData) {
        if (firstDraw || PlayMangement.instance.isMulligan) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        startPos = transform.parent.position;
        PlayMangement.instance.player.isPicking.Value = true;

        CardDropManager.Instance.ShowDropableSlot(cardData);

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //transform.position = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, cardScreenPos.z);
        transform.position = Input.mousePosition;
        CheckHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        iTween.MoveTo(gameObject, startPos, 0.3f);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        if (!isDropable) {
            highlighted = false;
            CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        else {
            GameObject unitPref = CardDropManager.Instance.DropUnit(gameObject, CheckSlot());
            if (unitPref != null) {
                if (unitPref.GetComponent<PlaceMonster>().unit.name == "방패병") {
                    Debug.Log("방패병!!!!");
                    //unitPref.AddComponent<TmpBuff>();
                }
                else {  
                    foreach (dataModules.Skill skill in cardData.skills) {
                        foreach (var effect in skill.effects) {
                            var newComp = unitPref.AddComponent(Type.GetType("SkillModules.UnitAbility_" + effect.method));
                            if (newComp == null) {
                                Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                            }
                            else {
                                //((Ability)newComp).InitData(skill, true);
                            }
                        }
                    }
                    /*
                    if (unitPref.GetComponent<PlaceMonster>().unit.attackType.ToList().Contains("assault")) {
                        unitPref.AddComponent<UnitAbility_assault>();
                    }
                    */
                    /*
                    //잠복
                    if (unitPref.GetComponent<PlaceMonster>().unit.cardCategories.ToList().Contains("stealth")) {
                        unitPref.AddComponent<ambush>();
                    }
                    */
                }

                object[] parms = new object[] { true, unitPref };
                PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
            }
        }

        CardDropManager.Instance.HideDropableSlot();
    }
}
