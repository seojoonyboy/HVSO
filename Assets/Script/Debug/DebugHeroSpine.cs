using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.Events;

public class DebugHeroSpine : MonoBehaviour
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
    public string dummyAnimation;

    protected SkeletonAnimation skeletonAnimation;
    protected Skeleton skeleton;

    protected string currentAnimationName;

    public UnityAction defenseFinish;

    private void Start() {
        Init();
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonAnimation.AnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;
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

    public virtual void Dead() {
        TrackEntry entry;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, deadAnimationName, false);
        currentAnimationName = hitAnimationName;
    }


    public virtual void DefendFinish(TrackEntry trackEntry = null) {
        if (defenseFinish != null) defenseFinish();
    }
}
