using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Items key;
    public bool open;
    public BoxCollider boxTrigger;

    HingeJoint hinge;
    JointLimits hingeLimits;
    Rigidbody rb;
    GameObject player;

    private void OpenHingeLimits()
    {
        hingeLimits.max = 85;
        hingeLimits.min = -85;
        hinge.limits = hingeLimits;
        rb.isKinematic = false;
    }

    private void Awake()
    {
        hinge = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (open)
        {
            OpenHingeLimits();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (open) return;

        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            player.GetComponent<PlayerMotion>().door = this;
            UIManager.instance.ShowInteract();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (open) return;
        
        if (other.CompareTag("Player"))
        {
            player.GetComponent<PlayerMotion>().door = null;
            player = null;
            UIManager.instance.HideInteract();
        }
    }

    public void Open()
    {
        if (open) return;

        PlayerMotion playerMotion = player.GetComponent<PlayerMotion>();
        if (player.GetComponent<Inventory>().weapons.Where(i => i == key).Count() == 0)
        {
            open = false;

            playerMotion.interacting = true;
            playerMotion.Stopping();

            UIManager.instance.HideInteract();
            UIManager.instance.ShowNotification("The door is closed!");

            Sequence s = DOTween.Sequence();
            s.AppendInterval(2).OnComplete(() =>
            {
                playerMotion.StopEnd();
                playerMotion.interacting = false;

                UIManager.instance.HideNotification();
            });
        }
        else
        {
            open = true;
            playerMotion.interacting = true;
            playerMotion.Stopping();

            UIManager.instance.HideInteract();
            UIManager.instance.ShowNotification("The door is open!");

            Sequence s = DOTween.Sequence();
            s.AppendInterval(2).OnComplete(() =>
            {
                playerMotion.StopEnd();
                playerMotion.interacting = false;

                UIManager.instance.HideNotification();

                OpenHingeLimits();

                boxTrigger.enabled = false;
                playerMotion.door = null;
            });
        }
    }
}
