using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class LocalizationAdder : EditorWindow {
    [MenuItem("Haegin/Add Fbl_TextConverter")]
    public static void ShowWindow() {
        System.Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
        EditorWindow.GetWindow<LocalizationAdder>("Text Converter", new System.Type[] { inspectorType });
    }

    void OnGUI() {
        GUILayout.Label("Settings");

        if (GUILayout.Button("Add Component")) {
            AddComp();
        }
    }

    void AddComp() {
         var textmeshProObjects = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
        foreach(object obj in textmeshProObjects) {
            if (((TextMeshProUGUI)obj).gameObject.GetComponent<FblTextConverter>() == null)
                ((TextMeshProUGUI)obj).gameObject.AddComponent<FblTextConverter>();
        }

        var textObjects = Resources.FindObjectsOfTypeAll(typeof(Text));
        foreach(object obj in textObjects) {
            if(((Text)obj).gameObject.GetComponent<FblTextConverter>() == null)
                ((Text)obj).gameObject.AddComponent<FblTextConverter>();
        }
    }
}
