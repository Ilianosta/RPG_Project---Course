using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none,
    sword,
    shield,
    crossbow,
    heal,
    bomb
}

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item", order = 1)]
public class Items : ScriptableObject
{
    public string _name;
    public WeaponType type;
    public int points;
    [TextArea]
    public string msg;
}
