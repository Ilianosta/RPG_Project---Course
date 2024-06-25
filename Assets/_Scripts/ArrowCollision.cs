using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    public int damage;
    public CinemachineImpulseSource cinemachineImpulseSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            cinemachineImpulseSource.GenerateImpulse(Camera.main.transform.forward);
            other.transform.GetComponent<Life>().GetHit(damage);
            Destroy(gameObject);
        }
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
