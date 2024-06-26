using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvents : MonoBehaviour
{
    public EnemyCombat enemyCombat;
    private void Awake()
    {
        enemyCombat = GetComponent<EnemyCombat>();
    }

    public void Hit()
    {
        enemyCombat.Hit();
    }

    public void FootL()
    {

    }

    public void FootR()
    {
        
    }
}
