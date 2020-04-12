using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SocketFormat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UnitSkill {

    public class CardPlayArgs {

    }

    public delegate void AfterCallBack();
    FieldUnitsObserver unitObserver;
    BattleConnector socket;


    public void Activate(string cardId, object args, DequeueCallback callback) {
        MethodInfo theMethod = this.GetType().GetMethod(cardId);
        object[] parameter = new object[] { args, callback };
        unitObserver = unitObserver == null ? PlayMangement.instance.UnitsObserver : unitObserver;
        socket = socket == null ? PlayMangement.instance.socketHandler : socket;
        if (theMethod == null) {
            Logger.Log(cardId + "해당 카드는 아직 준비가 안되어있습니다.");
            callback();
            return;
        }
        theMethod.Invoke(this, parameter);
    }

    public void Activate_ToArms(string cardId, object args, DequeueCallback callback) {
        MethodInfo theMethod = this.GetType().GetMethod(cardId);
        object[] parameter = new object[] { args, callback };
        unitObserver = unitObserver == null ? PlayMangement.instance.UnitsObserver : unitObserver;
        socket = socket == null ? PlayMangement.instance.socketHandler : socket;
        if (theMethod == null) {
            Logger.Log(cardId + "해당 카드는 아직 준비가 안되어있습니다.");
            DefaultMovement(cardId, args, callback);
            return;
        }
        theMethod.Invoke(this, parameter);
    }



    protected void DefaultMovement(string cardId, object args, DequeueCallback callback) {
        FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
        JObject method = (JObject)args;

        GameState gameState =  PlayMangement.instance.socketHandler.gameState;
        string[] toList = dataModules.JsonReader.Read<string[]>(method["to"].ToString());


        bool needMove = false;
        for (int i = 0; i < toList.Length; i++) {
            string itemId = toList[i].ToString();
            GameObject unitObject = observer.GetUnitToItemID(itemId);
            if (unitObject != null) {
                PlaceMonster monster = unitObject.GetComponent<PlaceMonster>();
                monster.UpdateGranted();
                FieldUnitsObserver.Pos pos = gameState.map.allMonster.Find(x => x.itemId.CompareTo(itemId) == 0).pos;
                if (pos.col != monster.x) needMove = true;
            }
            else {
                Unit unit = gameState.map.allMonster.Find(x => x.itemId.CompareTo(itemId) == 0);
                bool isPlayer = PlayMangement.instance.player.isHuman == (unit.origin.camp.CompareTo("human") == 0);
                PlayMangement.instance.SummonUnit(isPlayer, unit.origin.id, unit.pos.col, unit.pos.row, unit.itemId, -1, null, true);
            }
        }

        if (needMove) {
            for (int i = 0; i < toList.Length; i++) {
                string itemId = toList[i].ToString();
                GameObject toMonster = observer.GetUnitToItemID(itemId);
                Unit unit = gameState.map.allMonster.Find(x => string.Compare(x.itemId, itemId, StringComparison.Ordinal) == 0);
                observer.UnitChangePosition(toMonster, unit.pos, toMonster.GetComponent<PlaceMonster>().isPlayer, string.Empty, () => callback());
            }
        }
        else callback();
    }


    protected async void AfterCallAction(float time = 0f, AfterCallBack callAction = null, DequeueCallback callback = null) {
        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(time));
        callAction?.Invoke();
        callAction = null;
        callback?.Invoke();
    }

    public void ac10020(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();
        PlaceMonster unit = unitObserver.GetUnitToItemID(from).GetComponent<PlaceMonster>();
        if (unit.isPlayer == true)
            unit.gameObject.AddComponent<CardUseSendSocket>().Init(false);
        else
            unit.gameObject.AddComponent<CardSelect>().EnemyNeedSelect();
        callback();
    }


    public void ac10083(object args, DequeueCallback callback) {
        JObject method = (JObject)args;

        string from = method["from"].ToString();
        PlaceMonster unit = unitObserver.GetUnitToItemID(from).GetComponent<PlaceMonster>();
        PlayerController player;



        if (unit.isPlayer) {
            player = PlayMangement.instance.player;
            player.resource.Value = socket.gameState.players.orc.resource;
            player.ActiveOrcTurn();
        }
        else {
            player = PlayMangement.instance.enemyPlayer;
            player.resource.Value = socket.gameState.players.orc.resource;
        }
        callback();
    }

    /// <summary>
    /// 마력저장소 (turn_start trigger)
    /// </summary>
    /// <param name="args"></param>
    /// <param name="callback"></param>
    public void ac10030(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        
        string from = method["from"].ToString();
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());
        
        
        PlaceMonster unit = unitObserver.GetUnitToItemID(from).GetComponent<PlaceMonster>();
        for(int i = 0; i<toArray.Length; i++) {
            toArray[i] = toArray[i].Replace("\r\n", String.Empty).Trim();
            if (toArray[i] == "resource") {
                PlayerController player;
                if (unit.isPlayer) {
                    player = PlayMangement.instance.player;
                    player.resource.Value = player.isHuman ? socket.gameState.players.human.resource : socket.gameState.players.orc.resource;
                    // Logger.Log("<color=yellow> 마력 저장소 스킬 : " + player.resource.Value + "</color>");
                }
                else {
                    player = PlayMangement.instance.enemyPlayer;
                    player.resource.Value = !player.isHuman ? socket.gameState.players.human.resource : socket.gameState.players.orc.resource;
                }
                //TODO : Effect 적용 필요함
            }
        }
        callback();
    }

    public void ac10041(object args, DequeueCallback callback) {
        callback();
    }

    public void ac10056(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());
        PlayerController player = PlayMangement.instance.player;
        BattleConnector socket = PlayMangement.instance.SocketHandler;

        if (!player.isHuman)
            PlayMangement.instance.StartCoroutine(PlayMangement.instance.EnemyMagicCardDraw(toArray.Length, callback));
        else
            socket.DrawNewCards(toArray, callback);
    }


    //녹색의 현자
    public void ac10059(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();

        GameObject unit = unitObserver.GetUnitToItemID(from);
        PlaceMonster unitData = unit.GetComponent<PlaceMonster>();
        
        if(unitData.isPlayer == true) 
            PlayMangement.instance.player.resource.Value = PlayMangement.instance.socketHandler.gameState.players.myPlayer(PlayMangement.instance.player.isHuman).resource;        
        else 
            PlayMangement.instance.enemyPlayer.resource.Value = PlayMangement.instance.socketHandler.gameState.players.enemyPlayer(PlayMangement.instance.enemyPlayer.isHuman).resource;
        
        callback();
    }


    //탐구하는자
    public void ac10062(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();

        GameObject unit = unitObserver.GetUnitToItemID(from);
        unit.GetComponent<PlaceMonster>().UpdateGranted();
        callback();
    }

    public void ac10080(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());

        for(int i = 0; i<toArray.Length; i++) 
            unitObserver.GetUnitToItemID(toArray[i]).GetComponent<PlaceMonster>().UpdateGranted();      


        callback();
    }


    //맹렬한 추적자
    public void ac10089(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());


        List<GameObject> targetUnit = new List<GameObject>();

        for(int i = 0; i<toArray.Length; i++) {
            if (toArray[i].Contains("hero")) {
                PlayerController targetPlayer;
                if (toArray[i] == "hero_human")
                    targetPlayer = (PlayMangement.instance.player.isHuman == true) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
                else
                    targetPlayer = (PlayMangement.instance.player.isHuman == false) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
                targetUnit.Add(targetPlayer.gameObject);
            }
            else {
                GameObject unit = unitObserver.GetUnitToItemID(toArray[i]);
                targetUnit.Add(unit);
            }
        }

        GameObject attackUnit = unitObserver.GetUnitToItemID(from);
        attackUnit.GetComponent<PlaceMonster>().GetTarget(targetUnit, callback);
    }


    public void ac10087(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());

        GameObject unit = unitObserver.GetUnitToItemID(from);
        bool isPlayer = unit.GetComponent<PlaceMonster>().isPlayer;
        PlayerController targetPlayer = (isPlayer == true) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;


        if (isPlayer == true)
            PlayMangement.instance.socketHandler.DrawNewCards(toArray, callback);
        else
            targetPlayer.StartCoroutine(PlayMangement.instance.EnemyMagicCardDraw(toArray.Length, callback));
    }

    public void ac10079(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string from = method["from"].ToString();
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());

        for(int i = 0; i < toArray.Length; i++) 
            unitObserver.GetUnitToItemID(toArray[i]).GetComponent<PlaceMonster>().UpdateGranted();

        callback();
    }

    public void ac10053(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        string fromMessage = method["from"].ToString();
        bool isHuman = fromMessage[0] == 'H' ? true : false;

        //PlayerController player = (PlayMangement.instance.player.isHuman == isHuman) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;


        if(PlayMangement.instance.player.isHuman == isHuman) {
            PlayMangement.instance.player.resource.Value += 1;
            PlayMangement.instance.player.ActivePlayer();
        }
        else
            PlayMangement.instance.enemyPlayer.resource.Value += 1;

        callback();
    }

}
