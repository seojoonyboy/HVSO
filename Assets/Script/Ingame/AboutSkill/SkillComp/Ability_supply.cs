using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class Ability_supply : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;

            bool isPlayer = (bool)parms[0];
            GameObject card = (GameObject)parms[1];

            if (card != gameObject) return;

            var effectData = skillData.effects.ToList().Find(x => x.method == "supply");
            int drawNum = 0;

            int.TryParse(effectData.args[0], out drawNum);
            CardHandler cardHandler = gameObject.GetComponent<CardHandler>();
            PlayMangement playMangement = PlayMangement.instance;
            string itemId = cardHandler.itemID.ToString();
            
            string[] args = {itemId, "all"};
            playMangement.socketHandler.UseCard(args, delegate {
                bool isHuman = playMangement.player.isHuman;
                SocketFormat.Card[] cards = playMangement.socketHandler.gameState.players.myPlayer(isHuman).deck.handCards;
                for(int i = cards.Length - drawNum; i < cards.Length; i++) {
                    playMangement.player.cdpm.AddCard(null, cards[i]);
                }
            });
            GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_supply>());
        }
    }
}