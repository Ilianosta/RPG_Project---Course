using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    PlayerMotion playerMotion;
    private void Awake()
    {
        playerMotion = GetComponentInParent<PlayerMotion>();
    }

    public void Land()
    {
        playerMotion.FallEnd();
    }
}
