using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class TouchEffecter : Singleton<TouchEffecter> {
    // Update is called once per frame
    Transform spineParent;
    public void SetScript() {
        return;
    }
    private void Awake() {
        spineParent = Instantiate(Resources.Load("Prefabs/TouchScreenObjects") as GameObject).transform;
        DontDestroyOnLoad(spineParent);
    }
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            spineParent.GetChild(0).gameObject.SetActive(true);
            spineParent.GetChild(0).GetComponent<SkeletonGraphic>().Initialize(true);
            spineParent.GetChild(0).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
            spineParent.GetChild(0).position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            iTween.RotateTo(spineParent.GetChild(0).gameObject, iTween.Hash("z", Random.Range(0, 360), "islocal", true, "time", 0.01f));
            spineParent.GetChild(0).SetAsLastSibling();
        }
    }
}
