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

    public delegate void AfterCallBack();
    public AfterCallBack afterCallBack;


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
        afterCallBack?.Invoke();
        afterCallBack = null;
        callback?.Invoke();
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
        if (player.isHuman != isHuman)
            player.StartCoroutine(PlayMangement.instance.EnemyMagicCardDraw(itemIds.Length, callback));
        else
            socket.DrawNewCards(itemIds, callback);
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
        EffectSystem effectSystem = EffectSystem.Instance;
        EffectSystem.ActionDelegate skillAction;
        skillAction = delegate () { attacker.GetTarget(affected); };
        effectSystem.ShowEffectAfterCall(EffectSystem.EffectType.ANGRY, attacker.unitSpine.headbone, skillAction);
        AfterAction(attacker.totalAtkTime + 0.7f, callback);
    }

    //전쟁의 외침
    public void ac10017(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string[] itemIds =  dataModules.JsonReader.Read<string[]>(magicArgs.skillInfo.ToString());
        bool isHuman = magicArgs.targets[0].args[0] == "human";
        PlayerController player = PlayMangement.instance.player;
        BattleConnector socket = PlayMangement.instance.SocketHandler;
        if (player.isHuman != isHuman)
            player.StartCoroutine(PlayMangement.instance.EnemyMagicCardDraw(itemIds.Length, callback));
        else
            socket.DrawNewCards(itemIds, callback);
    }

    //투석 공격
    public void ac10021(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string[] itemIds = dataModules.JsonReader.Read<string[]>(magicArgs.skillInfo.ToString());
        bool userIsHuman = magicArgs.itemId[0] == 'H';
        PlayerController targetPlayer = PlayMangement.instance.player.isHuman == userIsHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
        BattleConnector socket = PlayMangement.instance.SocketHandler;
        int line = int.Parse(magicArgs.targets[0].args[0]);
        Unit[] units = (targetPlayer.isHuman == true) ? socket.gameState.map.lines[line].human : socket.gameState.map.lines[line].orc;
        afterCallBack = delegate () { PlayMangement.instance.CheckLine(line); };
        EffectSystem effectSystem = EffectSystem.Instance;
        EffectSystem.ActionDelegate skillAction;
        //socket.gameState.map.line        
        for (int i = 0; i < itemIds.Length; i++) {
            skillAction = null;
            if (itemIds[i] != "hero") {
                PlaceMonster unit = observer.GetUnitToItemID(itemIds[i]).GetComponent<PlaceMonster>();
                Unit socketUnit = Array.Find(units, x => x.itemId == itemIds[i]);
                skillAction = delegate () { unit.RequestChangeStat(0, -(unit.unit.currentHp - socketUnit.currentHp), "ac10021"); unit.Hit(); };
                effectSystem.ShowEffectOnEvent(EffectSystem.EffectType.TREBUCHET, unit.gameObject.transform.position, skillAction);
            }
            else {
                skillAction = delegate () { targetPlayer.TakeIgnoreShieldDamage(true, "ac10021"); targetPlayer.MagicHit(); };
                effectSystem.ShowEffectOnEvent(EffectSystem.EffectType.TREBUCHET, targetPlayer.bodyTransform.position, skillAction);
            }
        }
        AfterAction(1.1f, callback);
    }

    public void ac10055(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        string targetID = magicArgs.targets[0].args[0];
        GameObject affected = PlayMangement.instance.UnitsObserver.GetUnitToItemID(targetID);
        affected.GetComponent<PlaceMonster>().RequestChangeStat(-1, -1, "ac10055");
        AfterAction(0.5f, callback);
    }


    //한파
    public void ac10022(object args, DequeueCallback callback) {

        callback();
    }
}

