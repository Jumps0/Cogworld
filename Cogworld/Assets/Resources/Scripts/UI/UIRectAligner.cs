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

    public void AlignToBottomRight()
    {
        // Get the size of the reference RectTransform
        Vector2 referenceSize = referenceRectTransform.rect.size;

        // Calculate the position of the bottom right corner of the reference RectTransform in world space
        Vector3 referenceBottomRightWorld = referenceRectTransform.TransformPoint(referenceSize);

        // Convert the world position of the bottom right corner to local position of the target RectTransform
        Vector3 targetLocalPosition = targetRectTransform.parent.InverseTransformPoint(referenceBottomRightWorld);

        // Set the position of the target RectTransform to be at the calculated position
        targetRectTransform.localPosition = targetLocalPosition;
    }
}
