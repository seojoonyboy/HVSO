using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class Ability_supply : Ability {
        protected override void OnEventCallback(object parm) {
            var effectData = skillData.effects.ToList().Find(x => x.method == "supply");
            int drawNum = 0;

            int.TryParse(effectData.args[0], out drawNum);
            CardHandler cardHandler = gameObject.GetComponent<CardHandler>();
            PlayMangement playMangement =PlayMangement.instance;
            string itemId = cardHandler.itemID.ToString();
            string[] args = {itemId, "all"};
            playMangement.socketHandler.UseCard(args, delegate {
                bool isHuman = playMangement.player.isHuman;
                SocketFormat.Card[] cards = playMangement.socketHandler.gameState.players.myPlayer(isHuman).deck.handCards;
                for(int i = cards.Length - 1 - drawNum; i < cards.Length; i++) {
                    playMangement.player.cdpm.AddCard(null, cards[i]);
                }
            });
        }
    }
}