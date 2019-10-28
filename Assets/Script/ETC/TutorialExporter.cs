using dataModules;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TutorialExporter : MonoBehaviour {
    string exportPath;
    MenuTutorialManager tutorialManager;
    void Start() {
        tutorialManager = GetComponent<MenuTutorialManager>();
        exportPath = "/Script/ETC/Data/test.json";
        SaveData();
    }

    public void SaveData() {
        string dataAsJson = JsonConvert.SerializeObject(tutorialManager.sets);
        string filePath = Application.dataPath + exportPath;
        File.WriteAllText(filePath, dataAsJson);
    }
}
