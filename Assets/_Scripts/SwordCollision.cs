using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    public int attack;
    public PlayerCombat playerCombat;
    CinemachineImpulseSource cinemachineImpulse;
    private void Awake()
    {
        cinemachineImpulse = GetComponent<CinemachineImpulseSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            TargetDamage target = other.transform.GetComponent<TargetDamage>();
            if (target)
            {
                cinemachineImpulse.GenerateImpulse(Camera.main.transform.forward);
                if (target.player) target.player = playerCombat.gameObject;

                target.Damage(attack);
            }
        }
    }
}
