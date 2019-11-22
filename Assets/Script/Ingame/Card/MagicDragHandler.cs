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
            if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial == true) { 
                if(gameObject.transform.Find("drag") != null) 
                    gameObject.transform.Find("drag").gameObject.SetActive(false);
            }
            ShowCardsHandler showCardsHandler = GetComponentInParent<ShowCardsHandler>();
            showCardsHandler.Selecting(gameObject);

            heroCardInfo.SetActive(false);
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
            EffectSystem.Instance.TilledField();
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, this, parms1);
            return;
        }
        if (!PlayMangement.dragable) return;
        if (firstDraw || PlayMangement.instance.isMulligan) return;
        if (Input.touchCount > 1) return;
        if (PlayMangement.instance.player.dragCard) return;
        if (ScenarioGameManagment.scenarioInstance != null) {
            ScenarioMask.Instance.OffDeckCardGlow();
            ScenarioMask.Instance.StopEveryHighlight();
        }
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

    public void ForceToHandHeroCards() {
        ShowCardsHandler showCardsHandler = GetComponentInParent<ShowCardsHandler>();
        if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial && ScenarioGameManagment.scenarioInstance.canHeroCardToHand == false) {
            cardUsed = false;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localPosition = new Vector3(0, 0, 0);
            transform.Find("CardInfoWindow").gameObject.SetActive(false);
            transform.parent.parent.Find("HeroCardGuide").gameObject.SetActive(true);
            showCardsHandler.CancelSelecting();
        }
        else {
            showCardsHandler.FinishPlay(gameObject, true);
            handManager.AddHeroCard(gameObject);
            heroCardActivate = false;
        }
    }


    public void OnEndDrag(PointerEventData eventData) {
        EffectSystem.Instance.UnTillField();
        if (heroCardActivate) {
            ShowCardsHandler showCardsHandler = GetComponentInParent<ShowCardsHandler>();
            bool pass = false;
            heroCardInfo.SetActive(true);
            //영웅 카드를 핸드로 가져오는 부분
            if (transform.position.y < -3.5f) {
                if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.canHeroCardToHand == false) {
                    pass = true;
                    BackToDeckCard();
                    //if (heroCardActivate) {
                    //    transform.parent.parent.Find("HeroCardGuide").gameObject.SetActive(true);
                    //}
                    showCardsHandler.CancelSelecting();
                    //gameObject.transform.Find("drag").gameObject.SetActive(true);
                    SendEvent();
                }
                else {
                    if (ScenarioGameManagment.scenarioInstance == null) {
                        IngameTimer timer = isPlayer ? PlayMangement.instance.player.GetComponent<IngameTimer>() : PlayMangement.instance.enemyPlayer.GetComponent<IngameTimer>();
                        timer.OnTimeout.RemoveListener(PlayMangement.instance.showCardsHandler.TimeoutShowCards);
                        timer.EndTimer();
                    }

                    ForceToHandHeroCards();
                }
                    
            }
            else {
                CheckLocation(true);
                cardUsed = false;
                //영웅 실드 발동시 나온 카드를 사용 할 때만 여기로 들어옴
                if (CheckMagicSlot() != null) {
                    cardUsed = true;
                    showCardsHandler
                        .GetOppositeCard(gameObject)
                        .transform
                        .localPosition = new Vector3(4000f, 0);
                    //var abilities = GetComponents<MagicalCasting>();
                    //foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();
                    object[] parms = new object[] { true, gameObject };
                    transform.Find("GlowEffect").gameObject.SetActive(false);
                    transform.Find("CardInfoWindow").gameObject.SetActive(false);
                    showCardsHandler.hideShowBtn.SetActive(false);
                    skillHandler.highlight = highlightedSlot; 
                    if(!skillHandler.TargetSelectExist()) skillHandler.SendSocket();
                    StartCoroutine(UseSkillCard(parms));

                    if (ScenarioGameManagment.scenarioInstance == null) {
                        IngameTimer timer = isPlayer ? PlayMangement.instance.player.GetComponent<IngameTimer>() : PlayMangement.instance.enemyPlayer.GetComponent<IngameTimer>();
                        timer.OnTimeout.RemoveListener(PlayMangement.instance.showCardsHandler.TimeoutShowCards);
                        timer.EndTimer();
                    }
                }
                else {
                    highlighted = false;
                    CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
                    highlightedSlot = null;
                }
                if (!cardUsed) {
                    BackToDeckCard();


                    Invoke("SendEvent", 0.3f);
                    showCardsHandler.CancelSelecting();
                    if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial == true) { 
                        if(gameObject.transform.Find("drag") != null) 
                            gameObject.transform.Find("drag").gameObject.SetActive(true);
                    }
                }
            }
            CardDropManager.Instance.HideMagicSlot();
            CardInfoOnDrag.instance.OffCardDragInfo();
            if (pass == false)
                PlayMangement.instance.player.ConsumeShieldStack();
            return;
        }
        if (firstDraw) return;
        if (gameObject != itsDragging) return;
        CheckLocation(true);
        blockButton = PlayMangement.instance.player.dragCard = false;
        PlayMangement.instance.player.isPicking.Value = false;
        cardUsed = false;

        if (CheckMagicSlot() != null && PlayMangement.instance.player.resource.Value >= cardData.cost && turnMachine.isPlayerTurn()) {
            cardUsed = true;
            //var abilities = GetComponents<MagicalCasting>();
            //foreach (MagicalCasting ability in abilities) ability.RequestUseMagic();
            transform.Find("GlowEffect").gameObject.SetActive(false);
            PlayMangement.instance.player.resource.Value -= cardData.cost;
            object[] parms = new object[] { true, gameObject };
            skillHandler.highlight = highlightedSlot;
            if(!skillHandler.TargetSelectExist()) skillHandler.SendSocket();
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
            BackToDeckCard();

            if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial == true)
                SendEvent();

        }
        CardDropManager.Instance.HideMagicSlot();
        CardInfoOnDrag.instance.OffCardDragInfo();
        PlayMangement.instance.infoOn = false;
    }

    void SendEvent() {
        //튜토리얼에서 drop 이 실패하여 다시 핸드로 돌아온 경우 튜토리얼 재호출 처리
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, this);
    }

    IEnumerator UseSkillCardExceptInfo(object[] parms) {
        skillHandler.socketDone = false;
        PlayMangement.instance.LockTurnOver();
        yield return EffectSystem.Instance.HeroCutScene(PlayMangement.instance.player.isHuman);
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        highlighted = false;
        CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
        highlightedSlot = null;
        skillHandler.RemoveTriggerEvent();
    }


    IEnumerator UseSkillCard(object[] parms) {
        skillHandler.socketDone = false;
        PlayMangement.dragable = false;
        PlayMangement.instance.LockTurnOver();
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.MAGIC_USED, this, cardData.id);
        yield return PlayMangement.instance.cardHandManager.ShowUsedCard(transform.parent.GetSiblingIndex(), gameObject);
        if (cardData.isHeroCard == true) {
            HideCardImage();
            yield return EffectSystem.Instance.HeroCutScene(PlayMangement.instance.player.isHuman);            
        }
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        SoundManager.Instance.PlayMagicSound(cardData.id);
        highlighted = false;
        CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);       
        highlightedSlot = null;        
        skillHandler.RemoveTriggerEvent();
        ShowCardsHandler showCardsHandler = transform.root.GetComponentInChildren<ShowCardsHandler>();
        showCardsHandler.FinishPlay(gameObject);
        //GetComponentInParent<ShowCardsHandler>().RemoveCard(gameObject);
    }

    public override void OnTurnChanged(Enum Event_Type, Component Sender, object Param) {
        if (!isMyTurn) {
            //영웅 마법 카드 선택단계인경우 둘중 무작위 한장 선택하여 핸드로 가져오기
            if (heroCardActivate) {
                OnEndDrag(null);

                PlayMangement.instance.showCardsHandler.TimeoutShowCards();
            }
        }
    }

    private void HideCardImage() {
        transform.Find("GlowEffect").gameObject.SetActive(false);
        transform.Find("Portrait").gameObject.SetActive(false);
        transform.Find("BackGround").gameObject.SetActive(false);
        transform.Find("Cost").gameObject.SetActive(false);
    }

}