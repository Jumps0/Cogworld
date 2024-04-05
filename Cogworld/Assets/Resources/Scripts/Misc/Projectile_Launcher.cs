using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class Projectile_Launcher : MonoBehaviour
{
    [Header("References")]
    public GameObject _projectile;
    public GameObject _highlight;
    [SerializeField] private AudioSource _source;

    [Header("Values")]
    public Transform _target;
    public Transform _origin;
    private ItemObject _weapon;
    //
    public Color projColor;
    public Color highlightColor;
    //
    private float _speed = 20f;

    public void Setup(Transform origin, Transform target, ItemObject weapon)
    {
        _origin = origin;
        _target = target;
        projColor = weapon.projectile.projectileColor;
        highlightColor = Color.black;

        _projectile.GetComponent<Image>().sprite = weapon.projectile.projectileSprite;
        _projectile.GetComponent<Image>().color = projColor;
        _highlight.GetComponent<Image>().color = highlightColor;

        _weapon = weapon;

        StartCoroutine(MoveProjectile());
        StartCoroutine(LifetimeDestroy());
    }

    private IEnumerator MoveProjectile()
    {
        // While moving this projectile should play its sound
        _source.PlayOneShot(_weapon.shot.shotSound[Random.Range(0, _weapon.shot.shotSound.Count - 1)]);

        // Begin laying the trail if needed
        if (_weapon.projectile.projectileTrail != ProjectileTrailStyle.None)
            StartCoroutine(BeginLayTrail());

        // calculate the direction towards the target
        Vector3 direction = _target.position - this.transform.position;
        direction.z = 0f; // ensure the projectile stays in the 2D plane

        // Determine which direction the projectile should face
        #region Projectile Orientation
        _projectile.GetComponent<Image>().sprite = HF.GetProjectileSprite(_origin.transform.position, _target.transform.position);
        #endregion

        while (true)
        {
            if (_target == null || _origin == null || _projectile == null || _highlight == null)
            {
                if(!finishing)
                    StartCoroutine(OnReachTarget());
            }

            // rotate the projectile towards the target
            this.transform.up = direction.normalized;

            // calculate the distance to the target
            float distanceToTarget = direction.magnitude;

            /*
            if(distanceToTarget < 1)
            {
                Debug.Break(); // DEBUG
            }
            */

            if (distanceToTarget <= 0.01f)
            {
                    
                _highlight.transform.position = new Vector3(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(_target.position.y), _target.position.z);

                // destroy the projectile if it has reached the target
                if (!finishing)
                    StartCoroutine(OnReachTarget());
                yield break;
            }
            else
            {

                // move the projectile towards the target at the specified speed
                float step = _speed * Time.deltaTime;
                this.transform.position = Vector3.MoveTowards(this.transform.position, _target.position, step);

                // Snap the highlight & projectile to the nearest whole (int) number
                Vector3 snapPosition = this.transform.position;
                snapPosition.x = Mathf.Round(snapPosition.x);
                snapPosition.y = Mathf.Round(snapPosition.y);
                _highlight.transform.position = snapPosition;
                _projectile.transform.position = snapPosition;

                _highlight.transform.rotation = Quaternion.identity;
                _projectile.transform.rotation = Quaternion.identity;

                yield return null;
            }

            if (_target == null || _origin == null || _projectile == null || _highlight == null)
            {
                if (!finishing)
                    StartCoroutine(OnReachTarget());
            }

            // update the direction towards the target every frame
            direction = _target.position - this.transform.position;
            direction.z = 0f; // ensure the projectile stays in the 2D plane
        }
    }

    bool finishing = false;
    public IEnumerator OnReachTarget()
    {
        finishing = true;

        _source.Stop();
        UIManager.inst.projectiles.Remove(this.gameObject);

        _highlight.gameObject.SetActive(false);
        _projectile.gameObject.SetActive(false);

        while(trailObjects.Count > 0) // Stall while the trail is finishing up
        {
            yield return null;
        }

        Destroy(this.gameObject.transform.parent.gameObject);
    }

    IEnumerator LifetimeDestroy()
    {
        yield return new WaitForSeconds(5f);

        OnReachTarget();
    }

    #region Trail

    private Dictionary<Vector2Int, GameObject> trailObjects = new Dictionary<Vector2Int, GameObject>();

    private IEnumerator BeginLayTrail()
    {
        Vector2Int oldLocation = HF.V3_to_V2I(_highlight.transform.position);

        while (true)
        {
            if(HF.V3_to_V2I(_highlight.transform.position) != oldLocation) // Projectile has moved, create a new object.
            {
                GameObject trailObj = CreateTrailObj(oldLocation); // Create a new trail object

                oldLocation = HF.V3_to_V2I(_highlight.transform.position); // Set new old location

                StartCoroutine(AnimateTrailObj(trailObj, trailObjects.Count % 2 == 0));
            }

            yield return null;
        }
    }

    private GameObject CreateTrailObj(Vector2Int pos)
    {
        var spawnedTile = Instantiate(CFXManager.inst.prefab_tile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"TrailFX: {pos.x},{pos.y}"; // Give grid based name

        spawnedTile.GetComponent<SpriteRenderer>().color = CFXManager.inst.t_red;
        trailObjects.Add(pos, spawnedTile);

        return spawnedTile;
    }

    private IEnumerator AnimateTrailObj(GameObject obj, bool flair)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        float elapsedTime = 0f;
        float duration = 0.2f;

        switch (_weapon.projectile.projectileTrail)
        {
            case ProjectileTrailStyle.MissileMinor:
                /*
                 * 1. The sprite starts red, and stays red for a little bit,
                 * 2. then changes to gray
                 * 3. and quickly fades out
                 * 4. Then (if it is every other), they grey returns briefly before fading out again
                 */

                // 1
                sr.color = CFXManager.inst.t_red;

                yield return new WaitForSeconds(0.05f);

                // 2
                sr.color = CFXManager.inst.t_gray;

                // 3
                elapsedTime = 0f;
                duration = 0.2f;

                while (elapsedTime < duration)
                {
                    if (obj != null)
                    {
                        Color setColor = obj.GetComponent<SpriteRenderer>().color;
                        setColor.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                        obj.GetComponent<SpriteRenderer>().color = setColor;

                        elapsedTime += Time.deltaTime;
                        yield return null; // Wait for the next frame
                    }
                    else
                    { // In case we delete this tile while its animating
                        yield break;
                    }
                }

                // 4
                if (flair)
                {
                    yield return new WaitForSeconds(0.05f);

                    sr.color = CFXManager.inst.t_gray;

                    elapsedTime = 0f;

                    while (elapsedTime < duration)
                    {
                        if (obj != null)
                        {
                            Color setColor = obj.GetComponent<SpriteRenderer>().color;
                            setColor.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                            obj.GetComponent<SpriteRenderer>().color = setColor;

                            elapsedTime += Time.deltaTime;
                            yield return null; // Wait for the next frame
                        }
                        else
                        { // In case we delete this tile while its animating
                            yield break;
                        }
                    }
                }
                break;
            case ProjectileTrailStyle.MissileMajor:
                /*
                 * 1. The sprite starts red, and stays red for a little bit,
                 * 2. then changes to gray
                 * 3. and quickly fades out
                 * 4. Then (if it is every other), they grey returns briefly before fading out again
                 */

                // 1
                sr.color = CFXManager.inst.t_red;

                yield return new WaitForSeconds(0.1f);

                // 2
                sr.color = CFXManager.inst.t_gray;

                // 3
                elapsedTime = 0f;
                elapsedTime = 0.2f;

                while (elapsedTime < duration)
                {
                    if (obj != null)
                    {
                        Color setColor = obj.GetComponent<SpriteRenderer>().color;
                        setColor.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                        obj.GetComponent<SpriteRenderer>().color = setColor;

                        elapsedTime += Time.deltaTime;
                        yield return null; // Wait for the next frame
                    }
                    else
                    { // In case we delete this tile while its animating
                        yield break;
                    }
                }

                // 4
                if (flair)
                {
                    yield return new WaitForSeconds(0.05f);

                    sr.color = CFXManager.inst.t_gray;

                    elapsedTime = 0f;

                    while (elapsedTime < duration)
                    {
                        if (obj != null)
                        {
                            Color setColor = obj.GetComponent<SpriteRenderer>().color;
                            setColor.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                            obj.GetComponent<SpriteRenderer>().color = setColor;

                            elapsedTime += Time.deltaTime;
                            yield return null; // Wait for the next frame
                        }
                        else
                        { // In case we delete this tile while its animating
                            yield break;
                        }
                    }
                }
                break;
            case ProjectileTrailStyle.None:
                break;
        }
        
        yield return null;

        trailObjects.Remove(HF.V3_to_V2I(obj.transform.position));
        Destroy(obj);
    }

    #endregion
}
