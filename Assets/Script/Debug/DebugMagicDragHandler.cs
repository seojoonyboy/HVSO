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

public class DebugMagicDragHandler : DebugCardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public void OnBeginDrag(PointerEventData eventData) {
        if (Input.touchCount > 1) return;
        if (DebugManagement.instance.player.dragCard) return;
        if (cardData.skills.Length != 0)
            DebugCardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition, cardData.skills[0].desc);
        else
            DebugCardInfoOnDrag.instance.SetCardDragInfo(null, transform.localPosition);
        //beforeDragParent = transform.parent;
        //transform.SetParent(DebugManagement.instance.cardDragCanvas);
        itsDragging = gameObject;
        blockButton = DebugManagement.instance.player.dragCard = true;
        DebugManagement.instance.player.isPicking.Value = true;
        string target = cardData.skills[0].target.args[0];

        List<string> targetArgs = cardData.skills[0].target.args.ToList();

        if (isOnlySupplyCard()) {
            DebugCardDropManager.Instance.ShowMagicalSlot("all");
        }
        else {
            if (isBlast_EnemyExist()) {
                int standardNum = GetBlastStandardNum();
                Debug.Log("공격력이 " + standardNum + "이상인 유닛만 드롭 가능한 영역으로 지정합니다.");
                DebugCardDropManager.Instance.ShowMagicalSlot(target, standardNum);
            }
            else {
                if (targetArgs.Count == 1) {
                    DebugCardDropManager.Instance.ShowMagicalSlot(target);
                }
                else {
                    //my & all case
                    StringBuilder sb = new StringBuilder();
                    foreach (var targetArg in targetArgs) {
                        sb.Append(targetArg);
                    }
                    Debug.Log(sb.ToString() + "Target");
                    DebugCardDropManager.Instance.ShowMagicalSlot(sb.ToString());
                }
            }

        }

        //CardDropManager.Instance.BeginCheckLines();

        object[] parms = new object[] { true, gameObject };
        DebugManagement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (gameObject != itsDragging) return;
        if (!DebugManagement.dragable) {
            OnEndDrag(null);
            return;
        }
        Vector3 cardScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cardScreenPos = new Vector3(cardScreenPos.x, cardScreenPos.y + 0.3f, 0);
        transform.position = cardScreenPos;
        CheckLocation();
        DebugCardInfoOnDrag.instance.SetInfoPosOnDrag(transform.localPosition);
        CheckMagicHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        // if (firstDraw) return;
        // if (gameObject != itsDragging) return;
        // transform.SetParent(beforeDragParent);
        // CheckLocation(true);
        // iTween.MoveTo(gameObject, beforeDragParent.position, 0.3f);
        // iTween.ScaleTo(gameObject, new Vector3(1, 1, 1), 0.3f);
        // blockButton = DebugManagement.instance.player.dragCard = false;
        // DebugManagement.instance.player.isPicking.Value = false;
        // if (!isDropable) {
        //     highlighted = false;
        //     DebugCardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
        //     highlightedSlot = null;
        // }
        // else {
        //     if (CheckMagicSlot() != null) {
        //         var abilities = GetComponents<MagicalCasting>();
        //         foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();

        //         object[] parms = new object[] { true, gameObject };
        //         DebugManagement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);

        //         if (GetComponents<DebugAbility>() == null) UseCard();
        //     }
        // }
        // DebugCardDropManager.Instance.HideMagicSlot();
        // DebugCardInfoOnDrag.instance.OffCardDragInfo();
    }

    public void AttributeUsed() {
        // bool isValid = true;
        // MagicalCasting[] magicalCasts = GetComponents<MagicalCasting>();
        // if (magicalCasts.Length == 0) return;
        // foreach (MagicalCasting magicalCast in magicalCasts) {
        //     isValid = isValid && magicalCast.isRequested;
        // }

        // if (isValid) UseCard();
    }

    public void UseCard() {
        DebugCardDropManager.Instance.HighLightMagicSlot(highlightedSlot, false);
        DebugCardDropManager.Instance.HideMagicSlot();
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

        DebugManagement.instance.player.isPicking.Value = false;
        DebugManagement.instance.player.resource.Value -= cardData.cost;
        DebugManagement.instance.player.cdpm.DestroyCard(cardIndex);
        
    }

    private UnityAction CreateEventList() {
        // UnityAction useMagic = null;
        // MagicalCasting[] magicalCasts = GetComponents<MagicalCasting>();
        // foreach (MagicalCasting magicalCast in magicalCasts) {
        //     useMagic += magicalCast.UseMagic;
        // }
        // return useMagic;
        return null;
    }

    private bool isOnlySupplyCard() {
        // List<MagicalCasting> abilities = GetComponents<MagicalCasting>().ToList();
        // if (abilities.Count == 1) {
        //     if (abilities[0].GetType() == typeof(MagicalCasting_supply)) {
        //         return true;
        //     }
        //     return false;
        // }
        // else return false;
        return false;
    }

    private bool isBlast_EnemyExist() {
        // List<MagicalCasting> abilities = GetComponents<MagicalCasting>().ToList();
        // foreach (MagicalCasting ability in abilities) {
        //     if (ability.GetType() == typeof(MagicalCasting_over_a_kill)) {
        //         return true;
        //     }
        // }
        return false;
    }

    private int GetBlastStandardNum() {
        // int num = 0;
        // int.TryParse(cardData.skills[0].activate.conditions[0].args[0], out num);
        // return num;
        return 0;
    }

    private string[] GetArgsInfo(string status, string itemId, string line, string unitItemId, string camp = null) {
        switch (status) {
            case "all": return new string[] { itemId, status };
            case "unit": return new string[] { itemId, status, unitItemId };
            case "camp": return new string[] { itemId, status, camp };
            case "line": return new string[] { itemId, status, line };
            default: return null;
        }
    }

    DebugUnit CheckUnit() {
        Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        origin = new Vector3(origin.x, origin.y, origin.z);
        Ray2D ray = new Ray2D(origin, Vector2.zero);

        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);

        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.GetComponentInParent<DebugUnit>() != null) {
                return hit.transform.GetComponentInParent<DebugUnit>();
            }
        }
        return null;
    }
}


