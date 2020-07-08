using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{

    private AudioClip[] footstepSounds;

    // locals
    private AudioSource aud;

    private void Awake()
    {
        aud = GetComponent<AudioSource>();

        footstepSounds = Resources.LoadAll<AudioClip>("SFX/FootSteps/Grass/");
    }

    public void Footstep()
    {
        aud.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }

}
