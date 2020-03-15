using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SocketFormat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ActiveCard {

    public class CardPlayArgs {
        
    }

    public delegate void SkillAction();
    public SkillAction skillAction;

    public void Activate(string cardId, object args, DequeueCallback callback) {
        MethodInfo theMethod = this.GetType().GetMethod(cardId);
        object[] parameter = new object[]{args, callback};
        if(theMethod == null) {
            Logger.Log(cardId + "해당 카드는 아직 준비가 안되어있습니다.");
            callback();
            return;
        }
        theMethod.Invoke(this, parameter);
    }

    async void AfterAction(float time = 0f ,DequeueCallback callback = null) {
        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(time));
        skillAction?.Invoke();
        skillAction = null;
        callback();
    }


    //축복
    public void ac10006(object args, DequeueCallback callback) {
        JObject jObject = args as JObject;
        string itemId = jObject["targets"][0]["args"][0].ToString();
        PlayMangement.instance.UnitsObserver.GetUnitToItemID(itemId).GetComponent<PlaceMonster>().UpdateGranted();
        callback();
    }

    //긴급 보급
    public void ac10007(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string[] itemIds = dataModules.JsonReader.Read<string[]>(magicArgs.skillInfo.ToString());
        bool isHuman = magicArgs.targets[0].args[0] == "human";
        PlayerController player = PlayMangement.instance.player;
        BattleConnector socket = PlayMangement.instance.SocketHandler;

        if (player.isHuman != isHuman) {
            callback();
            return;
        }
        else {
            socket.DrawNewCards(itemIds);
        }
        callback();
    }

    //재배치
    public void ac10015(object args, DequeueCallback callback) {
        JObject jObject = args as JObject;
        string itemId = jObject["targets"][0]["args"][0].ToString();
        GameObject monster = PlayMangement.instance.UnitsObserver.GetUnitToItemID(itemId);
        Unit unit = PlayMangement.instance.socketHandler.gameState.map.allMonster.Find(x => string.Compare(x.itemId, itemId, StringComparison.Ordinal) == 0);
        PlayMangement.instance.UnitsObserver.UnitChangePosition(monster, unit.pos, monster.GetComponent<PlaceMonster>().isPlayer, string.Empty, () => callback());
    }

    //피의 분노
    public void ac10016(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        JObject jObject = JObject.FromObject(magicArgs.skillInfo);

        AttackInfo info = jObject.ToObject<AttackInfo>();
        FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
        PlaceMonster attacker = observer.GetUnitToItemID(info.attacker).GetComponent<PlaceMonster>();
        attacker.instanceAttack = true;
        List<GameObject> affected = observer.GetAfftecdList(attacker.unit.ishuman, info.affected);
        skillAction = delegate () { attacker.GetTarget(affected); };
        AfterAction(attacker.totalAtkTime, callback);
    }

    //전쟁의 외침
    public void ac10017(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string[] itemIds =  dataModules.JsonReader.Read<string[]>(magicArgs.skillInfo.ToString());
        bool isHuman = magicArgs.targets[0].args[0] == "human";
        PlayerController player = PlayMangement.instance.player;
        BattleConnector socket = PlayMangement.instance.SocketHandler;

        if (player.isHuman != isHuman) {
            callback();
            return;
        }
        else {
            socket.DrawNewCards(itemIds);
        }
        callback();
    }

    //투석 공격
    public void ac10021(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string[] itemIds = dataModules.JsonReader.Read<string[]>(magicArgs.skillInfo.ToString());
        bool isHuman = magicArgs.itemId[0] == 'H';
        PlayerController targetPlayer = PlayMangement.instance.player.isHuman == isHuman ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
        FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;

        for (int i = 0; i < itemIds.Length; i++) {
            if(itemIds[i] != "hero") {
                PlaceMonster unit = observer.GetUnitToItemID(itemIds[i]).GetComponent<PlaceMonster>();
                //unit.RequestChangeStat(0, Acco)
            }
            else {

            }


        }
        AfterAction(EffectSystem.Instance.GetAnimationTime(EffectSystem.EffectType.TREBUCHET), callback);
    }

    //한파
    public void ac10022(object args, DequeueCallback callback) {

        callback();
    }
}

