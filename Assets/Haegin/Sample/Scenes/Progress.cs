using UnityEngine;
using UnityEngine.UI;

namespace Haegin
{
    public class Progress : MonoBehaviour
    {

        Image foregroundImage;

        public int Value
        {
            get
            {
                if (foregroundImage != null)
                    return (int)(foregroundImage.fillAmount * 100);
                else
                    return 0;
            }
            set
            {
                if (foregroundImage != null)
                    foregroundImage.fillAmount = value / 100f;
            }
        }

        void Start()
        {
            foregroundImage = gameObject.GetComponent<Image>();
            Value = 0;
        }
    }
}
