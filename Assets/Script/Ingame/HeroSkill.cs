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

        switch (heroId) {
            case "h10002":
                BonusMana(targetPlayer);
                break;
            case "h10003":
                DiscountCardMana(targetPlayer);
                break;
            case "h10004":
                BonusMana(targetPlayer);
                break;
            default:
                Debug.Log("딱히 여기서 처리할건 아닌듯...");
                break;
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