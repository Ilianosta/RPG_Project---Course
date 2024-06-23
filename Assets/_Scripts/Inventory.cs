using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject sword;
    public GameObject shield;
    public GameObject crossbow;
    public GameObject bomb;
    public List<Items> weapons = new List<Items>();
    public List<Items> items = new List<Items>();
    public int arrows, potions, bombs;
    public bool swordUse;
    private Animator anim;
    PlayerCombat playerCombat;
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    public void SwordActive(Items item)
    {
        Reset();
        sword.SetActive(true);

        anim.SetFloat("WeaponN", 0);

        if (!swordUse)
        {
            swordUse = true;
            anim.SetBool("Weapon", true);
            playerCombat.enabled = true;
        }

        anim.SetTrigger("SwitchWeapon");
        playerCombat.actualWeapon = item;
        sword.GetComponent<SwordCollision>().attack = playerCombat.actualWeapon.points;
    }

    public void ShieldActive(Items item)
    {
        Reset();
        sword.SetActive(true);
        shield.SetActive(true);

        anim.SetFloat("WeaponN", 1);
        anim.SetTrigger("SwitchWeapon");
        playerCombat.actualWeapon = item;
    }

    public void CrossbowActive(Items item)
    {
        Reset();
        crossbow.SetActive(true);

        anim.SetFloat("WeaponN", 2);
        anim.SetTrigger("SwitchWeapon");
        playerCombat.actualWeapon = item;
    }

    private void Reset()
    {
        sword.SetActive(false);
        shield.SetActive(false);
        crossbow.SetActive(false);
    }
}
