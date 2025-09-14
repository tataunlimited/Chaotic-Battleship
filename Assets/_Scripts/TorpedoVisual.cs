//Torpedo
using UnityEngine;

public class TorpedoVisual : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;

    private float _timeAlive;

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

            // TODO Add a check here to destroy it after it's out of sight
            if (_timeAlive >= lifetime + 1f) // Example: destroy 1 second after starting to dive
            {
                Destroy(gameObject);
            }
        }
    }
}