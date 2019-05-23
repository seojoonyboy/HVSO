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
        GameObject deadEffect = Instantiate(PlayMangement.instance.effectManager.deadEffect);
        deadEffect.transform.position = transform.position;
        Destroy(target, 0.2f);
        Destroy(deadEffect, deadEffect.GetComponent<ParticleSystem>().main.duration - 0.1f);


        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
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
