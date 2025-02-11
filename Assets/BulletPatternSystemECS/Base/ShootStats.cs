
using UnityEngine;

[CreateAssetMenu(fileName = "ShootSystemData", menuName = "ScriptableObjects/ShootSystemData")]
public class ShootStats : ScriptableObject
{
    public int MagazineCount = 3; // How many times can this weapon reload
    public int MagazineCapacity = 3; // How many Ammo per reload 
    public float ReloadDelay = 1.0f; // How long the reload takes
    public float ShootDelay = 0.2f; // How long the shooting takes
}