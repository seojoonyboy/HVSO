using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 카드 고유 능력 처리에 대한 스크립트
/// </summary>
public class Ability : MonoBehaviour {
    public UnityEvent OnDropFinished;

    //유닛 소환시 추가 처리
    //딱히 없으면 바로 Invoke 호출
    public virtual void OnDrop() {
        OnDropFinished.Invoke();
    }

    //매번 공격시 추가 처리
    public virtual void OnAttack() { }
}
