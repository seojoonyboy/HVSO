using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using Bolt;
using Spine;
using Spine.Unity;

public class DebugPlayer : PlayerController
{    
    [SerializeField] public DebugCardHandDeckManager debugcdpm;

    private void Start() {
        HP = new ReactiveProperty<int>(20);
        resource.Value = 30;
        heroSpine = transform.Find("skeleton").GetComponent<DebugHeroSpine>();

        if (isPlayer == true)
            DebugCardDropManager.Instance.SetUnitDropPos();
    }

    public override void PlayerTakeDamage(int amount) {
        HP.Value -= amount;
        SetState(HeroState.HIT);
    }
}
