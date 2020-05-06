using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//using TreeEditor;
using UnityEngine;

public class ChapterAlertHandlerIngame : MonoBehaviour {
    private Dictionary<int, ChapterData> _dictionary;
    private void Start() {
        MakeTree();
    }

    public void RequestChangeChapterAlert(string camp, int chapterNum, int stageNum) {
        var clearedStages = AccountManager.Instance.clearedStages;
        bool isAlreadyCleared = clearedStages.Exists(x =>
            x.camp == camp && x.chapterNumber == chapterNum && x.stageNumber == stageNum);
                
        if(isAlreadyCleared) return;

        ChapterData selectedChapterData = null;
        foreach (var pair in _dictionary) {
            if (pair.Value.camp == camp && pair.Value.chapterNum == chapterNum && pair.Value.stageNum == stageNum) {
                selectedChapterData = pair.Value;
            }
        }
        if(selectedChapterData == null) return;

        List<int> next = selectedChapterData.next;
                
        foreach (var val in next) {
            var _key = MakeKey(_dictionary[val].camp, _dictionary[val].chapterNum, _dictionary[val].stageNum);
            dataModules.AlertWriter.AddNewConditionKey(_key);

            var _key2 = NewAlertManager.ButtonName.CHAPTER;
            dataModules.AlertWriter.AddNewKey(_key2.ToString());
        }

        var __key = MakeKey(
            selectedChapterData.camp, 
            selectedChapterData.chapterNum,
            selectedChapterData.stageNum
        );
        
        dataModules.AlertWriter.RemoveKey(__key);
    }

    private string MakeKey(string camp, int chapter, int stage) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(NewAlertManager.ButtonName.CHAPTER)
            .Append("_")
            .Append(camp)
            .Append("_")
            .Append(chapter)
            .Append("_")
            .Append(stage);
        
        return sb.ToString();
    }

    private void MakeTree() {
        _dictionary = new Dictionary<int, ChapterData>();
        
        _dictionary.Add(0, new ChapterData(id: 0, camp: "orc", chapterNum: 0, stageNum: 2));
        _dictionary[0].next = new List<int>(){ 1, 2 };
        
        _dictionary.Add(1, new ChapterData(id: 1, chapterNum: 1, stageNum: 1, camp: "human"));
        _dictionary[1].next = new List<int>(){ 3 };
        _dictionary[1].prev = new List<int>(){ 0 };
        
        _dictionary.Add(2, new ChapterData(id: 2, chapterNum: 1, stageNum: 1, camp: "orc"));
        _dictionary[2].next = new List<int>(){ 4 };
        _dictionary[2].prev = new List<int>(){ 0 };
        
        _dictionary.Add(3, new ChapterData(id: 3, chapterNum: 1, stageNum: 2, camp: "human"));
        _dictionary[3].next = new List<int>(){ 5 };
        _dictionary[3].prev = new List<int>(){ 1 };
        
        _dictionary.Add(4, new ChapterData(id: 4, chapterNum: 1, stageNum: 2, camp: "orc"));
        _dictionary[4].next = new List<int>(){ 6 };
        _dictionary[4].prev = new List<int>(){ 2 };
        
        _dictionary.Add(5, new ChapterData(id: 5, chapterNum: 1, stageNum: 3, camp: "human"));
        _dictionary[5].next = new List<int>(){ 7 };
        _dictionary[5].prev = new List<int>(){ 3 };
        
        _dictionary.Add(6, new ChapterData(id: 6, chapterNum: 1, stageNum: 3, camp: "orc"));
        _dictionary[6].next = new List<int>(){ 8 };
        _dictionary[6].prev = new List<int>(){ 4 };
        
        _dictionary.Add(7, new ChapterData(id: 7, chapterNum: 1, stageNum: 4, camp: "human"));
        _dictionary[7].prev = new List<int>(){ 5 };
        
        _dictionary.Add(8, new ChapterData(id: 8, chapterNum: 1, stageNum: 4, camp: "orc"));
        _dictionary[8].prev = new List<int>(){ 6 };
    }

    public class ChapterData {
        public int id;
        public string camp;
        public int chapterNum;
        public int stageNum;
        public List<int> next;
        public List<int> prev;
        
        public ChapterData(int id, string camp, int chapterNum, int stageNum) {
            this.id = id;
            this.camp = camp;
            this.chapterNum = chapterNum;
            this.stageNum = stageNum;
        }

        public void ConnectNext(int nextId) {
            if(next == null) next = new List<int>();
            next.Add(nextId);
        }

        public void ConnectPrev(int prevId) {
            if(prev == null) prev = new List<int>();
            prev.Add(prevId);
        }
    }
}