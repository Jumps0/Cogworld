using DungeonResources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SO Systems",menuName = "SO Systems/ScriptablePreferences")]
[Tooltip("Contains information regarding player gameplay preferences, passed from Main Menu to Gameplay Scene and saved inside a ScriptableObject.")]
public class ScriptablePreferences : ScriptableObject
{
    [Header("Enemies")]
    public bool squads_investigation = true; // Use investigation squads
    public bool squads_extermination = true; // Use extermination squads
    [Range(100, 1000)] // 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
    public int extermination_mtth = 500; // Mean-time-to-happen for extermination squads (this is a range but we can't really represent that in the UI)

    [Header("Hacking")]
    [Range(0.05f, 1f)] // 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    public float hacking_baseDetectionChance = 0.1f;

    [Header("Evolution")]
    public bool evolve_healBetweenFloors = true;
    public bool evolve_clearCorruption = true;
    [Range(50, 500)] // 50, 100, 150, 200, 300, 400, 500
    public int evolve_newHealthPerLevel = 150; // The amount of health that will get added on to the player's HP per evolution

    [Header("Corruption")]
    public bool corruption_enabled = true; // Disabling this makes player immune to corruption
    public bool corruption_effects = true; // Enable corruption effects

}
