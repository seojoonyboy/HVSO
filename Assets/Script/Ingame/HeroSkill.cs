using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SocketFormat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class HeroSkill {
    //public delegate void HeroEffect(PlayerController targetPlayer);

    public void Activate(bool isPlayer, string heroId, List<JToken> toList, string trigger, DequeueCallback callback) {
        PlayerController targetPlayer = (isPlayer == true) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;

        MethodInfo theMethod = this.GetType().GetMethod(heroId);
        object[] parameter = new object[] { targetPlayer, toList, trigger, callback };
        theMethod.Invoke(this, parameter);
    }


    public void h10001(PlayerController targetPlayer, List<JToken> toList, string trigger, DequeueCallback callback) {
        //2티어 : start_play, 체력 3 증가
        //3티어 : start_play, 실드 개수 증가
        //어차피 start_play 검사 안해도 동일하므로 if 생략

        //TODO : 체력 업데이트 작업

        callback();
    }

    public void h10002(PlayerController targetPlayer, List<JToken> toList, string trigger, DequeueCallback callback) {
        //2티어 : start_play, 실드 게이지 최소 개수 증가
        //3티어 : orc_post_turn, 마나 1개 증가
        if(trigger.CompareTo("orc_post_turn")==0) {
            BonusMana(targetPlayer);
        }
        else if(trigger.CompareTo("start_play")==0) {
            //작업 없음.
        }
        callback();
    }

    public void h10003(PlayerController targetPlayer, List<JToken> toList, string trigger, DequeueCallback callback) {
        //2티어 : start_play, 실드 게이지 최소 개수 증가
        //3티어 : after_card_play
        // 2번 오는데 경우의 수가 다름
        // 마법카드 2장 사용 시, 유닛 카드 비용 -1
        //그 이후 유닛 카드 사용 시 그 외 유닛 카드 비용 원상복구
        if(trigger.CompareTo("after_card_play")==0) {
            //TODO : 카드 전면 비용 체크 및 갱신 하기
            DiscountCardMana(targetPlayer);
        }
        else if(trigger.CompareTo("start_play")==0) {
            //작업 없음.
        }
        callback();
    }

    public void h10004(PlayerController targetPlayer, List<JToken> toList, string trigger, DequeueCallback callback) {
        //2티어 : start_play, 실드 게이지 최대 개수 증가
        //3티어 : after_card_play, tool 카드 사용 시 마나 1개 증가
        if(trigger.CompareTo("after_card_play")==0) {
            BonusMana(targetPlayer);
        }
        else if(trigger.CompareTo("start_play")==0) {
            //작업 없음.
        }
        callback();
    }

    protected void BonusMana(PlayerController targetPlayer) {
        bool isHuman = targetPlayer.isHuman;
        targetPlayer.resource.Value = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).resource;
    }

    protected void DiscountCardMana(PlayerController targetPlayer) {
        bool isHuman = targetPlayer.isHuman;       

        if (targetPlayer.isHuman)
            targetPlayer.ActivePlayer(1);
        else
            targetPlayer.ActiveOrcTurn(1);
    }


}