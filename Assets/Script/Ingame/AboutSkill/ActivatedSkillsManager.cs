using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using dataModules;
using System.Linq;
using System;

/// <summary>
/// Set Instance
/// </summary>
public partial class ActivatedSkillsManager : SerializedMonoBehaviour {
    IngameEventHandler eventHandler;
    public bool isPlayer;

    void Awake() {
        activatedSkillList = new List<SkillSet>();
    }

    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;

        RemoveEventListeners();
        AddEventListeners();
    }

    private void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        var event_type = (IngameEventHandler.EVENT_TYPE)Event_Type;
        if (isPlayer == (bool)Param) {
            TriggerSkills(event_type);
            Debug.Log("플레이어1의 " + event_type.ToString() + "이벤트 발생");
        }
        else {
            Debug.Log("플레이어2의 " + event_type.ToString() + "이벤트 발생");
        }
    }

    /// <summary>
    /// 카드 관련 이벤트 리스너 등록
    /// </summary>
    private void AddEventListeners() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_ORC_PRE_TURN, OnEventOccured);

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_HUMAN_TURN, OnEventOccured);

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_ORC_POST_TURN, OnEventOccured);

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnEventOccured);

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEventOccured);

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ATTACK, OnEventOccured);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_ATTACK, OnEventOccured);
    }

    /// <summary>
    /// 카드 관련 이벤트 리스너 해제
    /// </summary>
    private void RemoveEventListeners() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_ORC_PRE_TURN, OnEventOccured);

        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_HUMAN_TURN, OnEventOccured);

        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_ORC_POST_TURN, OnEventOccured);

        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnEventOccured);

        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEventOccured);

        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ATTACK, OnEventOccured);
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_ATTACK, OnEventOccured);
    }
}

/// <summary>
/// Managing all Skills, except immidiate skill
/// </summary>
public partial class ActivatedSkillsManager {
    [SerializeField] List<SkillSet> activatedSkillList;

    public void AddSkill(SkillSet set) {
        activatedSkillList.Add(set);
    }

    public void RemoveSkill(string keyword) {

    }

    public void TriggerSkills(IngameEventHandler.EVENT_TYPE triggerType) {
        string keyword = triggerType.ToString().ToLower();
        var selectedList = FindSkillList(keyword);

        foreach(SkillSet set in selectedList) {
            set.target.GetComponent<Ability>().TriggerSkill(keyword);
        }
    }

    List<SkillSet> FindSkillList(string keyword) {
        var result = activatedSkillList.FindAll(x => x.data.activate.trigger == keyword);
        return result;
    }
}

public partial class ActivatedSkillsManager {
    public class SkillSet {
        public Skill data = new Skill();
        public GameObject target;

        public SkillSet(Skill data, GameObject target) {
            this.data = data;
            this.target = target;
        }
    }
}