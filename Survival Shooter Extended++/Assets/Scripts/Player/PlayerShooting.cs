﻿using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;                  
    public static float timeBetweenBullets = 0.2f;        
    public float range = 100f;
    public static int shootCount = 1;

    public bool bouncing  = true;
    public float bounceRadius = 100f;                      
    public int bounceCount = 5;
    public static bool explosive = false;
    public static float explosionRadius = 10f;

    float timer;                                    
    Ray shootRay = new Ray();                                   
    RaycastHit shootHit;    
    RaycastHit shootHitOpaqueObj;                         
    int shootableMask;                             
    int opaqueMask;
    ParticleSystem gunParticles;                    
    LineRenderer gunLine;                           
    AudioSource gunAudio;                           
    ParticleSystem explosionParticle;
    Light gunLight;                                 
    float effectsDisplayTime = 0.2f;                

    void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        opaqueMask = LayerMask.GetMask("Opaque");
        gunParticles = GetComponent<ParticleSystem>();
        explosionParticle = GameObject.FindGameObjectWithTag("ExplosionAnim").GetComponent<ParticleSystem>();
        gunLine = GetComponent<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        gunLight = GetComponent<Light>();
        
    }

    void Update()
    {
        GameObject exp = transform.Find("Explosion").gameObject;
        exp.SetActive(explosive);
        PowerManager.power = damagePerShot;
        timer += Time.deltaTime;

        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            Shoot();
        }

        if (timer >= timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects();
        }
    }

    public void DisableEffects()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
        explosionParticle.Stop();
    }

    void Shoot()
    {
        // Normal shoot
        timer = 0f;

        gunAudio.Play();

        gunLight.enabled = true;

        gunParticles.Stop();
        gunParticles.Play();

        gunLine.enabled = true;
        
        gunLine.positionCount = shootCount * 2;
        shootRay.origin    = transform.position;

        // shootRay.origin.Set(shootRay.origin.x, 0f, shootRay.origin.z);

        for (int i = 0; i < shootCount; i++) {
            gunLine.SetPosition(i*2, transform.position);

            shootRay.direction = Quaternion.Euler(0f, 15f*i - shootCount * 7.5f, 0f) * transform.forward;

            bool directHit = Physics.Raycast(shootRay, out shootHit, range, shootableMask);
            bool wall      = Physics.Raycast(shootRay, out shootHitOpaqueObj, range, opaqueMask);
            if (directHit)
            {
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                }

                gunLine.SetPosition(1 + 2*i, shootHit.point);
            }
            else if (wall)
                gunLine.SetPosition(1 + 2*i, shootHitOpaqueObj.point);
            else
                gunLine.SetPosition(1 + 2*i, shootRay.origin + shootRay.direction * range);
            

            // Explosive
            if (explosive) {
                explosionParticle.startSize = explosionRadius;

                Collider[] enemyWithinRadius = Physics.OverlapSphere(shootHit.point, explosionRadius, shootableMask);
                foreach (var enemy in enemyWithinRadius) {
                    if (enemy.GetType() == typeof(CapsuleCollider)) {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        enemyHealth.TakeDamage(damagePerShot, enemy.transform.position);
                    }
                }

                enemyWithinRadius = Physics.OverlapSphere(shootHitOpaqueObj.point, explosionRadius, shootableMask);
                foreach (var enemy in enemyWithinRadius) {
                    if (enemy.GetType() == typeof(CapsuleCollider)) {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        enemyHealth.TakeDamage(damagePerShot, enemy.transform.position);
                    }
                }
            
                if (directHit)
                    explosionParticle.transform.position = shootHit.point;
                else if (wall)
                    explosionParticle.transform.position = shootHitOpaqueObj.point;

                if (explosive && (directHit || wall))
                    explosionParticle.Play();
            }

            if (bouncing) {
                gunLine.positionCount = 2 + bounceCount;
                
                int currentBounceCount = 0;
                Collider[] enemyWithinRadius = Physics.OverlapSphere(shootHit.point, bounceRadius, shootableMask);
                foreach (var enemy in enemyWithinRadius) {
                    if (currentBounceCount < bounceCount && enemy.GetType() == typeof(CapsuleCollider)) {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        enemyHealth.TakeDamage(damagePerShot, enemy.transform.position);
                        gunLine.SetPosition(2 + currentBounceCount, enemy.transform.position);
                        currentBounceCount++;
                    }
                }

                enemyWithinRadius = Physics.OverlapSphere(shootHitOpaqueObj.point, bounceRadius, shootableMask);
                foreach (var enemy in enemyWithinRadius) {
                    if (currentBounceCount < bounceCount && enemy.GetType() == typeof(CapsuleCollider)) {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        enemyHealth.TakeDamage(damagePerShot, enemy.transform.position);
                        gunLine.SetPosition(2 + currentBounceCount, enemy.transform.position);
                        currentBounceCount++;
                    }
                }
            }
        }

    }
}