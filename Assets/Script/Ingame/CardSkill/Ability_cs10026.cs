using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// cs10026
/// 공격력이 X 이상인 적 유닛 Y개 처치
/// </summary>
public class Ability_cs10026 : Ability {
    public GameObject selectedTarget;

    public override void OnDrop() {

    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RayCasting();
        }
    }

    void RayCasting() {
        if (isClickable()) {
            OnDropFinished.Invoke();
        }
    }

    bool isClickable() {
        return true;
    }
}
