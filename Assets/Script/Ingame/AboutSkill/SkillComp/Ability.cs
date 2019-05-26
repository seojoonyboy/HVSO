using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 카드 고유 능력 처리에 대한 스크립트
/// </summary>
public class Ability : MonoBehaviour {
    protected ActivatedSkillsManager skillManager;
    public UnityEvent<object> OnDropFinished;
    public UnityEvent<object> OnBeginDragFinished;

    public bool isChangeDropableSlot = false;

    void Awake() {
        skillManager = ActivatedSkillsManager.Instance;    
    }

    public virtual void InitData(object data) { }

    public virtual void BeginCardPlay() {

    }
    public virtual void EndCardPlay() { }
    public virtual void OnAttack() { }
}
