using System;
using System.Collections.Generic;
using UnityEngine;

using SA.Foundation.Templates;

namespace SA.iOS.ReplayKit
{
    [Serializable]
    public class ISN_PRPreivewResult : SA_Result
    {
        [SerializeField] List<string> m_activityTypes;

        /// <summary>
        /// A set of activity types as listed in UIActivity.
        /// </summary>
        /// <value>The activity types.</value>
        public List<string> ActivityTypes {
            get {
                return m_activityTypes;
            }
        }
    }
}