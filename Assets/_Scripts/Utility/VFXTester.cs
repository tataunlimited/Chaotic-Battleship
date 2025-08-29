using UnityEngine;

public class VFXTester : MonoBehaviour
{
    public GameObject explosionVFX;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }
    }
}
