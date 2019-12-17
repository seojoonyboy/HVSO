using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[ShowOdinSerializedPropertiesInInspector]
public class RewardsProvider : SerializedMonoBehaviour {
    const string path = "BattleReady/MMR_rewards_table";
    [SerializeField] GameObject RewardButtonPool;
    [SerializeField] Transform content;
    public List<GameObject> buttons;
    public List<int> standards;
    Dictionary<string, Sprite> rewardIcons;
    private void Awake() {
        rewardIcons = AccountManager.Instance.resource.rewardIcon;
    }

    public void Provide() {
        Clear();

        var rewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        Separate(ref rewards);
    }

    void Clear() {
        foreach(Transform tf in content) {
            if (tf.name.Contains("Bar")) continue;
            Destroy(tf.gameObject);
        }
    }

    void Separate(ref List<AccountManager.Reward> rewards) {
        buttons = new List<GameObject>();
        standards = new List<int>();

        foreach (AccountManager.Reward reward in rewards) {
            GameObject btn = Instantiate(RewardButtonPool);

            TextMeshProUGUI amountTxt = btn.transform.Find("Amount").GetComponent<TextMeshProUGUI>();
            amountTxt.text = reward.point.ToString();
            Image rewardIcon = btn.transform.Find("Image/RewardIcon").GetComponent<Image>();
            rewardIcon.sprite = GetRewardIcon(reward.reward.kind);
            Image checkMark = btn.transform.Find("CheckMark").GetComponent<Image>();

            btn.gameObject.SetActive(true);
            btn.transform.SetParent(content);
            btn.transform.Find("MMR").GetComponent<IntergerIndex>().Id = reward.id;
            btn.GetComponent<RewardButtonInBattleReady>().SetRewardData(reward);

            standards.Add(reward.point);

            buttons.Add(btn);
        }

        GetComponent<RewardProgressController>().OnRewardObjectSettingFinished();
    }

    Sprite GetRewardIcon(string keyword) {
        if (rewardIcons.ContainsKey(keyword)) {
            return rewardIcons[keyword];
        }
        else {
            return null;
        }
    }
}
