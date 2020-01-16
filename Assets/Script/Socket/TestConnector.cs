﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using BestHTTP.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketFormat;
using IngameEditor;
using TMPro;


public class TestConnector : BattleConnector {

    void Start() {
        PlayerPrefs.SetString("SelectedBattleType", "solo");
        PlayerPrefs.SetString("SelectedRace", "human");
        PlayerPrefs.SetString("selectedHeroId", "h10001");
        InitGameState();
    }

    private void InitGameState() {
        gameState = new GameState();
        gameState.players.orc.camp = "orc";
        
    }

    public override void OpenSocket() {
        Debug.Log("opened");
        begin_ready(null, null);
    }

    public override void OpenLobby() {
        OpenSocket();
    }
}
