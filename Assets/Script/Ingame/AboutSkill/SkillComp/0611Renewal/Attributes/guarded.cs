using System;
using TMPro;
using UnityEngine;

namespace SkillModules {
    public class guarded : UnitAttribute {
        private TextMeshPro textPro;
        IngameEventHandler eventHandler;

        private void Start() {
            //TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            //textPro.gameObject.SetActive(true);
            //textPro.text = "보호 받음";
            //PlayMangement.instance.AddSkillIcon("protect", transform);           


            Transform bodyBone = (gameObject.GetComponent<PlaceMonster>() != null) ? gameObject.GetComponent<PlaceMonster>().unitSpine.bodybone : gameObject.GetComponent<PlayerController>().bodyTransform;
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.NO_DAMAGE, transform, bodyBone);

            eventHandler = PlayMangement.instance.EventHandler;
            eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnBattleEndTurn);
        }

        private void OnBattleEndTurn(Enum Event_Type, Component Sender, object Param) {
            Destroy(GetComponent<guarded>());
        }

        void OnDestroy() {
            //TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            //textPro.gameObject.SetActive(false);
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.NO_DAMAGE, transform);
            //PlayMangement.instance.DisabelSkillIcon("protect", transform);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnBattleEndTurn);
        }
    }
}
