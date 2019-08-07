using UnityEngine;
using UnityEditor;
using Haegin;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ManagedTextMeshProText : MonoBehaviour {
    [HideInInspector]
    public string stringTagString;

    private TextManager.StringTag stringTag;

    [ExposeProperty]
    public TextManager.StringTag Tag {
        get {
            try {
                stringTag = (TextManager.StringTag)System.Enum.Parse(typeof(TextManager.StringTag), stringTagString);
            }
            catch {
                stringTag = TextManager.StringTag.Max;
            }
            return stringTag;
        }
        set {
            stringTag = value;
            gameObject.GetComponent<TextMeshProUGUI>().text = TextManager.GetString(stringTag);
            stringTagString = stringTag.ToString();
        }
    }

    void Awake() {
        gameObject.GetComponent<TextMeshProUGUI>().text = TextManager.GetString(Tag);
    }

#if UNITY_EDITOR
    void Reset() {
        gameObject.GetComponent<TextMeshProUGUI>().text = TextManager.GetString(stringTag);
    }

    [MenuItem("GameObject/UI/Managed_TMPRO_Text")]
    static GameObject Create() {
        GameObject obj = new GameObject("Managed_TMPRO_Text");
        obj.AddComponent<ManagedTextMeshProText>();
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(160.0f, 30.0f);
        obj.transform.SetParent(Selection.activeGameObject.transform);
        Selection.activeGameObject = obj;
        return obj;
    }
#endif
}