using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.Events;

public class DebugUnitSpine : UnitSpine
{

    private void Start() {
        Init();
    }


    public override void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;      

        if (arrow != null && transform.parent.GetComponent<DebugUnit>().isPlayer == true) {
            if (rangeUpAttackName != "")
                attackAnimationName = rangeUpAttackName;
            else
                attackAnimationName = generalAttackName;
        }
        else if (arrow != null && transform.parent.GetComponent<DebugUnit>().isPlayer == false) {
            if (rangeDownAttackName != "")
                attackAnimationName = rangeDownAttackName;
            else
                attackAnimationName = generalAttackName;
        }
    }
}
