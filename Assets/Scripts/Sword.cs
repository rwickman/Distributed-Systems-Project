﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public string swordAnimName = "SwordAttack";
    public float hitRate = 0.5f;
    public float hitForce = 10f;
    public int hitDamage = 2;

    private Animator m_anim;
    private Collider m_coll;
    private float elapsedTime = 0f, animElapsedTime = 0f;
    private float hitAnimTime;
    private HashSet<int> objectsHitAlready = new HashSet<int>();
    // Start is called before the first frame update
    void Start()
    {
        m_anim = gameObject.GetComponent<Animator>();
        m_coll = gameObject.GetComponent<Collider>();

        RuntimeAnimatorController ac = m_anim.runtimeAnimatorController;
        for (int i = 0; i < ac.animationClips.Length; i++) {
            if (ac.animationClips[i].name == swordAnimName) {
                hitAnimTime = ac.animationClips[i].length;
            }
        }   
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
           
        }

        if (animElapsedTime > hitAnimTime && objectsHitAlready.Count > 0) {
            objectsHitAlready = new HashSet<int>();
        }
    }

    private void OnTriggerStay(Collider other) {
         print("TRIGGERING");
        Health otherHealth = other.gameObject.GetComponent<Health>();
        
        if(otherHealth != null && 
           animElapsedTime < hitAnimTime && 
           !m_coll.transform.IsChildOf(other.transform) &&
           !objectsHitAlready.Contains(other.GetInstanceID())) {
            
            objectsHitAlready.Add(other.GetInstanceID());
            Vector3 dir = other.transform.position - transform.position;
            Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>(); 
            otherRb.AddForce(dir.normalized * hitForce,  ForceMode.Impulse);
            otherHealth.Hurt(hitDamage);
        }
    }

}
