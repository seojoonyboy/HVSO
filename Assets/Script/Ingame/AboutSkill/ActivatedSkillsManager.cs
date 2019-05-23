using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Set Instance
/// </summary>
public partial class ActivatedSkillsManager : SerializedBehaviour {
    ActivatedSkillsManager _instance;
    public ActivatedSkillsManager Instance {
        get {
            return _instance;
        }
        private set {
            _instance = value;
        }
    }

    void Awake() {
        Instance = GetComponent<ActivatedSkillsManager>();
    }
}

/// <summary>
/// Managing all Skills, except immidiate skill
/// </summary>
public partial class ActivatedSkillsManager {
    public List<Format> activatedSkillList;
    void Start() {

    }

    public void AddSkill() {

    }

    public void RemoveSkill(string keyword) {

    }
}

public partial class ActivatedSkillsManager {
    /// <summary>
    /// For Test
    /// </summary>
    public class Format {
        public string[] method;
        public string[] args;
    }


}