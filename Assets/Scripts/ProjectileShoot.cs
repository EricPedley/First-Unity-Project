using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShoot : MonoBehaviour
{
    public GameObject pfBullet;
    public Transform fireRef;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            Debug.Log("mouse pressed");
            shootBullet();
        }
    }

    private void shootBullet() {
        print("fireref position is " + fireRef.position);
        print("gun position is " + transform.position);
        GameObject newBullet = Instantiate(pfBullet,fireRef.position,fireRef.rotation);
        newBullet.GetComponent<Rigidbody>().velocity = newBullet.transform.rotation*Vector3.forward*10;
    }
}
