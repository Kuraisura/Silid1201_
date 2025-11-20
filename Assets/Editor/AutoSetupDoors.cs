using UnityEngine;
using UnityEditor;

public class AutoSetupDoors
{
    [MenuItem("Tools/Door/Setup Doors With Script")]
    public static void SetupDoors()
    {
        var player = GameObject.FindWithTag("Player");
        var all = GameObject.FindObjectsOfType<GameObject>();
        // DoorRotator logic removed. No longer adding or searching for DoorRotator.
        Debug.Log($"AutoSetupDoors: processed scene. DoorRotator references removed.");
    }
}
