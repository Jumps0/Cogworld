using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMachine : MonoBehaviour
{

    public string _name = "";
    public Vector2Int _size;
    [Tooltip("If this machine explodes upon destruction. To hit name should be orange.")]
    public bool explosive = false;


    [Header("Audio")]
    public AudioSource _source;
    public AudioClip _ambient;

    [Header("Special Flags")]
    public bool s_detonate = false;
    public int detonate_timer = 15;
    public bool s_unstable = false;


}
