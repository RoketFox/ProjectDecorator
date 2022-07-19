using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToSpawn : MonoBehaviour
{
    [SerializeField] private GameObject spawn;

    void Start()
    {
        spawn = GameObject.Find("Spawn");
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = spawn.transform.position;
    }

}
