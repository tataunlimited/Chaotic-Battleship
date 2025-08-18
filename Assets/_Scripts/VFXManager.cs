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
   
}
