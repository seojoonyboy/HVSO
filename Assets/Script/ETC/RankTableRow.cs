using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace dataModules {
    public class RankTableRow : MonoBehaviour {
        public AccountManager.RankTableRow data;

        public Text mmr;
        public TextMeshProUGUI minorRankName;
        public Image upperLine, middleLine;
        public Image background;
        public GameObject myLeagueMark;
    }
}
