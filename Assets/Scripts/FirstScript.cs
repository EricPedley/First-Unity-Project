using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        Debug.Log("helo other object!");
    }
}
