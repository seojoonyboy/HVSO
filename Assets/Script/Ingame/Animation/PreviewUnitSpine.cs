using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class PreviewUnitSpine : MonoBehaviour
{
    [SpineAnimation]
    public string previewAnimationName;

    protected string currentAnimationName;
    protected SkeletonAnimation skeletonAnimation;
    protected Skeleton skeleton;
    
    public void Init(string unitID) {
        skeletonAnimation = AccountManager.Instance.resource.heroSkeleton[unitID].GetComponent<UnitSpine>().GetSkeleton;

        if (skeletonAnimation != null) {
            gameObject.GetComponent<SkeletonGraphic>().skeletonDataAsset = skeletonAnimation.skeletonDataAsset;
            skeleton = skeletonAnimation.skeleton;
            skeletonAnimation.AnimationState.SetAnimation(0, previewAnimationName, true);
            currentAnimationName = previewAnimationName;
        }
    }    
}
