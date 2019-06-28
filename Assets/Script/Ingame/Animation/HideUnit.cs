using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class HideUnit : MonoBehaviour
{
    [SpineAnimation]
    public string appear;

    [SpineAnimation]
    public string idle;

    [SpineAnimation]
    public string disappear;
    

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;
    protected string currentAnimationName;

    [HideInInspector]
    public UnitSpine unitSpine;

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;
    }

    public virtual void Appear() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, appear, false);
        currentAnimationName = appear;
        entry.Complete += Idle;
    }

    public virtual void Idle(TrackEntry trackEntry = null) {
        spineAnimationState.SetAnimation(0, idle, true);
        currentAnimationName = idle;
    }

    public virtual void Disappear() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, disappear, false);
        currentAnimationName = disappear;
        entry.Complete += DisappearGilli;
    }

    public void DisappearGilli(TrackEntry trackEntry = null) {
        gameObject.SetActive(false);
    }

    public virtual void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if (e.Data.Name == "APPEAR") {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.APPEAR, transform.position);
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(SoundType.APPEAR_UNIT);
        }

        if (e.Data.Name == "HIDING") {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.APPEAR, transform.position);
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(SoundType.APPEAR_UNIT);

            unitSpine.transform.gameObject.SetActive(true);
            unitSpine.Appear();
        }
    }

}
