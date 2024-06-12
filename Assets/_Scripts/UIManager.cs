using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject notification;
    public GameObject interact;
    private void Awake()
    {
        if (UIManager.instance) Destroy(this);
        else UIManager.instance = this;
    }

    public void ShowInteract()
    {
        interact.SetActive(true);
    }

    public void HideInteract()
    {
        interact.SetActive(false);
    }

    public void ShowNotification(string msg)
    {
        if (!notification.activeSelf) notification.SetActive(true);

        TextMeshProUGUI tmpro = notification.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        tmpro.text = "";
        notification.GetComponent<RectTransform>().DOSizeDelta(new Vector2(720, 200), .2f).OnComplete(() =>
        {
            tmpro.text = msg;
        });
    }

    public void HideNotification()
    {
        TextMeshProUGUI tmpro = notification.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        notification.GetComponent<RectTransform>().DOSizeDelta(new Vector2(720, 0), .2f).OnComplete(() =>
        {
            tmpro.text = "";
            notification.SetActive(false);
        });
    }
}

