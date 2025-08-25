using System;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
   public static VFXManager Instance;

   public GameObject explosionPrefab;

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
   }

   public void SpawnSunkEffect(Vector3 vector3)
   {
      Debug.Log("SHIP SUNK!!!");
   }
}
