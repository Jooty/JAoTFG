using UnityEngine;

public class CharacterSFXBase : MonoBehaviour
{

    public CharacterSFXType SFXType;

    [HideInInspector] public ParticleSystem particles;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public TrailRenderer trailRenderer;

    private void Awake()
    {
        this.audioSource = GetComponent<AudioSource>();
        this.particles = GetComponent<ParticleSystem>();
        this.trailRenderer = GetComponent<TrailRenderer>();

        trailRenderer.enabled = false;
    }

}
