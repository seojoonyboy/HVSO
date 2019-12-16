using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ScenarioExporter : MonoBehaviour {
    string humanChapterDataPath, orcChapterDataPath;
    ScenarioManager scenarioManager;

    void Start() {
        scenarioManager = ScenarioManager.Instance;
        humanChapterDataPath = "/Script/ETC/Data/HumanChapterDatas.json";
        orcChapterDataPath = "/Script/ETC/Data/OrcChapterDatas.json";

        SaveData();
    }

    public void SaveData() {
        string dataAsJson = JsonConvert.SerializeObject(scenarioManager.human_chapterDatas);
        string filePath = Application.dataPath + humanChapterDataPath;
        File.WriteAllText(filePath, dataAsJson);
        //Logger.Log("Human Chapter Data Exported");

        dataAsJson = JsonConvert.SerializeObject(scenarioManager.orc_chapterDatas);
        filePath = Application.dataPath + orcChapterDataPath;
        File.WriteAllText(filePath, dataAsJson);
        //Logger.Log("Orc Chapter Data Exported");
    }
}
