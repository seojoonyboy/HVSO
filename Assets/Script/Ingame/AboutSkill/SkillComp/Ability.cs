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

    protected Skill skillData;
    protected Effect effectData;

    public virtual void InitData(Skill data, Effect effectData) {
        skillData = data;
        this.effectData = effectData;
    }

    public virtual void BeginCardPlay() { }
    public virtual void EndCardPlay(ref GameObject card) {
        Debug.Log("EndCardPlay In Abilty");
    }
    public virtual void OnAttack() { }
}
