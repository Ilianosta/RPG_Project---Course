using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public bool isAttacking;
    public LayerMask playerLayer;
    public Transform handL, handR;
    public float handArea;
    public int atkDamage1, atkDamage2;
    public int attackN, attackC;
    EnemyMotion enemyMotion;
    Animator anim;
    private void Awake()
    {
        enemyMotion = GetComponent<EnemyMotion>();
        anim = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        attackN = Random.Range(0, 2);

        anim.SetInteger("Attack", attackN);
        anim.SetTrigger("Atk");
    }

    public void Hit()
    {
        if (attackN == 0) Combo1();
        else Combo2();
    }

    void Combo1()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(handL.position, handArea, playerLayer);

        if (rangeChecks.Length > 0)
        {
            RaycastHit hit;
            Physics.Raycast(enemyMotion.pointOfView.position, enemyMotion.pointOfView.forward, out hit, 1, playerLayer);
            if (hit.collider.CompareTag("Shield"))
            {
                hit.collider.GetComponentInParent<PlayerCombat>().Block();
            }
            else
            {
                hit.collider.GetComponent<PlayerLife>().GetHit(atkDamage1);
            }
        }

        Sequence s = DOTween.Sequence();
        s.AppendInterval(1.5f).OnComplete(() =>
        {
            isAttacking = false;
            enemyMotion.StopEnd();
        });
    }
    void Combo2()
    {
        Vector3 hand = (attackC == 0) ? handL.position : handR.position;
        int damage = (attackC == 0) ? atkDamage1 : atkDamage2;

        Collider[] rangeChecks = Physics.OverlapSphere(hand, handArea, playerLayer);

        if (rangeChecks.Length > 0)
        {
            RaycastHit hit;
            Physics.Raycast(enemyMotion.pointOfView.position, enemyMotion.pointOfView.forward, out hit, 1, playerLayer);
            if (hit.collider.CompareTag("Shield"))
            {
                hit.collider.GetComponentInParent<PlayerCombat>().Block();
            }
            else
            {
                hit.collider.GetComponent<PlayerLife>().GetHit(damage);
            }
        }
        if (attackC == 1)
        {
            Sequence s = DOTween.Sequence();
            s.AppendInterval(1.5f).OnComplete(() =>
            {
                attackC = 0;
                isAttacking = false;
                enemyMotion.StopEnd();
            });
        }
        attackC++;
    }
}
