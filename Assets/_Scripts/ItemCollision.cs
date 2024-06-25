using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemCollision : MonoBehaviour
{
    public GameObject item;
    public Items drop;
    public WeaponType weaponType;
    public int arrows, potions, bombs;
    public bool fireMagic;
    public bool open;
    public string notificationText;
    public Transform upPoint;
    private GameObject player;
    private PlayerMotion playerMotion;
    private Inventory inventory;
    private Animator anim;
    private PlayerMotion _PlayerMotion
    {
        get
        {
            if (!player) return null;
            if (!playerMotion) playerMotion = player.GetComponent<PlayerMotion>();
            return playerMotion;
        }
    }
    private Inventory _Inventory
    {
        get
        {
            if (!player) return null;
            if (!inventory) inventory = player.GetComponent<Inventory>();
            return inventory;
        }
    }
    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (drop)
        {
            weaponType = drop.type;
            notificationText = drop.msg;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !open)
        {
            player = other.gameObject;
            _PlayerMotion.chest = this;
            UIManager.instance.ShowInteract();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !open)
        {
            player = other.gameObject;
            _PlayerMotion.chest = null;
            UIManager.instance.HideInteract();
        }
    }

    public void Open()
    {
        if (open) return;

        open = true;
        _PlayerMotion.interacting = true;
        _PlayerMotion.Stopping();

        UIManager.instance.HideInteract();
        anim.enabled = true;
        StartCoroutine("Finish");
    }

    IEnumerator Finish()
    {
        yield return new WaitForSeconds(2);
        _PlayerMotion.SelectTarget(upPoint);
        UIManager.instance.ShowNotification(notificationText);

        yield return new WaitForSeconds(.2f);
        item.SetActive(true);

        yield return new WaitForSeconds(2);
        UIManager.instance.HideNotification();
        item.SetActive(false);

        _PlayerMotion.chest = null;
        _PlayerMotion.interacting = false;
        _PlayerMotion.StopEnd();

        if (fireMagic)
        {
            _PlayerMotion.GetPlayerCombat.fireExist = fireMagic;
            UIManager.instance.ShowFire();
        }

        if (arrows != 0)
        {
            _Inventory.arrows += arrows;
            UIManager.instance.ShowIcons();
            UIManager.instance.UpdateArrows(_Inventory.arrows);
        }

        switch (weaponType)
        {
            case WeaponType.sword:
                _Inventory.SwordActive(drop);
                _Inventory.weapons.Add(drop);
                break;
            case WeaponType.shield:
                _Inventory.ShieldActive(drop);
                _Inventory.weapons.Add(drop);
                break;
            case WeaponType.crossbow:
                _Inventory.CrossbowActive(drop);
                _Inventory.weapons.Add(drop);
                break;
            case WeaponType.heal:
                if (_Inventory.items.Where(i => i == drop).Count() == 0)
                {
                    UIManager.instance.ShowPotion();
                    _Inventory.items.Add(drop);
                    if (_PlayerMotion.GetPlayerCombat.actualItem == null)
                    {
                        _PlayerMotion.GetPlayerCombat.actualItem = _Inventory.items[0];
                    }
                }
                _Inventory.potions += potions;
                UIManager.instance.ShowIcons();
                UIManager.instance.UpdatePotions(_Inventory.potions);
                break;
            case WeaponType.bomb:
                if (_Inventory.items.Where(i => i == drop).Count() == 0)
                {
                    UIManager.instance.ShowBomb();
                    _Inventory.items.Add(drop);
                    if (_PlayerMotion.GetPlayerCombat.actualItem == null)
                    {
                        _PlayerMotion.GetPlayerCombat.actualItem = _Inventory.items[0];
                    }
                }
                _Inventory.bombs += bombs;
                UIManager.instance.ShowIcons();
                UIManager.instance.UpdateBombs(_Inventory.bombs);
                break;
            default:
                break;
        }

        _PlayerMotion.NoTarget();
        player = null;
    }
}
