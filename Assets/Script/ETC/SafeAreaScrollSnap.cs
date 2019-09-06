using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaScrollSnap : MonoBehaviour {
    List<RectTransform> rects;

    private void Awake() {
        Init();
    }

    public void Refresh(Rect r) {
        if (rects == null || rects.Count == 0) Init();

        foreach (RectTransform rt in rects) {
            Logger.Log(rt.offsetMin);
            Logger.Log(rt.offsetMax);
            rt.offsetMin = new Vector2(rt.offsetMin.x, -1 * r.height / 2.0f);
            rt.offsetMax = new Vector2(rt.offsetMax.x, r.height / 2.0f);
            //rt.offsetMin = new Vector2(0, 0);
            //rt.offsetMax = new Vector2(0, 0);
        }
    }

    void Init() {
        rects = new List<RectTransform>();
        foreach (Transform tf in transform) {
            rects.Add(tf.GetComponent<RectTransform>());
        }
    }
}
