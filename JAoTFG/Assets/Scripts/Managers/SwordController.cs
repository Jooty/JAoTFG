using UnityEngine;

public class SwordController : MonoBehaviour
{

    [SerializeField] private AudioClip soundFXHit;

    private GameObject owner;

    private void Start()
    {
        owner = transform.FindParentWithTag("Player").gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "TitanNape")
        {
            other.GetComponent<TitanNape>().Hit();
            owner.GetComponent<AudioSource>().PlayOneShot(soundFXHit, .2f);
        }
    }

}