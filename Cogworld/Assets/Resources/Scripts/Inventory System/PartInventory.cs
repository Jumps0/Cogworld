using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartInventory : MonoBehaviour
{
    [Header("OwnerValues")]
    public GameObject attachedUser;
    public bool belongsToAI = true;

    [Header("Power")]
    public InventoryObject _invPower;
    public int maxSize_power;
    [Header("Propulsion")]
    public InventoryObject _invPropulsion;
    public int maxSize_propulsion;
    [Header("Utility")]
    public InventoryObject _invUtility;
    public int maxSize_utility;
    [Header("Weapon")]
    public InventoryObject _invWeapon;
    public int maxSize_weapon;
    [Header("Inventory")]
    public InventoryObject _inventory;
    public int maxSize_inv;

    // Start is called before the first frame update
    void Start()
    {
        attachedUser = this.gameObject;
    }

    public void SetNewInventorySizes(int power, int prop, int util, int wep, int inv)
    {
        maxSize_power = power;
        maxSize_propulsion = prop;
        maxSize_utility = util;
        maxSize_weapon = wep;
        maxSize_inv = inv;
    }
}
