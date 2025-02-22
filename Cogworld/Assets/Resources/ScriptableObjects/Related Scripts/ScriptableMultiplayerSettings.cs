using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SO Systems",menuName = "SO Systems/ScriptableMultiplayer")]
[Tooltip("Contains information regarding settings to be used during a multiplayer session.")]
public class ScriptableMultiplayerSettings : ScriptableObject
{
    public bool friendlyfire = false; // Players can damage each-other.
    
    public bool sharedHealthPool = true; // Players share the same (larger) health pool.

    public int maxAllowedDesync = 10; // Maximum allowed FORWARD desync between players.
}
