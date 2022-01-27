using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour{

    [SerializeField]ParticleSystem rocketTrailParticles;
    [SerializeField]Transform trailPos;
    [SerializeField]ParticleSystem explosionParticles;
    [SerializeField]float explosionForce;
    [SerializeField]float hitRadius;
    [SerializeField]float explosionRadius;
    [SerializeField]float upwardsModifier;
    [SerializeField]LayerMask explosionLayers;
    [SerializeField]float travelSpeed;
    ParticleSystem trail;

    private void Start() {
        trail = Instantiate(rocketTrailParticles,trailPos.position,Quaternion.LookRotation(transform.forward,transform.up));
        trail.transform.parent = transform;
    }

    private void Update() {
        transform.position += transform.forward * travelSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other) {
        Destroy(trail);
        if(other.gameObject.CompareTag("Player")){
            if(other.gameObject.TryGetComponent(out PlayerMovement pm)){
                if(pm.isGrounded){
                    pm.LaunchIntoAir(explosionForce);
                }
            }
        }
        else{
            Collider[] hits = Physics.OverlapSphere(transform.position,hitRadius,explosionLayers);
            for (var i = 0; i < hits.Length; i++){
                if(hits[i].TryGetComponent<Rigidbody>(out Rigidbody rb)){
                    rb.AddExplosionForce(explosionForce,transform.position,explosionRadius,upwardsModifier,ForceMode.Impulse);
                }
                if(other.gameObject.TryGetComponent(out Target target)){
                    target.Die();
                }
            }
        }
        ParticleSystem explosion = Instantiate(explosionParticles,transform.position,Quaternion.identity);
        Destroy(explosion.gameObject,2f);
        Destroy(gameObject);
    }

}
