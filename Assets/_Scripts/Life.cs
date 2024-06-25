using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    public int maxLife;
    int m_life;
    [SerializeField]
    public int currentLife
    {
        get { return m_life; }
        set
        {
            if (value > maxLife)
            {
                m_life = maxLife;
            }
            else
            {
                m_life = value;
            }
        }
    }

    public Animator anim;
    public Rigidbody rb;

    private void Start()
    {
        m_life = maxLife;

        if (GetComponentInChildren<Animator>()) anim = GetComponentInChildren<Animator>();
        if (GetComponentInChildren<Rigidbody>()) rb = GetComponentInChildren<Rigidbody>();
    }

    public virtual void GetHit(int damage)
    {
        currentLife -= damage;
    }
}
