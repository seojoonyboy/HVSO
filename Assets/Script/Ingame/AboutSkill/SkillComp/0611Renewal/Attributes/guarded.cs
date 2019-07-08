using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillModules {
    public class guarded : MonoBehaviour {
        private TextMeshPro textPro;
        IngameEventHandler eventHandler;

        private void Start() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(true);
            textPro.text = "보호 받음";
           //PlayMangement.instance.AddSkillIcon("protect", transform);

            eventHandler = PlayMangement.instance.EventHandler;
            eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnBattleEndTurn);
        }

        private void OnBattleEndTurn(Enum Event_Type, Component Sender, object Param) {
            Destroy(GetComponent<guarded>());
        }

        void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);

            //PlayMangement.instance.DisabelSkillIcon("protect", transform);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, OnBattleEndTurn);
        }
    }
}
