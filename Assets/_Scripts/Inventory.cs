using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject sword;
    public List<Items> weapons = new List<Items>();
    public List<Items> items = new List<Items>();
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
}
