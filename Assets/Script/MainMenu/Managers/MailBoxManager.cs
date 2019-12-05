using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailBoxManager : MonoBehaviour
{
    public void OpenMailBox() {
        gameObject.SetActive(true);
    }

    public void CloseMailBox() {
        gameObject.SetActive(false);
    }

    public void OpenMail() {
        transform.GetChild(0).Find("OpenedMail").gameObject.SetActive(true);
    }

    public void CloseMail() {
        transform.GetChild(0).Find("OpenedMail").gameObject.SetActive(false);
    }
}
