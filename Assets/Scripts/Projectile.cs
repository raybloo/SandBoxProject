using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public GameObject shooter;
    public float speed;
    public float radius;
    public float damage;
    public float explosionForce;
    public ParticleSystem explosionParticle;

    public float maxStep = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        explosionParticle = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        float remainingMomentum = speed * Time.deltaTime;
        while(remainingMomentum > maxStep) {
            transform.position += transform.forward * maxStep;
            if(Physics.CheckSphere(transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
                Destroy(gameObject);
                return;
            }
            remainingMomentum -= maxStep;
        }
        transform.position += transform.forward * remainingMomentum;
        if (Physics.CheckSphere(transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
            Destroy(gameObject);
        }
    }
}
