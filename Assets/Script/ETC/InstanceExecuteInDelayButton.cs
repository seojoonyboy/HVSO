using UnityEngine;

namespace ButtonModules {
    [RequireComponent(typeof(DelayButton))]
    public class InstanceExecuteInDelayButton : MonoBehaviour {
        DelayButton delayButton;
        void Start() {
            delayButton = GetComponent<DelayButton>();
            delayButton.instanceCallback.AddListener(() => Execute());
        }

        public virtual void Execute() { }
    }
}