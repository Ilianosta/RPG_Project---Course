using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject notification;
    public GameObject interact;
    public GameObject icons;
    public TextMeshProUGUI potionsTxt;
    public TextMeshProUGUI bombsTxt;
    public TextMeshProUGUI arrowsTxt;
    public Image bombIcon, starBomb;
    public Image potionIcon, starPotion;
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

        TextMeshProUGUI tmpro = notification.GetComponentInChildren<TextMeshProUGUI>();
        tmpro.text = "";
        notification.GetComponent<RectTransform>().DOSizeDelta(new Vector2(720, 200), .2f).OnComplete(() =>
        {
            tmpro.text = msg;
        });
    }

    public void HideNotification()
    {
        TextMeshProUGUI tmpro = notification.GetComponentInChildren<TextMeshProUGUI>();
        notification.GetComponent<RectTransform>().DOSizeDelta(new Vector2(720, 0), .2f).OnComplete(() =>
        {
            tmpro.text = "";
            notification.SetActive(false);
        });
    }

    public void ShowIcons()
    {
        if (!icons.activeSelf)
        {
            icons.SetActive(true);
            potionsTxt.text = "0";
            bombsTxt.text = "0";
            arrowsTxt.text = "0";
        }
    }

    public void UpdatePotions(int n)
    {
        potionsTxt.text = n.ToString();
    }
    public void UpdateBombs(int n)
    {
        bombsTxt.text = n.ToString();
    }
    public void UpdateArrows(int n)
    {
        arrowsTxt.text = n.ToString();
    }

    public void ShowBomb()
    {
        bombIcon.gameObject.SetActive(true);
        if (potionIcon.gameObject.activeSelf)
        {
            bombIcon.DOFade(.5f, 0);
        }
        else
        {
            starBomb.gameObject.SetActive(true);
        }
    }
    public void ShowPotion()
    {
        potionIcon.gameObject.SetActive(true);
        if (bombIcon.gameObject.activeSelf)
        {
            potionIcon.DOFade(.5f, 0);
        }
        else
        {
            starPotion.gameObject.SetActive(true);
        }
    }

    public void ItemSelect(Items item)
    {
        if (item.type == WeaponType.heal)
        {
            starBomb.gameObject.SetActive(false);
            starPotion.gameObject.SetActive(true);
            potionIcon.DOFade(1, 0);
            bombIcon.DOFade(.5f, 0);
        }
        else
        {
            starBomb.gameObject.SetActive(true);
            starPotion.gameObject.SetActive(false);
            bombIcon.DOFade(1, 0);
            potionIcon.DOFade(.5f, 0);
        }
    }
}

