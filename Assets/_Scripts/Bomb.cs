using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject burst;
    public LayerMask enemyLayer;
    public float area;
    public int damage;
    CinemachineImpulseSource cinemachineImpulseSource;
    private void Awake()
    {
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }
    private void OnDestroy()
    {
        Collider[] rangeCheck = Physics.OverlapSphere(burst.transform.position, area, enemyLayer);
        burst.transform.parent = null;
        burst.SetActive(true);
        Destroy(burst, 5);
        cinemachineImpulseSource.GenerateImpulse(Camera.main.transform.forward);
        foreach (Collider c in rangeCheck)
        {
            if (c.GetComponent<TargetDamage>() != null) c.GetComponent<TargetDamage>().Damage(damage);
        }
    }
}
