using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackIndicator : MonoBehaviour
{
    public GameObject strikeline;
    public Item _weapon;
    private bool didHit;

    public void Init(Item weapon, float rotation, bool hit)
    {
        strikeline.transform.rotation = Quaternion.Euler(0f, 0f, rotation * -45f);
        _weapon = weapon; // Set weapon var
        didHit = hit;

        strikeline.GetComponent<SpriteRenderer>().color = _weapon.itemData.itemColor; // Set the color (maybe change this later)?

        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        // Play the sound
        if(didHit) // Hit sound
        {
            AudioManager.inst.CreateTempClip(this.transform.position, _weapon.itemData.shot.shotSound[Random.Range(0, _weapon.itemData.shot.shotSound.Count - 1)], 0.7f);
        }
        else // Miss sound
        {
            AudioManager.inst.CreateTempClip(this.transform.position, _weapon.itemData.meleeAttack.missSound[Random.Range(0, _weapon.itemData.meleeAttack.missSound.Count - 1)], 0.7f);
        }
        

        yield return new WaitForSeconds(_weapon.itemData.meleeAttack.visualAttackTime); // Wait

        Destroy(this.gameObject); // Destroy self
    }
}
