using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
sealed class AI_Melee : MonoBehaviour
{
    [SerializeField] private int maxHp, hp, defense, power;
    [SerializeField] private Actor target;

    public int Hp
    {
        get => hp; set
        {
            hp = Mathf.Max(0, Mathf.Min(value, maxHp));
            if(hp == 0)
            {
                Die();
            }
        }
    }

    public int Defense { get => defense; }
    public int Power { get => power;  }
    public Actor Target { get => target; set => target = value; }

    private void Die()
    {
        if (GetComponent<PlayerData>())
        {
            Debug.Log($"You died!");
        }
        else
        {
            Debug.Log($"{name} is dead!");
        }

        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        //sp.sprite = GameManager.inst.DeadSprite;
        sp.color = new Color(191, 0, 0, 1);
        sp.sortingOrder = 0;

        name = $"Reamins of {name}";
        GetComponent<Actor>().BlocksMovement = false;
        GetComponent<Actor>().IsAlive = false;

        if (!GetComponent<PlayerData>())
        {
            TurnManager.inst.RemoveActor(this.GetComponent<Actor>());
        }
    }
}
