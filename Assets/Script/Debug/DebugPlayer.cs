using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using Bolt;
using Spine;
using Spine.Unity;

public class DebugPlayer : MonoBehaviour
{
    public bool isHuman;
    public bool isPlayer;

    public GameObject backLine;
    public GameObject frontLine;

    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    public Vector3 unitClosePosition;
    public Vector3 wallPosition;

    [SerializeField]
    protected DebugHeroSpine heroSpine;
    public static int activeCardMinCost;

    public enum HeroState {
        IDLE,
        ATTACK,
        HIT,
        DEAD
    }

    private void Start() {
        HP = new ReactiveProperty<int>(20);
        heroSpine = transform.Find("skeleton").GetComponent<DebugHeroSpine>();
        shieldCount = 3;
    }

    public void PlayerTakeDamage(int amount) {
        HP.Value -= amount;
        SetState(HeroState.HIT);
    }


    private void ObserverText() {
        //Image shieldImage = playerUI.transform.Find("PlayerHealth").GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>();

        //var ObserveHP = HP.Subscribe(_ => ChangedHP()).AddTo(PlayMangement.instance.transform.gameObject);
        //var ObserveResource = resource.Subscribe(_ => ChangedResource()).AddTo(PlayMangement.instance.transform.gameObject);
        //var ObserveShield = shieldStack.Subscribe(_ => shieldImage.fillAmount = (float)shieldStack.Value / 8).AddTo(PlayMangement.instance.transform.gameObject);
        //var heroDown = HP.Where(x => x <= 0).Subscribe(_ => ).AddTo(PlayMangement.instance.transform.gameObject);

        var gameOverDispose = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                  SetState(HeroState.DEAD);
                                  PlayMangement.instance.GetBattleResult();
                              })
                              .AddTo(PlayMangement.instance.transform.gameObject);
    }
    

    protected void SetState(HeroState state) {
        if (heroSpine == null) return;

        switch (state) {
            case HeroState.IDLE:
                heroSpine.Idle();
                break;
            case HeroState.HIT:
                heroSpine.Hit();
                break;
            case HeroState.ATTACK:
                heroSpine.Attack();
                break;
            case HeroState.DEAD:
                heroSpine.Dead();
                break;
        }
    }
}
