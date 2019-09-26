using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.Events;

public class HeroSpine : MonoBehaviour
{
    [SpineAnimation]
    public string idleAnimationName;
    [SpineAnimation]
    public string attackAnimationName;
    [SpineAnimation]
    public string hitAnimationName;
    [SpineAnimation]
    public string deadAnimationName;
    [SpineAnimation]
    public string shieldAnimationName;
    [SpineAnimation]
    public string lastHitAnimationName;


    [SpineAnimation]
    public string dummyAnimation;

    protected SkeletonAnimation skeletonAnimation;
    protected Skeleton skeleton;

    protected string currentAnimationName;

    protected Slot face;

    public UnityAction defenseFinish;
    public UnityAction afterAction;
    private bool thinking = false;
    
    public float deadTime {
        get { return skeletonAnimation.skeleton.Data.FindAnimation(deadAnimationName).Duration; }
    }


    private void Start() {
        Init();
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonAnimation.AnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;

        skeletonAnimation.skeleton.SetAttachment("head_lowHP", null);
        skeletonAnimation.skeleton.SetAttachment("head", "head");
    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {

    }

    public virtual void Idle(TrackEntry trackEntry = null) {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Hit() {
        TrackEntry entry;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        
        entry.Complete += Idle;
    }

    public virtual void Attack() {
        TrackEntry entry;
        skeletonAnimation.timeScale = 0.8f;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
        entry.Complete += DefendFinish;
        entry.Complete += Idle;
    }

    public virtual void Shield() {
        TrackEntry entry;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, shieldAnimationName, false);
        currentAnimationName = shieldAnimationName;
        entry.Complete += DefendFinish;
        entry.Complete += Idle;
    }


    public virtual void Dead() {
        skeletonAnimation.skeleton.SetAttachment("head_lowHP", "head2");
        EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HERO_DEAD, transform.Find("effect_body").position);
        TrackEntry entry;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, lastHitAnimationName, false);
        currentAnimationName = deadAnimationName;
    }


    public virtual void DefendFinish(TrackEntry trackEntry = null) {
        if (defenseFinish != null) defenseFinish();
    }

    public async void Thinking() {
        thinking = true;
        await System.Threading.Tasks.Task.Delay(7000);
        if(!thinking) return;
        if(gameObject == null) return;
        SkeletonAnimation thinkAni = transform.GetChild(0).GetComponent<SkeletonAnimation>();
        thinkAni.gameObject.SetActive(true);
        TrackEntry x = thinkAni.AnimationState.SetAnimation(0, "APPEAR", false);
        x.Complete += (y) => thinkAni.AnimationState.SetAnimation(0, "IDLE", true);
    }

    public void ThinkDone() {
        thinking = false;
        transform.GetChild(0).gameObject.SetActive(false);
    }


}
