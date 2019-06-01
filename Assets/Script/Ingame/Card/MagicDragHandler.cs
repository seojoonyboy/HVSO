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
        if (firstDraw || PlayMangement.instance.isMulligan) return;
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
        if(!isDropable && IsEnoughResource(cardData.cost)) {
            UserResource(cardData.cost);

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
        SendSocket();
        PlayMangement.instance.player.cdpm.DestroyCard(cardIndex);

        if (PlayMangement.instance.player.isHuman)
            PlayMangement.instance.player.ActivePlayer();
        else
            PlayMangement.instance.player.ActiveOrcSpecTurn();
    }

    private void SendSocket() {
        string[] args = null;
        string itemId = itemID.ToString();
        string line = selectedLine.parent.GetSiblingIndex().ToString();
        string unitItemId = string.Empty;
        string camp = cardData.camp;
        UnityEngine.Events.UnityAction drawCard = null;
        PlaceMonster mon = CheckUnit();
        if(mon != null) unitItemId = mon.itemId.ToString();

        switch(cardData.cardId) {
        //선택한 유닛에게 공격력 +3 / 체력 +3
        case "ac10006" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드 2장을 뽑음
        case "ac10007" : args = GetArgsInfo("all", itemId, line, unitItemId); drawCard = delegate {DrawNewCards(2);}; break;
        //내 유닛  해당 유닛의 공격력 +1
        case "ac10015" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //내 유닛 하나를 선택하여 즉시 1회 공격하게 함
        case "ac10016" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드 2장을 뽑음
        case "ac10017" : args = GetArgsInfo("all", itemId, line, unitItemId); drawCard = delegate {DrawNewCards(2);}; break;
        //선택한 라인의 모든 적에게 피해를 5 줌
        case "ac10021" : args = GetArgsInfo("line", itemId, line, unitItemId); break;
        //선택한 적 1명을 {{stun}}시키고, 내 덱에서 카드 1장을 뽑음
        case "ac10022" : args = GetArgsInfo("unit", itemId, line, unitItemId); drawCard = delegate {DrawNewCards(1);}; break;
        //무작위 적 유닛 1개를 상대 핸드로 되돌림
        case "ac10023" : args = GetArgsInfo("camp", itemId, line, unitItemId, camp.CompareTo("human") == 0? "orc" : "human") ; drawCard = delegate { ReturnUnitToCard(PlayMangement.instance.socketHandler.gameState);}; break;
        //선택한 내 유닛 하나의 공격력+2/체력+2
        case "ac10024" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //덱에서 카드를 2장 뽑음
        case "ac10025" : args = GetArgsInfo("all", itemId, line, unitItemId); drawCard = delegate {DrawNewCards(2);}; break;
        //공격력이 5 이상인 적 유닛 1개를 처치함
        case "ac10026" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        //배치된 모든 내 유닛이 {{poison}} 능력을 얻음
        case "ac10027" : args = GetArgsInfo("camp", itemId, line, unitItemId, camp); break;
        //내 유닛 1명을 즉시 1회 공격함
        case "ac10028" : args = GetArgsInfo("unit", itemId, line, unitItemId); break;
        default : Debug.LogError("wrong magic Id"); return;
        }
        PlayMangement.instance.socketHandler.UseCard(args, drawCard);
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

    private void DrawNewCards(int drawNum) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        SocketFormat.Card[] cards = playMangement.socketHandler.gameState.players.myPlayer(isHuman).deck.handCards;
        for(int i = cards.Length - drawNum; i < cards.Length; i++) {
            playMangement.player.cdpm.AddCard(null, cards[i]);
        }
    }

    private void ReturnUnitToCard(SocketFormat.GameState state) {
        PlayMangement playMangement = PlayMangement.instance;
        FieldUnitsObserver observer = playMangement.EnemyUnitsObserver;
        List<GameObject> enemyList = observer.GetAllFieldUnits();
        List<SocketFormat.Unit> socketList = state.map.allMonster;
        foreach(GameObject mon in enemyList) {
            bool found = false;
            PlaceMonster mondata = mon.GetComponent<PlaceMonster>();
            foreach(SocketFormat.Unit unit in socketList) {
                if(unit.itemId.CompareTo(mondata.itemId) == 0) {
                    found = true;
                    break;
                }
            }
            if(!found) {
                Pos pos = observer.GetMyPos(mon);
                observer.UnitRemoved(pos.row, pos.col);
                Destroy(mon);
                GameObject enemyCard = Instantiate(playMangement.enemyPlayer.back);
                enemyCard.transform.SetParent(playMangement.enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(PlayMangement.instance.CountEnemyCard() - 1).GetChild(0));
                enemyCard.transform.localScale = new Vector3(1, 1, 1);
                enemyCard.SetActive(true);
                break;
            }
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