using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour{
    public UnityEvent OnHitEvent;

    public void Hit(){
        OnHitEvent.Invoke();
    }

    public void Die(){
        if(TryGetComponent(out Animator anim)){
            anim.SetTrigger("Die");
            // GetComponent<CapsuleCollider>().height = 0.1f;
            Destroy(gameObject,5);
        }
    }

    public void Hook(){
        
    }
}
