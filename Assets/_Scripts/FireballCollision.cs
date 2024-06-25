using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    public int damage;
    public GameObject explosion;
    public CinemachineImpulseSource cinemachineImpulseSource;

    private void Awake()
    {
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield")) return;
        if (other.CompareTag("Target")) other.GetComponent<Life>().GetHit(damage);

        cinemachineImpulseSource.GenerateImpulse(Camera.main.transform.forward);

        explosion.transform.parent = null;
        explosion.SetActive(true);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (gameObject == null) return;

        if (explosion != null)
        {
            if (!explosion.activeSelf)
            {
                explosion.transform.parent = null;
                explosion.SetActive(true);
            }
            Destroy(explosion, 5);
        }
    }
}
