using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class IngameResourceLibrary : SerializedMonoBehaviour {

    public static IngameResourceLibrary gameResource { get; private set; }

    public Dictionary<string, GameObject> unitSkeleton;
    public Dictionary<string, GameObject> cardPreveiwSkeleton;
    public Dictionary<string, GameObject> toolObject;
    public Dictionary<string, GameObject> tutorialExample;

    public GameObject deadObject;
    public GameObject hideObject;


    // Start is called before the first frame update
    void Start()
    {
        gameResource = this;
    }

    private void OnDestroy() {
        gameResource = null;
    }

}
