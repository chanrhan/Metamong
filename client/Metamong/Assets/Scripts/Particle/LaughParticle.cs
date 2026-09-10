using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaughParticle : MonoBehaviour
{
    [SerializeField]
    private bool active = true;
    
    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] particles;
    private ParticleSystem.Particle particle;
    private Vector3 direction;
    private int numParticlesAlive;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();       
    }

    void Start()
    {
        particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
    }

    void LateUpdate()
    {
        if(!active){
            return;
        }
        
        // 파티클을 이동 방향으로 회전
        numParticlesAlive = _particleSystem.GetParticles(particles);
        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 v = particles[i].velocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            particles[i].rotation3D = new Vector3(0f, 0f, angle - 90f);
        }
        _particleSystem.SetParticles(particles, numParticlesAlive);
    }
}
