using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Quest;
using Spine.Unity;
using UnityEngine.UI;

public class DailyQuestAlarmHandler : MonoBehaviour {
    [SerializeField] Transform content;

    void OnDestroy() {
        StopAllCoroutines();
    }

    public void ShowQuestList(List<QuestData> quests) {
        SkeletonGraphic skeletonGraphic = transform.Find("InnerCanvas/background/Spine").GetComponent<SkeletonGraphic>();
        skeletonGraphic.Initialize(false);
        skeletonGraphic.Update(0);
        skeletonGraphic.AnimationState.SetAnimation(0, "NOMAL", false);

        Clear();
        StartCoroutine(_showQuestList(quests));
    }

    IEnumerator _showQuestList(List<QuestData> quests) {
        yield return new WaitForSeconds(1.0f);  //anim 대기
        foreach (QuestData data in quests) {
            GameObject _obj = getPool();
            _obj.gameObject.SetActive(true);

            SetData(_obj, data);
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(0.5f);
        StartGlowEffect();
    }

    private void Clear() {
        foreach (Transform child in content) {
            var texts = child.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts) {
                text.text = string.Empty;
            }
            Transform rewardList = child.Find("RewardList");
            foreach (Transform slot in rewardList) {
                slot.gameObject.SetActive(false);
            }
            child.Find("FrameLightEffect").gameObject.SetActive(false);
            child.gameObject.SetActive(false);
        }
    }

    private void SetData(GameObject obj, QuestData data) {
        TextMeshProUGUI header = obj.transform.Find("Header").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI description = obj.transform.Find("Description").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI progress = obj.transform.Find("Progress").GetComponent<TextMeshProUGUI>();

        Transform rewardList = obj.transform.Find("RewardList");
        var rewards = data.questDetail.rewards;
        for(int i=0; i<rewards.Length; i++) {
            Transform slot = rewardList.GetChild(i);
            slot.gameObject.SetActive(true);
            slot.Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[rewards[i].kind];
            slot.Find("Amount").GetComponent<TextMeshProUGUI>().text = rewards[i].amount.ToString();
        }
        header.text = data.questDetail.name;
        description.text = data.questDetail.desc;
        progress.text = "0/" + data.questDetail.progMax;
    }

    private GameObject getPool() {
        foreach(Transform pool in content) {
            if (!pool.gameObject.activeSelf) {
                return pool.gameObject;
            }
        }
        return null;
    }

    private void StartGlowEffect() {
        foreach(Transform item in content) {
            if (item.gameObject.activeSelf) {
                item.Find("FrameLightEffect").gameObject.SetActive(true);
            }
        }
    }
}
