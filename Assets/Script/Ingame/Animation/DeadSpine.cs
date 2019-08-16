using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class DeadSpine : MonoBehaviour
{
    [SpineAnimation]
    public string DeadAnimationName;
    [SpineEvent]
    public string deadEventName;


    protected string currentAnimationName;

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;

    [SpineSkin]
    public string humanTomb;
    [SpineSkin]
    public string orcTomb;

    public GameObject target;

    public void StartAnimation(bool race) {
        EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.DEAD, transform.position);
        Destroy(target, 0.05f);

        SoundManager.Instance.PlaySound(SoundType.DEAD);

        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        //spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;

        string setRace = (race == true) ? "human" : "orc";
        skeleton.SetSkin(setRace);

        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, DeadAnimationName, false);
        entry.Complete += DestroyTomb;
    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if(e.Data.Name == deadEventName) {
        }
    }

    public void DestroyTomb(TrackEntry entry = null) {
        Destroy(gameObject);
    }


}
