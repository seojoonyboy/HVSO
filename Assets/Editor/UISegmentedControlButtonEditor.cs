using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

[CustomEditor(typeof(DelayButton))]
public class UISegmentedControlButtonEditor : UnityEditor.UI.ButtonEditor {
    public override void OnInspectorGUI() {

        DelayButton targetMenuButton = (DelayButton)target;
        base.OnInspectorGUI();

        targetMenuButton._delay = EditorGUILayout.FloatField("Delay", targetMenuButton._delay);
    }
}