using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRectAligner : MonoBehaviour
{
    public RectTransform targetRectTransform;
    public RectTransform referenceRectTransform;

    void Start()
    {
        // Align the target RectTransform to the bottom right corner of the reference RectTransform
        AlignToBottomRight();
    }

    private void Update()
    {
        if (referenceRectTransform.gameObject.activeInHierarchy)
        {
            AlignToBottomRight();
        }
    }

    public void AlignToBottomRight()
    {
        // Get the size of the reference RectTransform
        Vector2 referenceSize = referenceRectTransform.rect.size;

        // Get the pivot points of both RectTransforms
        Vector2 referencePivot = referenceRectTransform.pivot;
        Vector2 targetPivot = targetRectTransform.pivot;

        // Calculate the local position of the bottom right corner of the reference RectTransform
        Vector2 referenceBottomRightLocal = new Vector2(referenceSize.x * (1f - referencePivot.x), -referenceSize.y * referencePivot.y);

        // Convert the local position to world space
        Vector3 referenceBottomRightWorld = referenceRectTransform.TransformPoint(referenceBottomRightLocal);

        // Convert the world position to local position of the target RectTransform
        Vector3 targetLocalPosition = targetRectTransform.parent.InverseTransformPoint(referenceBottomRightWorld);

        // Offset the local position by the pivot of the target RectTransform
        Vector2 targetSize = targetRectTransform.rect.size;
        targetLocalPosition -= new Vector3(targetSize.x * targetPivot.x, targetSize.y * targetPivot.y, 0f);

        // Set the position of the target RectTransform to be at the calculated position
        targetRectTransform.localPosition = targetLocalPosition + new Vector3(-8, 13);
    }
}
