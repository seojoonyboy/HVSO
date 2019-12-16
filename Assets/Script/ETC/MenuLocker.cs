using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Linq;
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

    Color32 deactiveColor = new Color32(70, 70, 70, 255);
    Color32 activeColor = new Color32(255, 255, 255, 255);
    List<Image> _images = new List<Image>();
    List<SkeletonGraphic> _skeletonGraphics = new List<SkeletonGraphic>();

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
        DeactiveInnerImages();

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

        ActiveInnerImages();
        if (!gameObject.activeInHierarchy) {
            button = transform.parent.GetComponent<Button>();
            button.enabled = true;

            gameObject.SetActive(false);
        }
        else {
            if (skeletonGraphic == null) Start();
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

    public void OnlyUnlockEffect() {
        if (state == State.UNLOCKED) return;

        if (gameObject.activeInHierarchy) {
            skeletonGraphic.enabled = true;
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        }
        ActiveInnerImages();

        state = State.UNLOCKED;
    }

    public void UnlockWithNoEffect() {
        if (button == null) button = transform.parent.GetComponent<Button>();
        button.enabled = true;
        gameObject.SetActive(false);

        state = State.UNLOCKED;
    }

    public State GetState() {
        return state;
    }

    public enum State {
        LOCKED,
        UNLOCKED,
        DEFAULT
    }

    public string GetSlotName(int sibilingIndex) {
        switch (sibilingIndex) {
            case 1:
                return "2";
            case 2:
                return "3";
            case 3:
                return "5";
            case 4:
                return "4";
        }
        return "";
    }

    /// <summary>
    /// 구조가 다른 경우가 있음...
    /// </summary>
    private void DeactiveInnerImages() {
        switch (transform.parent.name) {
            case "LeagueButton":
            case "StoryButton":
                transform.parent.parent.GetComponent<SkeletonGraphic>().Initialize(false);
                transform.parent.parent.GetComponent<SkeletonGraphic>().Skeleton.SetColor(deactiveColor);
                break;
            case "DeckEdit":
            case "Dictionary":
            case "Shop":
            case "Story":
                var parentSibilingIndex = transform.parent.GetSiblingIndex();

                transform.root.Find("InnerCanvas/ButtonAnimation").GetComponent<SkeletonGraphic>().Initialize(false);
                SkeletonGraphic skeletonGraphic = transform.root.Find("InnerCanvas/ButtonAnimation").GetComponent<SkeletonGraphic>();
                var slot = skeletonGraphic.Skeleton.FindSlot(GetSlotName(parentSibilingIndex));
                slot.SetColor(deactiveColor);

                slot = skeletonGraphic.Skeleton.FindSlot("1");
                slot.SetColor(deactiveColor);
                break;
            case "ModeButton":
            case "DeckObject":
                transform.parent.GetComponent<Image>().color = deactiveColor;
                break;
            case "RewardBox":
                transform.parent.Find("WoodenImage").GetComponent<Image>().color = deactiveColor;
                transform.parent.Find("BoxImage/BoxValue").GetComponent<Image>().color = deactiveColor;
                transform.parent.Find("BoxImage/Image").GetComponent<Image>().color = deactiveColor;
                transform.parent.Find("SupplyGauge").GetComponent<Image>().color = deactiveColor;
                transform.parent.Find("SupplyGauge/Image").GetComponent<Image>().color = deactiveColor;
                //transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Initialize(true);
                transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Initialize(false);
                transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.FindSlot("box").SetColor(deactiveColor);
                break;
        }
    }

    private void ActiveInnerImages() {
        switch (transform.parent.name) {
            case "LeagueButton":
            case "StoryButton":
                transform.parent.parent.GetComponent<SkeletonGraphic>().Initialize(false);
                transform.parent.parent.GetComponent<SkeletonGraphic>().Skeleton.SetColor(activeColor);
                break;
            case "DeckEdit":
            case "Dictionary":
            case "Shop":
            case "Story":
                var parentSibilingIndex = transform.parent.GetSiblingIndex();

                transform.root.Find("InnerCanvas/ButtonAnimation").GetComponent<SkeletonGraphic>().Initialize(false);
                SkeletonGraphic skeletonGraphic = transform.root.Find("InnerCanvas/ButtonAnimation").GetComponent<SkeletonGraphic>();
                var slot = skeletonGraphic.Skeleton.FindSlot(GetSlotName(parentSibilingIndex));
                slot.SetColor(activeColor);

                break;
            case "ModeButton":
            case "DeckObject":
                transform.parent.GetComponent<Image>().color = activeColor;
                break;
            case "RewardBox":
                transform.parent.Find("WoodenImage").GetComponent<Image>().color = activeColor;
                transform.parent.Find("BoxImage/BoxValue").GetComponent<Image>().color = activeColor;
                transform.parent.Find("BoxImage/Image").GetComponent<Image>().color = activeColor;
                transform.parent.Find("SupplyGauge").GetComponent<Image>().color = activeColor;
                transform.parent.Find("SupplyGauge/Image").GetComponent<Image>().color = activeColor;
                //transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Initialize(true);
                transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Initialize(false);
                transform.parent.Find("BoxImage").GetChild(0).GetComponent<SkeletonGraphic>().Skeleton.FindSlot("box").SetColor(activeColor);
                break;
        }
    }
}
