using UnityEngine;

public class CrewPhysicsCleanup : MonoBehaviour
{
    private float cleanupTime = 5f; // Default time
    private float timer;

    /// <summary>
    /// Sets the time before the object is destroyed.
    /// </summary>
    public void SetCleanupTime(float time)
    {
        cleanupTime = time;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= cleanupTime)
        {
            Destroy(gameObject); // Cleanup the crew capsule
        }
    }
}