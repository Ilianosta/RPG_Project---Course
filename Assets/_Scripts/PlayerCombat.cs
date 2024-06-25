using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Items actualWeapon;
    public Items actualItem;
    public CapsuleCollider swordCollision;
    public GameObject shieldBox, shieldBoxParticle;
    public GameObject healParticle;
    public GameObject arrowPrefab;
    public GameObject fireballPrefab;
    public Transform attachPoint;
    public Transform forwardT;
    public LayerMask enemyMask;
    public float arrowSpeed;
    public float focusAtkImpulse;
    public float combo;
    public float fireSpeed, timeFire, fireCooldown;
    public bool fireExist, magicUse;
    public bool isAttacking;
    public bool shieldUse;
    public bool bombUse;
    PlayerMotion playerMotion;
    PlayerLife playerLife;
    Inventory inventory;
    zTarget zTarget;
    CinemachineImpulseSource cinemachineImpulseSource;
    Animator anim;
    Rigidbody rb;
    bool heavyAtk;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
        playerLife = GetComponent<PlayerLife>();
        inventory = GetComponent<Inventory>();
        zTarget = GetComponent<zTarget>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
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

            if (actualWeapon.type == WeaponType.sword || actualWeapon.type == WeaponType.shield) anim.SetInteger("Attack", 1);
            if (actualWeapon.type == WeaponType.crossbow) anim.SetInteger("Attack", 4);

            anim.SetTrigger("Atk");

            if (combo == 2) combo = 0;
            else combo++;

            if (playerMotion.focus && actualWeapon.type != WeaponType.crossbow)
            {
                if (playerMotion.targetPlayer)
                {
                    if (Vector3.Distance(transform.position, playerMotion.targetPlayer.position) > 1f)
                    {
                        rb.AddForce(playerMotion.cam.forward * focusAtkImpulse, ForceMode.Impulse);
                    }
                }
            }

            StartCoroutine("MoveAgain", 1f);
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

            if (actualWeapon.type == WeaponType.sword)
            {
                heavyAtk = true;
                anim.SetInteger("Attack", 2);
            }

            if (actualWeapon.type == WeaponType.shield) anim.SetInteger("Attack", 3);
            if (actualWeapon.type == WeaponType.crossbow) anim.SetInteger("Attack", 4);

            anim.SetTrigger("Atk");

            if (playerMotion.focus && actualWeapon.type != WeaponType.crossbow)
            {
                if (playerMotion.targetPlayer)
                {
                    if (Vector3.Distance(transform.position, playerMotion.targetPlayer.position) > 1f)
                    {
                        rb.AddForce(playerMotion.cam.forward * focusAtkImpulse, ForceMode.Impulse);
                    }
                }
            }

            StartCoroutine("MoveAgain", 1);
            StartCoroutine("ComboEnd");
        }
    }

    public void ShieldBlock()
    {
        if (!actualWeapon) return;

        if (actualWeapon.type == WeaponType.shield && playerMotion.Attack())
        {
            OnShieldUse(shieldUse);
        }
    }

    public void OnShieldUse(bool use)
    {
        if (!actualWeapon) return;

        anim.SetBool("ShieldUse", use);
        shieldBox.GetComponent<BoxCollider>().enabled = use;
    }

    public void OnShieldUse(InputValue input)
    {
        if (!actualWeapon) return;

        shieldUse = input.isPressed;
        ShieldBlock();
    }

    public void OnMagic()
    {
        if (actualWeapon == null || !fireExist || magicUse) return;

        if (!isAttacking && playerMotion.Attack())
        {
            isAttacking = true;
            Reset();
            playerMotion.Stopping();

            if (actualWeapon.type == WeaponType.shield) inventory.shield.SetActive(false);

            magicUse = true;
            anim.SetBool("MagicOff", true);
            anim.SetTrigger("Magic");
        }
    }

    public void Fire()
    {
        if (zTarget.target != null) playerMotion.UpdateFocus();

        UIManager.instance.FireUse();
        GameObject fireball = Instantiate(fireballPrefab, null);
        fireball.transform.position = fireballPrefab.transform.position;
        fireball.transform.rotation = fireballPrefab.transform.rotation;

        if (playerMotion.targetPlayer != null) fireball.transform.LookAt(playerMotion.targetPlayer.position);

        Vector3 targetDir = fireball.transform.forward * fireSpeed * 2;
        fireball.GetComponent<Rigidbody>().AddForce(targetDir);

        Destroy(fireball, 5);
        StartCoroutine("FireOff");
    }

    IEnumerator FireOff()
    {
        yield return new WaitForSeconds(timeFire);

        anim.SetBool("MagicOff", false);

        yield return new WaitForSeconds(.5f);

        if (actualWeapon.type == WeaponType.shield) inventory.shield.SetActive(false);

        isAttacking = false;
        playerMotion.StopEnd();
        UIManager.instance.ShowFireCooldown(fireCooldown);

        yield return new WaitForSeconds(fireCooldown);

        magicUse = false;
    }

    public void OnUseItem()
    {
        if (actualWeapon == null || actualItem == null) return;

        if (actualItem.type == WeaponType.bomb)
        {
            if (inventory.bombs != 0 && playerMotion.Attack())
            {
                isAttacking = true;
                Reset();
                playerMotion.Stopping();
                if (actualWeapon.type == WeaponType.crossbow) inventory.crossbow.SetActive(false);
                if (actualWeapon.type == WeaponType.sword) inventory.sword.SetActive(false);
                if (actualWeapon.type == WeaponType.shield) inventory.shield.SetActive(false);

                anim.SetTrigger("Throw");
                inventory.bombs--;
                UIManager.instance.UpdateBombs(inventory.bombs);
                bombUse = true;
            }
        }
        if (actualItem.type == WeaponType.heal)
        {
            if (inventory.potions != 0 && playerMotion.Attack())
            {
                isAttacking = true;
                Reset();
                playerMotion.Stopping();
                healParticle.SetActive(true);
                anim.SetTrigger("Heal");
                playerLife.currentLife += actualItem.points;
                UIManager.instance.UpdateLife(playerLife.currentLife);
                inventory.potions--;
                UIManager.instance.UpdatePotions(inventory.potions);
                Sequence s = DOTween.Sequence();
                s.AppendInterval(1).OnComplete(() =>
                {
                    HealEnd();
                });
            }
        }
    }

    public void OnItemChangeUp()
    {
        if (actualWeapon == null || actualItem == null) return;

        if (!isAttacking && playerMotion.Attack())
        {
            if (inventory.items.Count < 2) return;
            for (int i = inventory.items.Count - 1; i >= 0; i--)
            {
                if (inventory.items[i] == actualItem)
                {
                    actualItem = (i == 0) ? inventory.items[inventory.items.Count - 1] : inventory.items[i - 1];
                    break;
                }
            }
            UIManager.instance.ItemSelect(actualItem);
        }
    }
    public void OnItemChangeDown()
    {
        if (actualWeapon == null || actualItem == null) return;

        if (!isAttacking && playerMotion.Attack())
        {
            if (inventory.items.Count < 2) return;
            for (int i = 0; i < inventory.items.Count - 1; i++)
            {
                if (inventory.items[i] == actualItem)
                {
                    actualItem = (i == inventory.items.Count - 1) ? inventory.items[0] : inventory.items[i + 1];
                    break;
                }
            }
            UIManager.instance.ItemSelect(actualItem);
        }
    }

    public void HealEnd()
    {
        healParticle.SetActive(false);
        isAttacking = false;
        playerMotion.StopEnd();
    }

    public void Block()
    {
        isAttacking = true;
        Reset();
        playerMotion.Stopping();

        if (!anim.GetBool("ShieldUse")) anim.SetBool("ShieldUse", true);

        anim.SetInteger("Attack", 5);

        shieldBoxParticle.SetActive(false);
        shieldBoxParticle.SetActive(true);

        cinemachineImpulseSource.GenerateImpulse(Camera.main.transform.forward);

        anim.SetTrigger("Attack");
        StartCoroutine("MoveAgain");
    }

    public void Hit()
    {
        if (bombUse)
        {
            bombUse = false;
            GameObject bomb = Instantiate(inventory.bomb, null);
            bomb.transform.position = inventory.bomb.transform.position;
            bomb.transform.rotation = inventory.bomb.transform.rotation;
            bomb.SetActive(true);
            bomb.GetComponent<Bomb>().damage = actualItem.points;
            if (playerMotion.targetPlayer != null)
            {
                bomb.GetComponent<Rigidbody>().DOJump(playerMotion.targetPlayer.position - (Vector3.up * 1.5f), 2, 1, 1).OnComplete(() =>
                {
                    Destroy(bomb, .5f);
                    if (actualWeapon.type == WeaponType.crossbow) inventory.crossbow.SetActive(true);
                    if (actualWeapon.type == WeaponType.sword) inventory.sword.SetActive(true);
                    if (actualWeapon.type == WeaponType.shield) inventory.shield.SetActive(true);

                    isAttacking = false;
                    playerMotion.StopEnd();
                });
            }
            else
            {
                Vector3 f = forwardT.position - (transform.position + (Vector3.up * 1.5f));
                bomb.GetComponent<Rigidbody>().DOJump(forwardT.position + (f.normalized * 10f), 2, 1, 1).OnComplete(() =>
                {
                    Destroy(bomb, .5f);
                    if (actualWeapon.type == WeaponType.crossbow) inventory.crossbow.SetActive(true);
                    if (actualWeapon.type == WeaponType.sword) inventory.sword.SetActive(true);
                    if (actualWeapon.type == WeaponType.shield) inventory.shield.SetActive(true);

                    isAttacking = false;
                    playerMotion.StopEnd();
                });
            }
            return;
        }

        swordCollision.enabled = true;
        Reset();

        StartCoroutine("MoveAgain", (heavyAtk) ? .8f : .5f);
        StartCoroutine("ComboEnd");
    }

    public void Shoot()
    {
        if (zTarget.tag != null) playerMotion.UpdateFocus();

        if (inventory.arrows != 0)
        {
            GameObject arrow = Instantiate(arrowPrefab, null);
            arrow.transform.position = attachPoint.position;
            arrow.transform.rotation = arrowPrefab.transform.rotation;
            arrow.SetActive(true);

            ArrowCollision arrowCollision = arrow.GetComponent<ArrowCollision>();
            arrowCollision.cinemachineImpulseSource = cinemachineImpulseSource;
            arrowCollision.damage = actualWeapon.points;

            if (playerMotion.targetPlayer != null) arrow.transform.LookAt(playerMotion.targetPlayer);

            Vector3 targetDir = arrow.transform.forward * arrowSpeed * 2;
            arrow.GetComponent<Rigidbody>().AddForce(targetDir);

            Destroy(arrow, 5);
            inventory.arrows--;
            UIManager.instance.UpdateArrows(inventory.arrows);
        }
        StartCoroutine("MoveAgain");
    }

    public void OnArrowL()
    {
        if (!actualWeapon) return;

        if (!isAttacking && playerMotion.Attack())
        {
            StopCoroutine("ComboEnd");
            if (inventory.weapons.Count < 2) return;

            for (int i = inventory.weapons.Count - 1; i >= 0; i--)
            {
                if (inventory.weapons[i] == actualWeapon)
                {
                    actualWeapon = (i == 0) ? inventory.weapons[inventory.weapons.Count - 1] : inventory.weapons[i - 1];
                    break;
                }
            }
            ActiveWeapon();
        }
    }

    public void OnArrowR()
    {
        if (!actualWeapon) return;

        if (!isAttacking && playerMotion.Attack())
        {
            StopCoroutine("ComboEnd");
            if (inventory.weapons.Count < 2) return;

            for (int i = 0; i < inventory.weapons.Count; i++)
            {
                if (inventory.weapons[i] == actualWeapon)
                {
                    actualWeapon = (i == inventory.weapons.Count - 1) ? inventory.weapons[0] : inventory.weapons[i + 1];
                    break;
                }
            }
            ActiveWeapon();
        }
    }

    public void ActiveWeapon()
    {
        switch (actualWeapon.type)
        {
            case WeaponType.sword:
                inventory.SwordActive(actualWeapon);
                OnShieldUse(false);
                break;
            case WeaponType.shield:
                inventory.ShieldActive(actualWeapon);
                OnShieldUse(playerMotion.focus);
                break;
            case WeaponType.crossbow:
                inventory.CrossbowActive(actualWeapon);
                OnShieldUse(false);
                break;
            default:
                break;
        }
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
