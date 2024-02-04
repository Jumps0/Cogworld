using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Used whenever the player enters a new floor/level during the little "scanning" animation. Deleted afterwards.
/// </summary>
public class AnimTileBlock : MonoBehaviour
{
    [Header("Core")]
    [Tooltip("WALL, DOOR, ITEM, BOT, MACHINE, TRASH, FLOOR")]
    public string type = "";
    public SpriteRenderer _sprite;
    public GameObject _highlight;
    public GameObject _dartHighlight;
    [SerializeField] private Animator _animator;
    
    [Header("Assignments")]
    [HideInInspector] public ItemObject _item;
    [HideInInspector] public GameObject _tile;
    [HideInInspector] public BotObject _bot;
    [Tooltip("Is this AnimTileBlock representing the player?")]
    public bool player;
    [HideInInspector] public Sprite _machineSprite;

    [Header("Colors")]
    public Color lowGreen;
    public Color highGreen;
    public Color playerColor;
    public Color veryDark;
    [SerializeField] private Sprite playerASCII;

    public void Init(string setType, GameObject tile = null, ItemObject item = null, BotObject bot = null, bool isPlayer = false, Sprite macSprite = null)
    {
        _sprite.color = Color.black;
        type = setType;

        if (tile)
        {
            _tile = tile;
            if (_tile.GetComponent<TileBlock>()._debrisSprite.activeInHierarchy)
            {
                _sprite.sprite = MiscSpriteStorage.inst.ASCII_debrisSprites[Random.Range(1, MiscSpriteStorage.inst.ASCII_debrisSprites.Count)];
            }
            else
            {
                _sprite.sprite = _tile.GetComponent<TileBlock>().tileInfo.asciiRep;
            }
        }
        if (item)
        {
            _item = item;
            _sprite.sprite = _item.asciiRep;
        }
        if (bot || isPlayer)
        {
            _bot = bot;
            player = isPlayer;

            if (isPlayer)
            {
                _sprite.sprite = playerASCII;
            }
            else
            {
                _sprite.sprite = _bot.asciiRep;
            }
        }
        if (macSprite)
        {
            _machineSprite = macSprite;
            // TODO
        }
    }

    public void Scan()
    {
        _animator.enabled = true;
        _highlight.SetActive(true);
        _animator.Play("TileAnimScanline");
        StartCoroutine(ScanDark());
    }

    public void HaltScan() // Probably uneccessary.
    {
        _animator.enabled = false;
        _highlight.SetActive(false);
    }

    public void Dart()
    {
        StopDart();
        StartCoroutine(DartTimer());
    }

    public void StopDart()
    {
        StopCoroutine(DartTimer()); // In-case it's already running
    }

    private IEnumerator DartTimer()
    {
        _dartHighlight.GetComponent<Animator>().enabled = true;
        _dartHighlight.SetActive(true);
        _dartHighlight.GetComponent<Animator>().Play("TileAnimScanlineQuick");

        yield return new WaitForSeconds(0.5f);

        _dartHighlight.SetActive(false);
        _dartHighlight.GetComponent<Animator>().enabled = false;
    }

    /// <summary>
    /// For when this object gets hit by a scan wave, the original tile should briefly appear dark while the highlight wave is active.
    /// Not for walls or doors.
    /// </summary>
    private IEnumerator ScanDark()
    {
        if (!finalize)
        {
            if (type != "WALL" && type != "DOOR")
            {
                float elapsedTime = 0f;
                float duration = 0.15f;
                // From veryDark -> Black (0.5s)
                while (elapsedTime < duration)
                {
                    _sprite.color = Color.Lerp(veryDark, Color.black, elapsedTime / duration);

                    elapsedTime += Time.deltaTime;
                    yield return null; // Wait for the next frame
                }
                _sprite.color = Color.black;
            }
        }
        else
        {
            if(type != "WALL" && type != "DOOR")
            {
                _sprite.color = lowGreen;
            }
            yield break;
        }
    }

    public void Animate()
    {
        StartCoroutine(Animation());
    }

    public bool pinged = false;
    private bool wallSkip = false;
    private bool finalize = false;

