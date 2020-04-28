using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MainWindowBase : MonoBehaviour {
    protected string pageName;
    [SerializeField] private HorizontalScrollSnap hss;
    
    public virtual void OnChangePageEvent() {
        if (hss.CurrentPageObject().name.Equals(pageName)) {
            OnPageLoaded();
        }
    }
    
    public virtual void OnPageLoaded() {
        
    }
}