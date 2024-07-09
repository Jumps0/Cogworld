using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using UnityEditor;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject//, ISerializationCallbackReceiver
{

    /// <summary>
    /// Item database idk????
    /// </summary>

    public string savePath;
    public ItemDatabaseObject database;
    public Inventory Container;
    
    public InventoryObject(int size, string name = "")
    {
        database = MapManager.inst.itemDatabase; // Set the database
        Container = new Inventory(); // Set the container
        Container.Items = new InventorySlot[size]; // Set size

        for (int i = 0; i < Container.Items.Length; i++)
        {
            Container.Items[i] = new InventorySlot();
        }

        if(name != "")
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Tries to add the specified item (and the amount) to this inventory. Returns true/false if successful.
    /// </summary>
    /// <param name="_item">The item to add.</param>
    /// <param name="_amount">The ammount of that item to add. Usually 1.</param>
    /// <returns>Returns true/false if successful at adding the item.</returns>
    public bool AddItem(Item _item, int _amount = 1)
    {
        /*
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].item.Id == _item.Id)
            {
                Container.Items[i].AddAmount(_amount);
                return;
            }
        }
        SetEmptySlot(_item, _amount);
        */
        
        if (EmptySlotCount <= 0)
            return false;
        InventorySlot slot = FindItemOnInventory(_item);

        _item = new Item(_item);
        
        if (!database.Items[_item.Id].data.stackable || slot == null)
        {
            SetEmptySlot(_item, _amount);
            return true;
        }
        slot.AddAmount(_amount);
        return true;
        
    }

    /// <summary>
    /// Swaps two slots in this inventory.
    /// </summary>
    /// <param name="item1">The 1st slot.</param>
    /// <param name="item2">The 2nd slot.</param>
    public void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
        {
            InventorySlot temp = new InventorySlot(item2.item, item2.amount, item2.parent);
            item2.UpdateSlot(item1.item, item1.amount, item1.parent);
            item1.UpdateSlot(temp.item, temp.amount, temp.parent);
        }
    }

    /// <summary>
    /// Removes the specified item from this inventory.
    /// </summary>
    /// <param name="_item">The item to remove.</param>
    public void RemoveItem(Item _item)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].item == _item)
            {
                Container.Items[i].UpdateSlot(null, 0);
            }
        }
    }

    /// <summary>
    /// Resorts the inventory so that all the slots with items are at the top, and all the empty slots are at the bottom.
    /// </summary>
    public void Sort()
    {
        int nonNegativeIndex = 0;
        int negativeIndex = Container.Items.Length - 1;

        while (nonNegativeIndex < negativeIndex)
        {
            if (Container.Items[nonNegativeIndex].item.Id == -1)
            {
                // Swap the current element with the one at the negative index
                SwapItems(Container.Items[nonNegativeIndex], Container.Items[negativeIndex]);
                negativeIndex--;
            }
            else
            {
                // Move to the next non-negative element
                nonNegativeIndex++;
            }
        }
    }

    /// <summary>
    /// Returns how many EMPTY slots this inventory has (getter).
    /// </summary>
    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < Container.Items.Length; i++)
            {
                if (Container.Items[i].item.Id <= -1)
                    counter++;
            }
            return counter;
        }
    }

    /// <summary>
    /// Checks to see how many REAL items are in this inventory (getter).
    /// </summary>
    /// <returns>Returns an int value of how many REAL items are in the inventory.</returns>
    public int ItemCount
    {
        get
        {
            int amount = 0;
            for (int i = 0; i < Container.Items.Length; i++)
            {
                if (Container.Items[i] != null && Container.Items[i].item != null && Container.Items[i].item.Id >= 0)
                {
                    amount++;
                }
            }

            return amount;
        }
    }

    /// <summary>
    /// Finds the first empty slot in this inventory and attempts to add the specified item (and amount) at that slot.
    /// </summary>
    /// <param name="_item">The item to add.</param>
    /// <param name="_amount">The amount of said item to add (usually 1).</param>
    /// <returns></returns>
    public InventorySlot SetEmptySlot(Item _item, int _amount = 1)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if(Container.Items[i].item.Id <= -1)
            {
                Container.Items[i].UpdateSlot(_item, _amount);
                return Container.Items[i];
            }
        }
        // TODO: Setup functionality for full inventory.
        return null;
    }

    /// <summary>
    /// Attempts to add a slot in the inventory based on an item input (matches ids).
    /// </summary>
    /// <param name="_item">An item within the slot we want to look for.</param>
    /// <returns>The slot found containing our specified input item.</returns>
    public InventorySlot FindItemOnInventory(Item _item)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].item.Id == _item.Id)
            {
                return Container.Items[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Checks to see if this inventory contains any item which inherts from an ItemObject that we are looking for.
    /// </summary>
    /// <param name="item">An ItemObject that we are looking for.</param>
    /// <returns>True/False. If this ItemObject can be found in this inventory.</returns>
    public bool HasGenericItem(ItemObject item)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].item.itemData == item)
            {
                return true;
            }
        }

        return false;
    }

    /*
    public void OnAfterDeserialize()
    {
        for (int i = 0; i < Container.Items.Count; i++)
        {
            Container.Items[i].item = database.GetItem[Container.Items[i].ID];
        }
    }

    public void OnBeforeSerialize()
    {

    }
    */

    #region Serialization
    [ContextMenu("Save")]
    public void Save()
    {
        Debug.Log("Saving to: " + savePath);

        //string saveData = JsonUtility.ToJson(this, true);
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        //bf.Serialize(file, saveData);
        //file.Close();
        

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        stream.Close();

    }

    [ContextMenu("Load")]
    public void Load()
    {
        if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            Debug.Log("Loading from: " + savePath);

            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
            //file.Close();
            

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);

            //Container = (Inventory)formatter.Deserialize(stream);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for (int i = 0; i < Container.Items.Length; i++)
            {
                Container.Items[i].UpdateSlot(newContainer.Items[i].item, newContainer.Items[i].amount);
            }

            stream.Close();

        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Container.Clear();
    }
    #endregion
}

[System.Serializable]
public class Inventory
{
    public InventorySlot[] Items = new InventorySlot[24];
    // 24 is default size, we can change this later

    public void Clear()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].RemoveItem();
        }
    }
}

[System.Serializable]
public class InventorySlot{

    public List<ItemSlot> AllowedItems = new List<ItemSlot>();
    [System.NonSerialized] // No touchy!
    public UserInterface parent;
    public Item item; // Item stored in this inventory slot
    public int amount;

    public ItemObject ItemObject
    {
        get
        {
            if(item.Id >= 0)
            {
                return parent._inventory.database.Items[item.Id];
            }
            return null;
        }
    }

    public InventorySlot()
    {
        item = new Item();
        amount = 0;
    }
    
    public InventorySlot(Item _item, int _amount, UserInterface _parent = null)
    {
        item = _item;
        amount = _amount;
        if(_parent != null)
        {
            parent = _parent;
        }
    }

    public void UpdateSlot(Item _item, int _amount, UserInterface _parent = null)
    {
        item = _item;
        amount = _amount;
        if(_parent != null)
        {
            parent = _parent;
        }
    }

    public void RemoveItem()
    {
        item = new Item();
        amount = 0;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public bool CanPlaceInSlot(ItemObject _itemObject)
    {
        if(AllowedItems.Count <= 0 || _itemObject == null || _itemObject.data.Id < 0)
        {
            return true;
        }

        if (AllowedItems.Contains(_itemObject.slot))
        {
            return true;
        }

        return false;
    }
        
}

 
