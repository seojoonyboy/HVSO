using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChallengeExporter : MonoBehaviour {
    string humanChallengeDataPath, orcChallengeDataPath;
    ScenarioManager scenarioManager;

    void Start() {
        scenarioManager = ScenarioManager.Instance;

        humanChallengeDataPath = "/Script/ETC/Data/humanChallengeData.json";
        orcChallengeDataPath = "/Script/ETC/Data/orcChallengeData.json";

        SaveData();
    }

    public void SaveData() {
        string dataAsJson = JsonConvert.SerializeObject(scenarioManager.human_challengeDatas);
        string filePath = Application.dataPath + humanChallengeDataPath;
        File.WriteAllText(filePath, dataAsJson);
        //Logger.Log("Human Challenge Data Exported");

        dataAsJson = JsonConvert.SerializeObject(scenarioManager.orc_challengeDatas);
        filePath = Application.dataPath + orcChallengeDataPath;
        File.WriteAllText(filePath, dataAsJson);
        //Logger.Log("Orc Challenge Data Exported");
    }
}
