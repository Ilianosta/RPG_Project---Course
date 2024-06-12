using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Items actualWeapon;
    public Items actualItem;
    public CapsuleCollider swordCollision;
    public LayerMask enemyMask;
    public float focusAtkImpulse;
    public float combo;
    public bool isAttacking;
    PlayerMotion playerMotion;
    Inventory inventory;
    zTarget zTarget;
    Animator anim;
    Rigidbody rb;
    bool heavyAtk;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
        inventory = GetComponent<Inventory>();
        zTarget = GetComponent<zTarget>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void OnLightAttack()
    {
        if (!actualWeapon) return;

        if (!isAttacking && playerMotion.Attack())
        {
            isAttacking = true;
            Reset();

            rb.velocity = Vector3.zero;
            playerMotion.Stopping();

            anim.SetFloat("Combo", combo);
            anim.SetInteger("Attack", 1);
            anim.SetTrigger("Atk");

            if (combo == 2) combo = 0;
            else combo++;

            if (playerMotion.targetPlayer)
            {
                if (Vector3.Distance(transform.position, playerMotion.targetPlayer.position) > 1f)
                {
                    rb.AddForce(playerMotion.cam.forward * focusAtkImpulse, ForceMode.Impulse);
                }
            }
            StartCoroutine("MoveAgain");
            StartCoroutine("ComboEnd");
        }
    }

    public void OnHeavyAttack()
    {
        if (!actualWeapon) return;

        if (!isAttacking && playerMotion.Attack())
        {
            isAttacking = true;
            Reset();

            rb.velocity = Vector3.zero;
            playerMotion.Stopping();

            heavyAtk = true;
            anim.SetInteger("Attack", 2);
            anim.SetTrigger("Atk");

            if (playerMotion.targetPlayer)
            {
                if (Vector3.Distance(transform.position, playerMotion.targetPlayer.position) > 1f)
                {
                    rb.AddForce(playerMotion.cam.forward * focusAtkImpulse, ForceMode.Impulse);
                }
            }
            StartCoroutine("MoveAgain");
            StartCoroutine("ComboEnd");
        }
    }

    public void Hit()
    {
        swordCollision.enabled = true;
        Reset();

        StartCoroutine("MoveAgain", (heavyAtk) ? .8f : .5f);
        StartCoroutine("ComboEnd");
    }

    IEnumerator ComboEnd()
    {
        yield return new WaitForSeconds(1.5f);
        combo = 0;
    }

    IEnumerator MoveAgain(float f = .5f)
    {
        yield return new WaitForSeconds(f);
        heavyAtk = false;
        anim.SetInteger("Attack", 0);

        swordCollision.enabled = false;

        isAttacking = false;
        playerMotion.StopEnd();
    }

    public void Reset()
    {
        StopCoroutine("MoveAgain");
        StopCoroutine("ComboEnd");
    }
}
