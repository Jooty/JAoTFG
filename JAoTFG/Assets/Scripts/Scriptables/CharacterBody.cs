using System.Linq;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    [HideInInspector] public Collider Collider;
    [HideInInspector] public CharacterSFXBase[] allSFX;

    private CharacterController controller;

    // global managers
    private AudioManager audioManager;

    private void Awake()
    {
        this.Collider = GetComponent<Collider>();

        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Start()
    {
        controller = transform.parent.GetComponent<CharacterController>();

        allSFX = transform.GetComponentsInChildren<CharacterSFXBase>(true);
    }

    public void PlaySFXParticles(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        sfx.particles.Play();
    }

    public void StopSFXParticles(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        sfx.particles.Stop();
    }

    public void PlaySFXTrail(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        sfx.trailRenderer.enabled = true;
    }

    public void StopSFXTrail(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        sfx.trailRenderer.enabled = false;
    }

    public void PlaySFXTrails(CharacterSFXType type)
    {
        var sfx = getMultipleSFXFromType(type);
        if (sfx.Length == 0)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }
        else if (sfx[0].trailRenderer.enabled == true) return;

        for (int i = 0; i < sfx.Length; i++)
        {
            sfx[i].trailRenderer.enabled = true;
        }
    }

    public void StopSFXTrails(CharacterSFXType type)
    {
        var sfx = getMultipleSFXFromType(type);
        if (sfx.Length == 0)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }
        else if (sfx[0].trailRenderer.enabled == false) return;


        for (int i = 0; i < sfx.Length; i++)
        {
            sfx[i].trailRenderer.enabled = false;
        }
    }

    public void PlaySFXAudio(CharacterSFXType type, AudioClip clip)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }
        else if (sfx.audioSource.clip == clip && sfx.audioSource.isPlaying) return;

        sfx.audioSource.loop = true;
        sfx.audioSource.clip = clip;
        audioManager.StartFadeIn(sfx.audioSource, .5f, 0.05f);
    }

    public void PlaySFXAudioOneShot(CharacterSFXType type, AudioClip clip)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        sfx.audioSource.PlayOneShot(clip);
    }

    public void StopSFXAudio(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        audioManager.StartFadeOut(sfx.audioSource, .15f);
    }

    public void StopAllSFX(CharacterSFXType type)
    {
        var sfx = getSFXFromType(type);
        if (!sfx)
        {
            Debug.LogWarning($"SFX type {type} could not be found on {gameObject.name}!");
            return;
        }

        audioManager.StartFadeOut(sfx.audioSource, .15f);
        sfx.particles.Stop();
    }

    public CharacterSFXBase getSFXFromType(CharacterSFXType type)
    {
        return allSFX.First(x => x.SFXType == type);
    }

    public CharacterSFXBase[] getMultipleSFXFromType(CharacterSFXType type)
    {
        return allSFX.Where(x => x.SFXType == type).ToArray();
    }

    public void ColliderEvent(Collision coll)
    {
        controller.CharacterBodyColliderEvent(coll);
    }
}