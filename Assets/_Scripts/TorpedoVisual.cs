//Torpedo
using UnityEngine;

public class TorpedoVisual : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;

    public float degree = -20f;

    private float _timeAlive;

    void Start()
    {
        transform.eulerAngles = new Vector3(degree, transform.eulerAngles.y, transform.eulerAngles.z);
    }
    void Update()
    {
        // Move the torpedo forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Update the lifetime counter and destroy the object when it expires
        _timeAlive += Time.deltaTime;
        if (_timeAlive >= lifetime)
        {
            // To create a diving effect, we can move the torpedo down before destroying it.
            // This is a simple way to make it "despawn" underwater.
            transform.position += Vector3.down * speed * Time.deltaTime * 0.5f;

            if (_timeAlive >= lifetime) // Example: destroy 1 second after starting to dive
            {
                Destroy(gameObject);
            }
        }
    }
}