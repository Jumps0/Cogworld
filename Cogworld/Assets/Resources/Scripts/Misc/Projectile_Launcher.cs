using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private float _speed = 10f;

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

        // calculate the direction towards the target
        Vector3 direction = _target.position - _projectile.transform.position;
        direction.z = 0f; // ensure the projectile stays in the 2D plane

        while (true)
        {
            if (_target == null || _origin == null || _projectile == null || _highlight == null)
            {
                OnReachTarget();
            }

            // rotate the projectile towards the target
            _projectile.transform.up = direction.normalized;

            // calculate the distance to the target
            float distanceToTarget = direction.magnitude;

            if (distanceToTarget <= 0.01f)
            {
                _projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                _highlight.transform.position = new Vector3(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(_target.position.y), _target.position.z);

                // destroy the projectile if it has reached the target
                OnReachTarget();
                yield break;
            }
            else
            {
                // move the projectile towards the target at the specified speed
                float step = _speed * Time.deltaTime;
                _projectile.transform.position = Vector3.MoveTowards(_projectile.transform.position, _target.position, step);

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
            direction = _target.position - _projectile.transform.position;
            direction.z = 0f; // ensure the projectile stays in the 2D plane
        }
    }

    public void OnReachTarget()
    {
        _source.Stop();
        UIManager.inst.projectiles.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    IEnumerator LifetimeDestroy()
    {
        yield return new WaitForSeconds(5f);

        OnReachTarget();
    }
}
