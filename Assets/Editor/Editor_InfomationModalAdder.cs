using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Editor_InfomationModalAdder : EditorWindow {
    [MenuItem("GameObject/UI/InfomationModal")]
    public static void AddModal() {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/InfomationModal"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
        obj.name = "InfomationModal";
        obj.transform.SetAsLastSibling();
    }
}
