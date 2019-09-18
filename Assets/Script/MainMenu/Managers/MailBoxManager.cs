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
        transform.Find("OpenedMail").gameObject.SetActive(true);
    }

    public void CloseMail() {
        transform.Find("OpenedMail").gameObject.SetActive(false);
    }
}
