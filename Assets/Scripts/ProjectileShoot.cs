using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShoot : MonoBehaviour{
    [Header("Input Stuff:")]
    [SerializeField]KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField]GameObject projectile;
    [SerializeField]Transform gunPoint;
    [SerializeField]float shootForce;
    [SerializeField]float projectileTime;
    [SerializeField]float range;

    private void Update() {
        if(Input.GetKeyDown(fireKey)){
            Shoot();
        }
    }

    public void Shoot(){
        GameObject bullet = Instantiate(projectile,gunPoint.position,Quaternion.LookRotation(gunPoint.forward, gunPoint.up));
        bullet.GetComponent<Rigidbody>().AddForce(gunPoint.forward * shootForce);
        Destroy(bullet,projectileTime);
    }
}