    /// <summary>
    /// 3 Seconds of fluff before the finalization emote plays across all AnimTileBlocks. This can be halted early.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animation()
    {

        // NOTE: WALLs & DOORs are special, they do their own thing.
        //       Everything else goes from black, to bright green, to dark green (unless stated otherwise).

        if ((type == "WALL" || type == "DOOR") && !(_tile.GetComponent<TileBlock>().tileInfo.type == TileType.Exit)) // This only gets called once, and its by the player's darts or a neighboring wall/door.
        {
            pinged = true;

            _sprite.color = lowGreen;

            // Ping any neighboring walls if possible.
            StartCoroutine(PingNeighboringWalls());

            // We want to flash from LOW to HIGH for 3 seconds.
            // - We will do this by using the code chunk below to go from high->low OR low->high 6 times (total) with duration 0.5f for 3 seconds total.
            #region 3 Second Flash
            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame

                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = highGreen;

            elapsedTime = 0f;
            duration = 0.5f;

            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = lowGreen;

            duration = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = highGreen;

            elapsedTime = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = lowGreen;

            duration = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = highGreen;

            elapsedTime = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = lowGreen;

            duration = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
                if (wallSkip)
                {
                    yield break;
                }
            }
            _sprite.color = highGreen;
            #endregion

        }
        else if (type == "BOT" && player) // First thing to appear
        {
            #region < HighGreen -> Blue -> HighGreen -> LowGreen >

            _sprite.color = highGreen;
            float elapsedTime = 0f;
            float duration = 0.5f;

            // First from HighGreen -> Blue (0.5s)
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, playerColor, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = playerColor;

            yield return new WaitForSeconds(1f + 1.5f); // Blue for 2 seconds + 1.5s delay
            elapsedTime = 0f;
            duration = 0.5f;
            // Then from Blue -> HighGreen (0.5s)
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(playerColor, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;
            // Then from HighGreen -> LowGreen (0.25s)
            elapsedTime = 0f;
            duration = 0.25f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = lowGreen;
            #endregion
        }
        else if (type == "WALL" && _tile.GetComponent<TileBlock>().tileInfo.type == TileType.Exit) // Next thing to appear after player
        {
            yield return new WaitForSeconds(0.25f);

            #region < Black -> HighGreen -> LowGreen 2>
            float elapsedTime = 0f;
            float duration = 0.5f;
            // First from Black -> HighGreen (0.5s)
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;
            // Then from HighGreen -> LowGreen (0.25s)
            elapsedTime = 0f;
            duration = 0.25f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = lowGreen;
            #endregion
        }
        else if (type == "BOT" && !player) // Next thing to appear after EXIT
        {
            yield return new WaitForSeconds(0.5f);

            #region < Black -> HighGreen -> LowGreen 3>
            float elapsedTime = 0f;
            float duration = 0.5f;
            // First from Black -> HighGreen (0.5s)
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;
            // Then from HighGreen -> LowGreen (0.25s)
            elapsedTime = 0f;
            duration = 0.25f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = lowGreen;
            #endregion
        }
        else if (type == "ITEM") // Next thing to appear after BOTs
        {
            yield return new WaitForSeconds(0.75f);

            #region < Black -> HighGreen -> LowGreen 4>
            float elapsedTime = 0f;
            float duration = 0.5f;
            // First from Black -> HighGreen (0.5s)
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;
            // Then from HighGreen -> LowGreen (0.25s)
            elapsedTime = 0f;
            duration = 0.25f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = lowGreen;
            #endregion
        }
        else if (type == "FLOOR" || type == "TRASH")
        {
            // This doesn't do anything until the final animation.
        }
    }

    public void FinalizeAnimation()
    {
        StopCoroutine(Animation());
        wallSkip = true;
        finalize = true;
        _sprite.color = lowGreen;
        StartCoroutine(FinalizationAnim());
    }

    /// <summary>
    /// Runs over a period of 2 seconds. All AnimTileBlocks should be in sync.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FinalizationAnim()
    {
        if(type == "WALL" || type == "DOOR")
        {
            // Over a period of 2 seconds, go from low to high, then back to low.
            #region 2 Second Flash
            yield return new WaitForSeconds(0.5f);
            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;

            elapsedTime = 0f;
            duration = 0.5f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _sprite.color = lowGreen;
            yield return new WaitForSeconds(0.5f);
            #endregion

        }
        else if (type == "BOT" || type == "ITEM" || type == "MACHINE")
        {
            // This just stays dark green the whole time.
            _sprite.color = lowGreen;
            yield return new WaitForSeconds(2f);
        }
        else if(type == "TRASH" || type == "FLOOR")
        {
            // This quickly goes from low to high, then quickly back to low for some time.
            #region 2 Second Quick Flash
            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(lowGreen, highGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            _sprite.color = highGreen;

            elapsedTime = 0f;
            duration = 1f;
            while (elapsedTime < duration)
            {
                _sprite.color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            _sprite.color = lowGreen;
            #endregion
        }

        // And when finished...
        yield return null;

        // Remove self from list
        Vector2Int selfLocation = new Vector2Int((int)this.gameObject.transform.position.x, (int)this.gameObject.transform.position.y);
        if (UIManager.inst.NFA_squares.Count > 0 && UIManager.inst.NFA_squares.ContainsKey(selfLocation))
        {
            UIManager.inst.NFA_squares.Remove(selfLocation);
        }

        // Destroy Self
        Destroy(gameObject);
    }


    private IEnumerator PingNeighboringWalls()
    {
        yield return new WaitForSeconds(0.25f);

        List<GameObject> list = FindNeighboringWalls();
        if(list.Count > 0)
        {
            foreach(GameObject go in list)
            {
                if (!go.GetComponent<AnimTileBlock>().pinged)
                {
                    go.GetComponent<AnimTileBlock>().Animate();
                }
            }
        }

    }

    /// <summary>
    /// Goes through all of the (visible) neighboring tiles and returns a list of all neighbors that are WALLS or DOORs.
    /// </summary>
    /// <returns>A list of any neighboring AnimTileBlock gameObjects that are classified as WALL or DOOR.</returns>
    private List<GameObject> FindNeighboringWalls()
    {
        List<GameObject> returns = new List<GameObject>();

        Vector2Int selfLocation = new Vector2Int((int)this.gameObject.transform.position.x, (int)this.gameObject.transform.position.y);

        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(0, 1))) // Top
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, 1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, 1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, 1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(1, 1))) // Top-Right
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(0, -1))) // Bottom
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, -1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, -1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(0, -1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(1, -1))) // Bottom-Right
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, -1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, -1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, -1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(-1, 1))) // Top-Left
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(-1, 0))) // Left
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 0)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 0)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, 0)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(-1, -1))) // Bottom-Left
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, -1)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, -1)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(-1, -1)]);
            }
        }
        if (UIManager.inst.NFA_squares.ContainsKey(selfLocation + new Vector2Int(1, 0))) // Right
        {
            if (UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 0)].GetComponent<AnimTileBlock>().type == "WALL"
                || UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 0)].GetComponent<AnimTileBlock>().type == "DOOR")
            {
                returns.Add(UIManager.inst.NFA_squares[selfLocation + new Vector2Int(1, 0)]);
            }
        }

        return returns;
    }
}
