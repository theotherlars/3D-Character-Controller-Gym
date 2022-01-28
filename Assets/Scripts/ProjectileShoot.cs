using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShoot : MonoBehaviour{
    [Header("Input Stuff:")]
    [SerializeField]KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField]KeyCode reloadKey = KeyCode.R;
    
    [Header("Gun Specific stuff:")]
    [SerializeField]float shootForce;
    [SerializeField]bool shouldDestoryAfterTime;
    [SerializeField]float projectileTime;
    [SerializeField]float range;
    [SerializeField]int magazineSize;

    
    [Header("Technical stuff: ")]
    [SerializeField]GameObject projectile;
    [SerializeField]Transform gunPoint;

    // PRIVATE
    int currentMagazine;

    private void Start() {
        currentMagazine = magazineSize;
    }

    private void Update() {
        if(Input.GetKeyDown(fireKey) && currentMagazine > 0){
            Shoot();
        }

        if(Input.GetKeyDown(reloadKey)){
            currentMagazine = magazineSize;
        }
        UIScript.Instance.currentMagazine = currentMagazine;
    }

    public void Shoot(){
        currentMagazine--;
        GameObject bullet = Instantiate(projectile,gunPoint.position,Quaternion.LookRotation(gunPoint.forward, gunPoint.up));
        if(shouldDestoryAfterTime){
            Destroy(bullet,projectileTime);
        }
    }
}
