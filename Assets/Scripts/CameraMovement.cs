using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    void Start() {
        Update();
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position+new Vector3(0,3,-4)-player.GetComponent<Rigidbody>().velocity;
    }
}
