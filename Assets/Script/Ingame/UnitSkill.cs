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


    protected async void AfterCallAction(float time = 0f, AfterCallBack callAction = null, DequeueCallback callback = null) {
        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(time));
        callAction?.Invoke();
        callAction = null;
        callback?.Invoke();
    }

    public void ac10020(object args, DequeueCallback callback) {
        JObject method = (JObject)args;
        
        string[] toArray = dataModules.JsonReader.Read<string[]>(method["to"].ToString());
        string from = method["from"].ToString();

        for(int i = 0; i<toArray.Length; i++) {
            string itemID = toArray[i];
            PlaceMonster unit = unitObserver.GetUnitToItemID(itemID).GetComponent<PlaceMonster>();

            if (unit.isPlayer == true)
                unit.gameObject.AddComponent<CardUseSendSocket>().Init(false);
            else
                unit.gameObject.AddComponent<CardSelect>().EnemyNeedSelect();
        }
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


    public void ac10030(object args, DequeueCallback callback) {


        callback();
    }

    public void ac10041(object args, DequeueCallback callback) {


        callback();
    }


}
