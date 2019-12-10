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
    void Start() {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        button = GetComponentInParent<Button>();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }

    public void Lock() {
        StartCoroutine(_Lock());
    }

    IEnumerator _Lock() {
        skeletonGraphic.Initialize(true);
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        yield return new WaitForEndOfFrame();

        skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(1.0f);
        skeletonGraphic.enabled = false;

        button.enabled = false;
    }

    public void Unlock() {
        skeletonGraphic.enabled = true;
        StartCoroutine(_Unlock());
    }

    IEnumerator _Unlock() {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(true);
        skeletonGraphic.AnimationState.SetAnimation(0, "NOANI", false);

        button.enabled = true;
    }
}
