using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryArrowHandler : MonoBehaviour {
    [SerializeField] private ScenarioManager _scenarioManager;
    public ArrowType arrowType;
    private int MaxPageIndex = 0;
    
    public void Init() {
        _scenarioManager.OnChapterChanged.AddListener(OnArrowClicked);
    }

    private void OnDestroy() {
        _scenarioManager.OnChapterChanged.RemoveListener(OnArrowClicked);
    }

    public void OnArrowClicked(bool isHuman, int chapter) {
        MaxPageIndex = _scenarioManager.maxPageIndex;
        
        gameObject.SetActive(true);
        if (chapter == 0) {
            if (arrowType == ArrowType.LEFT) {
                gameObject.SetActive(false);
            }
        }
        else if (MaxPageIndex == chapter) {
            if (arrowType == ArrowType.RIGHT) {
                gameObject.SetActive(false);
            }
        }
    }
    
    public enum ArrowType {
        LEFT,
        RIGHT
    }
}