using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardDescriptionHandler : MonoBehaviour {
    Dictionary<string, Description> descriptionDict;
    public GameObject rewordDescModal;

    private static RewardDescriptionHandler m_instance;
    public static RewardDescriptionHandler instance {
        get {
            if (m_instance == null) m_instance = FindObjectOfType<RewardDescriptionHandler>();

            return m_instance;
        }
    }

    void Start() {
        ReadFile();
    }

    public void ReadFile() {
        string dataAsJson = ((TextAsset)Resources.Load("RewardDescription")).text;
        var formattedData = JsonReader.Read<List<Description>>(dataAsJson);

        descriptionDict = new Dictionary<string, Description>();

        foreach(Description desc in formattedData) {
            descriptionDict.Add(desc.key, desc);
        }
    }

    public void RequestDescriptionModal(string _keyword) {
        Description description = GetDescription(_keyword);
        if (description == null) return;

        GameObject modal = Instantiate(rewordDescModal);
        modal
            .GetComponent<Button>()
            .onClick.AddListener(() => { Destroy(modal); });
        modal
            .transform
            .Find("InnerModal/Content/Button")
            .GetComponent<Button>()
            .onClick.AddListener(() => { Destroy(modal); });

        //Logger.Log(description.name);
        Transform content = modal.transform.Find("InnerModal/Content");
        content.Find("Header").GetComponent<TextMeshProUGUI>().text = description.name;
        content.Find("Description").GetComponent<TextMeshProUGUI>().text = description.description;
        modal.transform.Find("InnerModal/Slot/Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[description.key];
    }

    public Description GetDescription(string _keyword) {
        string keyword = string.Empty;

        if (_keyword.Contains("card")) {
            keyword = "card";
        }
        else if (_keyword.Contains("gold")) {
            keyword = "goldFree";
        }
        else {
            keyword = _keyword;
        }

        if (!descriptionDict.ContainsKey(keyword)) return null;
        return descriptionDict[keyword];
    }

    public class Description {
        public string key;
        public string name;
        public string description;
    }
}
