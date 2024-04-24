using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBorderIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sprite;
    public MachinePart machine_parent;
    [SerializeField] private Animator animator;

    public void SetFlash(bool flash)
    {
        animator.gameObject.SetActive(flash);

        if (animator.gameObject.activeInHierarchy && !this.animator.GetCurrentAnimatorStateInfo(0).IsName("TileIndicatorFlash"))
        {
            animator.Play("TileIndicatorFlash");
        }
    }

    

    private void OnDestroy()
    {
        if (UIManager.inst && UIManager.inst.GetComponent<BorderIndicators>().locations.ContainsValue(this.gameObject))
        {
            UIManager.inst.GetComponent<BorderIndicators>().locations.Remove(HF.V3_to_V2I(this.gameObject.transform.position)); // This should work?
        }
    }

}
