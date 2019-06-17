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

    public void SendSocket(UnityAction callbacks = null) {
        string[] args = null;
        string itemId = itemID.ToString();
        string line = string.Empty;
        if(highlightedSlot != null)
            line = highlightedSlot.parent.GetSiblingIndex().ToString();
        string unitItemId = string.Empty;
        string camp = cardData.camp;
        PlaceMonster mon = CheckUnit();
        if(mon != null) unitItemId = mon.itemId.ToString();
        switch(cardData.cardId) {
        //선택한 유닛에게 공격력 +3 / 체력 +3
        case "ac10006" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드 2장을 뽑음
        case "ac10007" : args = GetArgsInfo("all", itemId, line, unitItemId);  break;
        //내 유닛  해당 유닛의 공격력 +1
        case "ac10015" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //내 유닛 하나를 선택하여 즉시 1회 공격하게 함
        case "ac10016" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드 2장을 뽑음
        case "ac10017" : args = GetArgsInfo("all", itemId, line, unitItemId);  break;
        //선택한 라인의 모든 적에게 피해를 5 줌
        case "ac10021" : args = GetArgsInfo("line", itemId, line, unitItemId); break;
        //선택한 적 1명을 {{stun}}시키고, 내 덱에서 카드 1장을 뽑음
        case "ac10022" : args = GetArgsInfo("unit", itemId, line, unitItemId);  break;
        //무작위 적 유닛 1개를 상대 핸드로 되돌림
        case "ac10023" : args = GetArgsInfo("camp", itemId, line, unitItemId, camp.CompareTo("human") == 0? "orc" : "human"); break;
        //선택한 내 유닛 하나의 공격력+2/체력+2
        case "ac10024" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드를 2장 뽑음
        case "ac10025" : args = GetArgsInfo("all", itemId, line, unitItemId); break;
        //공격력이 5 이상인 적 유닛 1개를 처치함
        case "ac10026" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //배치된 모든 내 유닛이 {{poison}} 능력을 얻음
        case "ac10027" : args = GetArgsInfo("camp", itemId, line, unitItemId, camp); break;
        //내 유닛 1명을 즉시 1회 공격함
        case "ac10028" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        default : Debug.LogError("wrong magic Id"); return;
        }
        PlayMangement.instance.socketHandler.UseCard(args, callbacks);
    }



    private string[] GetArgsInfo(string status, string itemId, string line, string unitItemId, string camp = null) {
        switch(status) {
        case "all" : return new string[]{itemId, status};
        case "unit" : return new string[]{itemId, status, unitItemId};
        case "camp" : return new string[]{itemId, status, camp};
        case "line" :return new string[]{itemId, status, line};
        default : return null;
        }
    }

    PlaceMonster CheckUnit() {
        Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        origin = new Vector3(origin.x, origin.y, origin.z);
        Ray2D ray = new Ray2D(origin, Vector2.zero);

        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);

        foreach(RaycastHit2D hit in hits) {
            if(hit.transform.GetComponentInParent<PlaceMonster>() != null) {
                return hit.transform.GetComponentInParent<PlaceMonster>();
            }
        }
        return null;
    }
}