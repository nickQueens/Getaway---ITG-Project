using UnityEngine;

public class CarAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource engineAudio;
    private Rigidbody rigidbody;
    private float maxVelocity = 40;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        engineAudio.pitch = 1 + (rigidbody.velocity.magnitude / maxVelocity);
    }
}
