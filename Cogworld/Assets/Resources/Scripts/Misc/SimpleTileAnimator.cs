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
    [Header("   Collapse/Destroyed Animation")]
    public bool isDestroyed = false;

    private void Update()
    {
        if (isDestroyed)
        {
            CollapseDestroyed();
        }
    }

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
            yield return null;
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

    #region IFF Animation
    public void InitIFF(float delay)
    {
        // Set to unseen!
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0f);

        // Start the coroutine
        StartCoroutine(IFFAnimation(delay));
    }

    private IEnumerator IFFAnimation(float delay)
    {
        // Wait for the delay to end (part of staggered animation process)
        float elapsedTime = 0f;
        float duration = delay;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Now do the animation
        // -Become visible as white
        sprite.color = new Color(Color.white.r, Color.white.g, Color.white.b, 1f);

        // -End color is dark blue, somewhere between approx: 170-220b
        Color start = Color.white;
        Color end = new Color(0f, 0f, Random.Range(170f, 220f) / 256f);

        // Over a period of ~0.5f seconds, go from the start color to the end color
        elapsedTime = 0f;
        duration = Random.Range(0.45f, 0.55f);

        while (elapsedTime < duration)
        {
            sprite.color = Color.Lerp(start, end, elapsedTime / duration);

            // While this is happening, we also want to randomly cycle through the character sprites
            sprite.sprite = MiscSpriteStorage.inst.ASCII_characters[Random.Range(0, MiscSpriteStorage.inst.ASCII_characters.Count - 1)];

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sprite.color = end;

        // All done? Destroy this object
        Destroy(this.gameObject);
    }
    #endregion

    #region Collapse/Destroyed Animation
    private void CollapseDestroyed()
    {
        sprite.color = GameManager.inst.warningPulseColor;
    }
    #endregion
}
