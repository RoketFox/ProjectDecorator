using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private Transform spawn;
    [SerializeField] private string[] tagsTeleportToSpawnOnEnteringDeadZone = new string[2];
    [SerializeField] private bool onExitDeadZone = false;

    void Start()
    {
        if (spawn == null)
        {
            if (GameObject.FindGameObjectWithTag("Spawn") != null)
                spawn = GameObject.FindGameObjectWithTag("Spawn").transform;
            else
                spawn.position = Vector3.zero;
        }
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onExitDeadZone)
        {
            if (tagsTeleportToSpawnOnEnteringDeadZone.Length > 0)
            {
                for (int i = 0; i < tagsTeleportToSpawnOnEnteringDeadZone.Length; i++)
                {
                    if (other.gameObject.tag.Equals(tagsTeleportToSpawnOnEnteringDeadZone[i]))
                        other.gameObject.transform.position = spawn.position;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (onExitDeadZone)
        {
            if (tagsTeleportToSpawnOnEnteringDeadZone.Length > 0)
            {
                for (int i = 0; i < tagsTeleportToSpawnOnEnteringDeadZone.Length; i++)
                {
                    if (other.gameObject.tag.Equals(tagsTeleportToSpawnOnEnteringDeadZone[i]))
                        other.gameObject.transform.position = spawn.position;
                }
            }
        }
    }

}
