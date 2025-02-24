using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Projectile_Generic : MonoBehaviour
{
    [Header("References")]
    public GameObject _projectile;
    public GameObject _highlight;

    [Header("Values")]
    public Vector2Int _target;
    public Vector2Int _origin;
    //
    public Color projColor;
    public Color highlightColor;
    //
    public float _speed = 0.5f;
    public bool _accurate;

    public void Setup(Vector2Int origin, Vector2Int target, ItemProjectile weapon, float speed, bool isAccurate)
    {
        _origin = origin;
        _target = target;
        _speed = speed;
        _accurate = isAccurate;

        projColor = weapon.projectileColor;
        Color boxColor = new Color(projColor.r, projColor.g, projColor.b, 0.7f);

        _projectile.GetComponent<Image>().color = projColor;
        _highlight.GetComponent<Image>().color = boxColor;

        StartCoroutine(MoveProjectile());
        StartCoroutine(LifetimeDestroy());
    }

    private IEnumerator MoveProjectile()
    {
        // calculate the direction towards the target
        Vector3 direction = new Vector3(_target.x, _target.y) - _projectile.transform.position;
        direction.z = 0f; // ensure the projectile stays in the 2D plane

        while (true)
        {
            if (_projectile == null || _highlight == null)
            {
                OnReachTarget();
            }

            // rotate the projectile towards the target
            _projectile.transform.up = direction.normalized;

            // calculate the distance to the target
            float distanceToTarget = direction.magnitude;

            if (distanceToTarget <= 0.01f)
            {
                if (_accurate)
                {
                    _projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                    _highlight.transform.position = new Vector3(_target.x, _target.y, 0);
                }
                else
                {
                    // Determine a random direction to miss the target by
                    Vector2 randomOffset = Random.insideUnitCircle.normalized;
                    Vector3 missTarget = new Vector3(_target.x, _target.y) + new Vector3(randomOffset.x, randomOffset.y, 0f);

                    _projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, (missTarget - new Vector3(_origin.x, _origin.y)).normalized);
                    _highlight.transform.position = new Vector3(Mathf.RoundToInt(missTarget.x), Mathf.RoundToInt(missTarget.y), missTarget.z);
                }

                // destroy the projectile if it has reached the target
                OnReachTarget();
                yield break;
            }
            else
            {
                // move the projectile towards the target at the specified speed
                float step = _speed * Time.deltaTime;
                _projectile.transform.position = Vector3.MoveTowards(_projectile.transform.position, new Vector3(_target.x, _target.y), step);

                // snap the highlight to the nearest whole (int) number
                Vector3 snapPosition = _projectile.transform.position;
                snapPosition.x = Mathf.Round(snapPosition.x);
                snapPosition.y = Mathf.Round(snapPosition.y);
                _highlight.transform.position = snapPosition;

                yield return null;
            }

            if (_target == null || _origin == null || _projectile == null || _highlight == null)
            {
                OnReachTarget();
            }

            // update the direction towards the target every frame
            direction = new Vector3(_target.x, _target.y) - _projectile.transform.position;
            direction.z = 0f; // ensure the projectile stays in the 2D plane
        }
    }

    public void OnReachTarget()
    {
        UIManager.inst.projectiles.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    IEnumerator LifetimeDestroy()
    {
        yield return new WaitForSeconds(5f);

        OnReachTarget();
    }
}
