using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[ShowOdinSerializedPropertiesInInspector]
public class RewardsProvider : SerializedMonoBehaviour {
    const string path = "BattleReady/MMR_rewards_table";
    [SerializeField] GameObject RewardButtonPool;
    [SerializeField] Transform content;
    public Dictionary<string, Sprite> rewardIcons;

    void Start() {

    }

    public void Provide() {
        Clear();

        var rewards = Read();
        Separate(ref rewards);
    }

    void Clear() {
        foreach(Transform tf in content) {
            if (tf.name.Contains("Slider")) continue;
            Destroy(tf.gameObject);
        }
    }

    Rewards Read() {
        string dataAsJson = ((TextAsset)Resources.Load(path)).text;
        Logger.Log(dataAsJson);
        var rewards = JsonReader.Read<Rewards>(dataAsJson);
        return rewards;
    }

    void Separate(ref Rewards data) {
        List<Reward> rewards = data.rewards;
        foreach(Reward reward in rewards) {
            GameObject btn = Instantiate(RewardButtonPool);

            TextMeshProUGUI amountTxt = btn.transform.Find("Amount").GetComponent<TextMeshProUGUI>();
            amountTxt.text = reward.amount.ToString();
            Image rewardIcon = btn.transform.Find("Image").GetComponent<Image>();
            rewardIcon.sprite = GetRewardIcon(reward.type, reward.args);
            Image checkMark = btn.transform.Find("CheckMark").GetComponent<Image>();

            btn.gameObject.SetActive(true);
            btn.transform.SetParent(content);
        }
    }

    Sprite GetRewardIcon(string keyword, string[] args = null) {
        if (keyword.Contains("Card")) {
            if (args == null) return null;

            return rewardIcons[args[0]];
        }
        else {
            if (rewardIcons.ContainsKey(keyword)) {
                return rewardIcons[keyword];
            }
            else {
                return null;
            }
        }
    }

    public class Rewards {
        public int maxSliderNum;
        public List<Reward> rewards;
    }

    public class Reward {
        public int standard;
        public string type;
        public int amount;
        public string[] args;
    }
}
