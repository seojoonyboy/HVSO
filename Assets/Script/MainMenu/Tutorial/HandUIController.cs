using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUIController : MonoBehaviour {
    [SerializeField] List<GameObject> pools;
    Dictionary<string, GameObject> usedPools;
    // Start is called before the first frame update
    void Start() {
        pools = new List<GameObject>();
        usedPools = new Dictionary<string, GameObject>();

        foreach(Transform ui in transform) {
            pools.Add(ui.gameObject);
        }
    }

    public GameObject ActiveHand(RectTransform rect, string key) {
        var available_pools = pools.FindAll(x => !x.activeSelf);
        if (available_pools == null) return null;

        usedPools[key] = available_pools[0];
        available_pools[0].gameObject.SetActive(true);
        switch (key) {
            case "storyButton":
            case "orc_story_tutorial":
            case "battle_button":
                available_pools[0].GetComponent<RectTransform>().localPosition = new Vector3(rect.localPosition.x + 200, rect.localPosition.y - 120, rect.localPosition.z);
                break;
            case "story_orc_button":
            case "tutorial_play_button":
            case "ai_battle_start_button":
            case "reward_box":
                available_pools[0].GetComponent<RectTransform>().localPosition = new Vector3(rect.localPosition.x + 100, rect.localPosition.y, rect.localPosition.z);
                break;
            default:
                available_pools[0].GetComponent<RectTransform>().localPosition = rect.localPosition;
                break;
        }
        if (key.Contains("deck_")) {
            available_pools[0].GetComponent<RectTransform>().localPosition = new Vector3(rect.localPosition.x + 200, rect.localPosition.y - 120, rect.localPosition.z);
        }
        return available_pools[0];
    }

    public void DeactiveHand(string key) {
        if (usedPools.ContainsKey(key)) {
            usedPools[key].GetComponent<RectTransform>().localPosition = Vector2.zero;
            usedPools[key].SetActive(false);
            usedPools.Remove(key);
        }
    }
}
