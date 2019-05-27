using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dataModules;

/// <summary>
/// 카드 고유 능력 처리에 대한 스크립트
/// </summary>
public class Ability : MonoBehaviour {
    public UnityEvent<object> OnDropFinished;
    public UnityEvent<object> OnBeginDragFinished;

    public bool isChangeDropableSlot = false;

    public bool isPlayer = true;

    Skill skillData;
    Condition[] activateConditions;

    [SerializeField] ActivatedSkillsManager 
        playerSkillsManager,
        enemySkillsManager;

    public virtual void InitData(Skill data) {
        if (isPlayer) {
            playerSkillsManager = PlayMangement.instance.Player1SkillsManager;
            enemySkillsManager = PlayMangement.instance.Player2SkillsManager;
        }
        else {
            playerSkillsManager = PlayMangement.instance.Player2SkillsManager;
            enemySkillsManager = PlayMangement.instance.Player1SkillsManager;
        }

        skillData = data;
        activateConditions = skillData.activate.conditions;

        playerSkillsManager.AddSkill(
            new ActivatedSkillsManager.SkillSet(skillData, gameObject)
        );
    }

    public virtual void BeginCardPlay() {

    }
    public virtual void EndCardPlay() { }
    public virtual void OnAttack() { }

    public virtual void TriggerSkill(string keyword) {
        //Debug.Log(keyword + "이벤트 발생");

        bool query = isMyEvent(keyword);
        if (!query) return;

        bool query2 = isAdditionalConditionExist();

    }

    /// <summary>
    /// 자신이 해당 이벤트에 관련되어 있는지 확인
    /// </summary>
    /// <param name="keyword">스킬 처리 발동 시점</param>
    /// <returns>자신이 해당 이벤트에 관련되어 있는가</returns>
    private bool isMyEvent(string keyword) {
        return keyword == skillData.activate.trigger;
    }

    /// <summary>
    /// 이벤트 처리시 추가적인 조건이 있는지 확인
    /// </summary>
    /// <returns>이벤트 처리시 추가적인 조건이 있는가</returns>
    private bool isAdditionalConditionExist() {
        return activateConditions.Length != 0;
    }
}
