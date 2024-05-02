using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMoney : GenericSingletonClass<PlayerMoney>
{
    [SerializeField] private int money;
    
    public int Money
    {
        get => money;
        set => money = Mathf.Clamp(value, 0, 999);
    }

    public void IncrementMoney(int amount) => money += amount;
    
    public void DecrementMoney(int amount) => money -= amount;
}
