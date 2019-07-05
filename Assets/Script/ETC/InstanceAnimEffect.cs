using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonModules {
    public class InstanceAnimEffect : InstanceExecuteInDelayButton {
        [SerializeField] SkeletonGraphic skeleton;
        public string animName;

        public override void Execute() {
            skeleton.AnimationState.SetAnimation(0, animName, false);
        }
    }
}