using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    PlayerMotion playerMotion;
    PlayerCombat playerCombat;
    private void Awake()
    {
        playerMotion = GetComponentInParent<PlayerMotion>();
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    public void Land()
    {
        playerMotion.FallEnd();
    }

    public void RollStop()
    {
        playerMotion.RollStop();
    }

    public void Hit()
    {
        playerCombat.Hit();
    }

    public void Shoot()
    {
        playerCombat.Shoot();
    }

    public void HealEnd()
    {
        playerCombat.HealEnd();
    }
}
