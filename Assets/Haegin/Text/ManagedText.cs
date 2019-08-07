using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Haegin;

[RequireComponent(typeof(Text))]
public class ManagedText : MonoBehaviour
{
    [HideInInspector]
    public string stringTagString;

    private TextManager.StringTag stringTag;

    [ExposeProperty]
    public TextManager.StringTag Tag {
        get { 
            try
            {
                stringTag = (TextManager.StringTag)System.Enum.Parse(typeof(TextManager.StringTag), stringTagString);
            }
            catch
            {
                stringTag = TextManager.StringTag.Max;
            }
            return stringTag; 
        }
        set { 
            stringTag = value; 
            gameObject.GetComponent<Text>().text = TextManager.GetString(stringTag); 
            stringTagString = stringTag.ToString(); 
        }
    }

    void Awake()
    {
        gameObject.GetComponent<Text>().text = TextManager.GetString(Tag);
    }

#if UNITY_EDITOR
    void Reset()
    {
        gameObject.GetComponent<Text>().text = TextManager.GetString(stringTag);
    }

    [MenuItem("GameObject/UI/ManagedText")]
    static GameObject Create()
    {
        GameObject obj = new GameObject("ManagedText");
        obj.AddComponent<ManagedText>();
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(160.0f, 30.0f);
        obj.transform.SetParent(Selection.activeGameObject.transform);
        Selection.activeGameObject = obj;
        return obj;
    }
#endif
}
