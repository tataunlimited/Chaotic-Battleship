using System;
using UnityEngine;
using UnityEngine.Audio;

public class VFXManager : MonoBehaviour
{
   public static VFXManager Instance;

   public GameObject explosionPrefab;
    public GameObject shipDestroyedExplosionPrefab;
   
    [SerializeField] private AudioSource hitSound;
    [SerializeField] private AudioSource shipSunkSound;
    [SerializeField] private AudioSource fireSound;

    private void Awake()
   {
      Instance = this;
   }

   public void SpawnExplosion(Vector3 position)
   {
      Instantiate(explosionPrefab, position, Quaternion.identity);
   }

   public void SpawnHitEffect(Vector3 vector3)
   {
      Debug.Log("SHIP HIT!!!");
        if (hitSound != null)
        {
            hitSound.Play();
            //AudioSource.PlayClipAtPoint(hitSound, vector3);
        }
    }

    public void SpawnSunkEffect(Vector3 vector3)
    {
        Debug.Log("SHIP SUNK!!!");
        if (shipSunkSound != null)
        {
            shipSunkSound.Play();
            //AudioSource.PlayClipAtPoint(shipSunkSound, vector3);
        }
        Instantiate(shipDestroyedExplosionPrefab, vector3, Quaternion.identity);
    }
   
   public void PlayFireSound()
   {
       fireSound.Play();
   }
}
