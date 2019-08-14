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
        if (heroCardActivate) {
            heroCardInfo.SetActive(false);
            transform.parent.Find("HeroCardGuide").gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            if (cardData.skills.Length != 0)
                CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition, cardData.skills[0].desc);
            else
                CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition);
            //TODO : Filter를 통해(Use Condition) 타겟 표시 추가 제어
            try {
                if (skillHandler.skills[0].conditionCheckers[0] != null) {
                    Logger.Log("ConditionChecker [0] 존재 : " + skillHandler.skills[0].conditionCheckers[0]);
                    CardDropManager.Instance.ShowMagicalSlot(cardData.skills[0].target.args, skillHandler.dragFiltering, skillHandler.skills[0].conditionCheckers[0]);
                }
            }
            catch (Exception ex) {
                CardDropManager.Instance.ShowMagicalSlot(cardData.skills[0].target.args, skillHandler.dragFiltering);
            }

            object[] parms1 = new object[] { true, gameObject };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms1);
            return;
        }
        if (!PlayMangement.dragable) return;
        if (firstDraw || PlayMangement.instance.isMulligan) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        StartDragCard();
        if (cardData.skills.Length != 0)
            CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition, cardData.skills[0].desc);
        else
            CardInfoOnDrag.instance.SetCardDragInfo(null, mouseLocalPos.localPosition);
        itsDragging = gameObject;
        blockButton = PlayMangement.instance.player.dragCard = true;
        PlayMangement.instance.player.isPicking.Value = true;
        //TODO : Filter를 통해(Use Condition) 타겟 표시 추가 제어
        try {
            if(skillHandler.skills[0].conditionCheckers[0] != null) {
                Logger.Log("ConditionChecker [0] 존재 : " + skillHandler.skills[0].conditionCheckers[0]);
                CardDropManager.Instance.ShowMagicalSlot(cardData.skills[0].target.args, skillHandler.dragFiltering, skillHandler.skills[0].conditionCheckers[0]);
            }
        }
        catch(Exception ex) {
            CardDropManager.Instance.ShowMagicalSlot(cardData.skills[0].target.args, skillHandler.dragFiltering);
        }

        object[] parms = new object[] { true, gameObject };
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms);
    }

    public void OnDrag(PointerEventData eventData) {
        if (heroCardActivate) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0);
            transform.position = mousePos;
            mouseLocalPos.position = transform.position;
            Debug.Log(transform.position.y);
            CheckLocation();
            CardInfoOnDrag.instance.SetInfoPosOnDrag(mouseLocalPos.localPosition);
            CheckMagicHighlight();
            return;
        }
        if (!PlayMangement.dragable) {
            eventData.pointerDrag = null;
            eventData.dragging = false;
            OnEndDrag(null);
            return;
        }


        if (firstDraw) return;
        if (Input.touchCount > 1) return;
        if (gameObject != itsDragging) return;
        OnDragCard();
        CheckLocation();
        CardInfoOnDrag.instance.SetInfoPosOnDrag(mouseLocalPos.localPosition);
        CheckMagicHighlight();
    }


    public void OnEndDrag(PointerEventData eventData) {
        EffectSystem.Instance.IncreaseFadeAlpha();
        if (heroCardActivate) {
            heroCardInfo.SetActive(true);
            if (transform.position.y < -3.5f) {
                handManager.AddHeroCard(gameObject);
                heroCardActivate = false;
            }
            else {
                CheckLocation(true);
                cardUsed = false;
                //영웅 실드 발동시 나온 카드를 사용 할 때만 여기로 들어옴
                if (CheckMagicSlot() != null) {
                    cardUsed = true;
                    //var abilities = GetComponents<MagicalCasting>();
                    //foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();
                    object[] parms = new object[] { true, gameObject };
                    skillHandler.socketDone = false;
                    StartCoroutine(EffectSystem.Instance.HeroCutScene(PlayMangement.instance.player.isHuman));                    
                    PlayMangement.instance.LockTurnOver();
                    PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
                    skillHandler.RemoveTriggerEvent();
                    //if (GetComponents<Ability>() == null) UseCard();
                }
                highlighted = false;
                CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
                highlightedSlot = null;
                if (!cardUsed) {
                    transform.localScale = new Vector3(1, 1, 1);
                    transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            if(!cardUsed && heroCardActivate)
                transform.parent.Find("HeroCardGuide").gameObject.SetActive(true);
            CardDropManager.Instance.HideMagicSlot();
            CardInfoOnDrag.instance.OffCardDragInfo();
            return;
        }
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        CheckLocation(true);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        cardUsed = false;

        if (CheckMagicSlot() != null && PlayMangement.instance.player.resource.Value >= cardData.cost && isMyTurn(true)) {
            cardUsed = true;
            //var abilities = GetComponents<MagicalCasting>();
            //foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();
            PlayMangement.instance.player.resource.Value -= cardData.cost;
            object[] parms = new object[] { true, gameObject };
            StartCoroutine(UseSkillCard(parms));
            //if (GetComponents<Ability>() == null) UseCard();
        }
        else {
            highlighted = false;
            CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
            highlightedSlot = null;
        }
        handManager.transform.SetParent(mouseXPos.parent);
        if (!cardUsed) {
            transform.localScale = new Vector3(1, 1, 1);
            transform.localPosition = new Vector3(0, 0, 0);
            StartCoroutine(handManager.SortHandPosition());
        }
        CardDropManager.Instance.HideMagicSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
        PlayMangement.instance.infoOn = false;
    }

    IEnumerator UseSkillCard(object[] parms) {
        skillHandler.socketDone = false;
        PlayMangement.dragable = false;
        PlayMangement.instance.LockTurnOver();
        if (cardData.hero_chk == true)
            yield return EffectSystem.Instance.HeroCutScene(PlayMangement.instance.player.isHuman);
        yield return PlayMangement.instance.cardHandManager.ShowUsedCard(transform.parent.GetSiblingIndex(), gameObject);
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        highlighted = false;
        CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
        highlightedSlot = null;        
        skillHandler.RemoveTriggerEvent();
    }
}