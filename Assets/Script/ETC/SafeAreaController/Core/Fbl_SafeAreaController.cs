using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SafeAreaMethodType {
    CanvasBased,
    CameraBased,
};

public enum AreaUpdateTiming {
    OnReciveMessage = (1 << 0),
    Awake = (1 << 1),
    OnEnable = (1 << 2),
    Start = (1 << 3),
    Update = (1 << 4),
    FixedUpdate = (1 << 5),
};

public class Fbl_SafeAreaController : MonoBehaviour {
    public SafeAreaMethodType ControlType = SafeAreaMethodType.CanvasBased;

    public bool addSoftkeyArea = false;

    [EnumMask]
    public AreaUpdateTiming UpdateTimming = AreaUpdateTiming.Awake;

    private Canvas _mainCanvas;
    private Rect Offset { get { return new Rect(0, 0, 0, navigationBarHeight); } }
    private static int navigationBarHeight = 0;

    public int additionalSortingOrder = 0;

    // Update Function
    public void UpdateSafeArea() {
        switch (this.ControlType) {
            case SafeAreaMethodType.CanvasBased:
                UpdateCanvasProperty(_mainCanvas.sortingOrder, Offset);
                break;
            case SafeAreaMethodType.CameraBased:
                //UpdateCameraProperty();
                break;
            default:
                break;
        }
    }

    // Life cycle function
    private void Awake() {
        _mainCanvas = GetComponent<Canvas>();

        if (HaveMask(AreaUpdateTiming.Awake))
            UpdateSafeArea();
    }

    private void OnEnable() {
        if (HaveMask(AreaUpdateTiming.OnEnable))
            UpdateSafeArea();
    }

    private void Start() {
        if (HaveMask(AreaUpdateTiming.Start))
            UpdateSafeArea();
    }

    private void Update() {
        if (HaveMask(AreaUpdateTiming.Update))
            UpdateSafeArea();
    }

    private void FixedUpdate() {
        if (HaveMask(AreaUpdateTiming.FixedUpdate))
            UpdateSafeArea();
    }

    // Utility
    private bool HaveMask(AreaUpdateTiming mask) {
        return ((int)UpdateTimming & (int)mask) != 0;
    }

    // Update Method
    public void UpdateCanvasProperty(int rootSortingOrder, Rect offset) {
        Logger.Log("노치 대응");

        // 0. Get Value
        Canvas myCanvas = GetComponent<Canvas>();
        RectTransform myTransform = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;
        Vector2 screen = new Vector2(Screen.width, Screen.height);

        Vector2 _saAnchorMin;
        Vector2 _saAnchorMax;

        var offset_right = offset.x;
        var offset_left = offset.y;
        var offset_top = offset.width;
        var offset_bottom = offset.height;

        // 1. Setup and apply safe area
        _saAnchorMin.x = (safeArea.x + offset_right) / screen.x;
        _saAnchorMin.y = (safeArea.y + offset_bottom) / screen.y;
        _saAnchorMax.x = (safeArea.x + safeArea.width - offset_top) / screen.x;
        _saAnchorMax.y = (safeArea.y + safeArea.height - offset_left) / screen.y;

        myTransform.anchorMin = _saAnchorMin;
        myTransform.anchorMax = _saAnchorMax;

        // 2. Add aditional sorting order
        myCanvas.sortingOrder = rootSortingOrder + additionalSortingOrder;
    }
}
