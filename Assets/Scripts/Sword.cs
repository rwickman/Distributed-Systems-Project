using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public float hitRate = 0.5f;
    public int hitDamage = 2;
    
    public float hitForce = 10f;
    private Animator m_anim;
    private Collider m_coll;
    private float elapsedTime = 0f, animElapsedTime = 0f;
    private float hitAnimTime = 0.20f;
    // Start is called before the first frame update
    void Start()
    {
        m_anim = gameObject.GetComponent<Animator>();
        m_coll= gameObject.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        animElapsedTime += Time.deltaTime;

        if (Input.GetButton("Fire1") && elapsedTime >= hitRate) {      
            m_anim.SetTrigger("isAttacking");
            elapsedTime = 0f;
            animElapsedTime = 0f;
            m_coll.isTrigger = true;
            
        }
         
        if (m_coll.isTrigger && animElapsedTime > hitAnimTime) {
            m_coll.isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
         print("TRIGGERING");
        Health otherHealth = other.gameObject.GetComponent<Health>();
        if(otherHealth != null && animElapsedTime < hitAnimTime && !GameObject.ReferenceEquals(gameObject, other.gameObject)) {
            Vector3 dir = other.transform.position - transform.position;
            Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>(); 
            otherRb.AddForce(dir.normalized * hitForce,  ForceMode.Impulse);
            otherHealth.Hurt(hitDamage);
        }
    }

}
