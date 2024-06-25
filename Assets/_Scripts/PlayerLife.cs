using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : Life
{
    public PlayerMotion playerMotion;
    public PlayerCombat playerCombat;
    public Inventory inventory;
    public GameObject particleDamage;
    CinemachineImpulseSource cinemachineImpulseSource;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
        playerCombat = GetComponent<PlayerCombat>();
        inventory = GetComponent<Inventory>();
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public override void GetHit(int damage)
    {
        if (currentLife == 0) return;
        base.GetHit(damage);
        StopCoroutine("NoHit");
        playerCombat.Reset();
        playerMotion.Stopping();
        if (currentLife > 0)
        {
            StartCoroutine("NoHit");
        }
        else
        {
            playerMotion.stop = true;
            playerMotion.enabled = false;
            playerCombat.enabled = false;
            StartCoroutine("Death");
        }
        anim.Rebind();
        if (inventory.swordUse)
        {
            anim.SetBool("Weapon", true);
            anim.SetTrigger("SwitchWeapon");
        }

        if (playerCombat.actualWeapon != null)
        {
            if (playerCombat.actualWeapon.type == WeaponType.crossbow) anim.SetFloat("WeaponN", 0);
            if (playerCombat.actualWeapon.type == WeaponType.sword) anim.SetFloat("WeaponN", 1);
            if (playerCombat.actualWeapon.type == WeaponType.shield) anim.SetFloat("WeaponN", 2);
        }
        anim.SetInteger("Life", currentLife);
        UIManager.instance.UpdateLife(currentLife);
        rb.velocity = Vector3.zero;
        particleDamage.SetActive(false);
        particleDamage.SetActive(true);
        cinemachineImpulseSource.GenerateImpulse(Camera.main.transform.forward);
        anim.SetTrigger("Hit");
        Sequence time = DOTween.Sequence();
        Time.timeScale = .4f;
        time.AppendInterval(.03f).OnComplete(() =>
        {
            Time.timeScale = 1;
        }).SetUpdate(true);
    }

    IEnumerator NoHit()
    {
        yield return new WaitForSeconds(.5f);

        if (currentLife != 0)
        {
            playerCombat.isAttacking = false;
            playerMotion.StopEnd();
        }
    }

    IEnumerator Death()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
