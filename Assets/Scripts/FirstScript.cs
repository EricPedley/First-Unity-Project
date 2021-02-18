using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScript : MonoBehaviour
{
    public Collider thisCollider;
    private void OnTriggerEnter(Collider other) {
        print("in da trash!");
        Physics.IgnoreCollision(other,thisCollider);
        Destroy(other.gameObject,0.2f);
    }
}
