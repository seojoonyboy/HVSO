using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Button))]
[ExecuteInEditMode()]
public class MenuLocker : MonoBehaviour {
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/MenuLocker")]
    public static void AddMenuLocker() {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/MenuLocker"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
        obj.transform.SetAsLastSibling();
        obj.name = "Lock";
    }
#endif
    SkeletonGraphic skeletonGraphic;
    Button button;
    State state = State.DEFAULT;
    void Start() {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        button = GetComponentInParent<Button>();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }

    public void Lock() {
        if (state == State.LOCKED) return;

        gameObject.SetActive(true);
        if (!gameObject.activeInHierarchy) {
            button = transform.parent.GetComponent<Button>();
            button.enabled = false;
        }
        else {
            StartCoroutine(_Lock());
        }

        state = State.LOCKED;
    }

    IEnumerator _Lock() {
        yield return new WaitForEndOfFrame();

        skeletonGraphic.AnimationState.SetAnimation(0, "NOANI", false);
        button.enabled = false;
    }

    public void Unlock() {
        if (state == State.UNLOCKED) return;

        if (!gameObject.activeInHierarchy) {
            button = transform.parent.GetComponent<Button>();
            button.enabled = true;
        }
        else {
            skeletonGraphic.enabled = true;
            StartCoroutine(_Unlock());
        }

        state = State.UNLOCKED;
    }

    IEnumerator _Unlock() {
        skeletonGraphic.Initialize(true);
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        yield return new WaitForEndOfFrame();
        skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);

        button.enabled = true;
    }

    public enum State {
        LOCKED,
        UNLOCKED,
        DEFAULT
    }
}
