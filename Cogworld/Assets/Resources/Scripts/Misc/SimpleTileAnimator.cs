using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple and generic method to animate a tile.
/// </summary>
public class SimpleTileAnimator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sprite;

    [Header("Assignments")]
    public Color startColor;
    public Color endColor;
    public float animationTime;
    [Header("   Chain Animation")]
    [Tooltip("Does this animation have a secondary component that should be triggered afterwards?")]
    public bool isChain = false;
    public Color chain_startColor;
    public Color chain_endColor;
    public float chain_animationTime;
    [Tooltip("Is there any delay between the first animation ending and the second animation starting? 0 by default.")]
    public float chain_delay;

    public void Init(Color startC, Color endC, float time)
    {
        startColor = startC;
        endColor = endC;
        animationTime = time;
    }

    public void Animate()
    {
        StartCoroutine(Animation());
    }


    private IEnumerator Animation()
    {
        float elapsedTime = 0f;
        float duration = animationTime;

        while (elapsedTime < duration)
        {
            sprite.color = Color.Lerp(startColor, endColor, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        sprite.color = endColor;

        if(isChain)
        {
            yield return new WaitForSeconds(chain_delay);
            StartCoroutine(ChainAnimation());
        }
    }

    public void Stop()
    {
        StopCoroutine(Animation());

        sprite.color = endColor;

        if (isChain)
        {
            StopCoroutine(ChainAnimation());
            sprite.color = chain_endColor;
        }
    }

    #region Chain Animation

    public void InitChain(Color startC, Color endC, float time, float chainDelay = 0f)
    {
        chain_startColor = startC;
        chain_endColor = endC;
        chain_animationTime = time;
        isChain = true;
        chain_delay = chainDelay;
    }

    private IEnumerator ChainAnimation()
    {
        float elapsedTime = 0f;
        float duration = chain_animationTime;

        while (elapsedTime < duration)
        {
            sprite.color = Color.Lerp(chain_startColor, chain_endColor, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        sprite.color = chain_endColor;
    }

    #endregion
}
