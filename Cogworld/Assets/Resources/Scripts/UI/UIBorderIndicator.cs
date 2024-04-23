using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBorderIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sprite;
    public MachinePart parent;

    private void OnDestroy()
    {
        if (UIManager.inst && UIManager.inst.GetComponent<BorderIndicators>().locations.ContainsValue(this.gameObject))
        {
            UIManager.inst.GetComponent<BorderIndicators>().locations.Remove(HF.V3_to_V2I(this.gameObject.transform.position)); // This should work?
        }
    }
}
