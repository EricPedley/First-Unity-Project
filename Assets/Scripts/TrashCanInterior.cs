using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanInterior : MonoBehaviour
{
    public Collider thisCollider;
    private ParticleSystem particles;
    void Start() {
        particles = GetComponentInChildren<ParticleSystem>();
    }
    private void OnTriggerEnter(Collider other) {
        print("in da trash!");
        Physics.IgnoreCollision(other,thisCollider);
        Destroy(other.gameObject,0.2f);
        particles.Play();
    }
}
