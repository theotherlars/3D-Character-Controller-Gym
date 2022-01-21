using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    [Header("Input Stuff:")]
    [SerializeField]KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField]KeyCode aimKey = KeyCode.Mouse1;
    
    [Header("Gun Stuff:")]
    [SerializeField] float shootDistance;
    [SerializeField]Transform gunpos;
    [SerializeField]LayerMask whatToShoot;
    [SerializeField]Vector3 aimPos;

    [Header("Visual Stuff:")]
    [SerializeField] ParticleSystem HitEffect;
    [SerializeField] ParticleSystem MuzzleFlash;
    
    [Header("Debug Stuff:")]
    [SerializeField]bool debug;
    
    Camera cam;
    Vector3 tempGunPos;
    Animator anim;

    void Start(){
        cam = Camera.main;
        anim = GetComponentInParent<Animator>();
        tempGunPos = gunpos.localPosition;
    }

    void Update(){
        if(Input.GetKeyDown(fireKey)){
            Shoot();
        }
        if(Input.GetKeyDown(aimKey)){
            Aim();
        }
        if(Input.GetKeyUp(aimKey)){
            UnAim();
        }
    }

    private void Shoot(){
        
        MuzzleFlash.Play();
        anim.SetTrigger("Shoot");

        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position + cam.transform.forward, cam.transform.forward, out hit, shootDistance, whatToShoot)){
            ParticleSystem effect = Instantiate(HitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(effect,HitEffect.main.duration);            
            if(debug){Debug.Log(hit.collider.name);}
            if(hit.collider.TryGetComponent(out Target target)){
                target.Hit();
            }
        }
    }

    private void Aim(){
        gunpos.localPosition = aimPos;
    }

    private void UnAim(){
        gunpos.localPosition = tempGunPos;
    }
}
