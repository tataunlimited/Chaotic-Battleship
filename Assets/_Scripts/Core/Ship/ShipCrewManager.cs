using System.Collections.Generic;
using UnityEngine;

public class ShipCrewManager : MonoBehaviour
{
	[Header("Crew Settings")]
	[Tooltip("The CapsuleCrew prefab to spawn.")]
	public GameObject capsulePrefab;
	[Tooltip("Number of crew members to spawn on this ship.")]
	public int crewCount = 15;

	[Header("Movement Targets")]
	[Tooltip("Empty GameObjects placed on the deck where crew should walk.")]
	public Transform[] targetPoints;

	private List<GameObject> crewMembers = new List<GameObject>();

	void Start()
	{
		// 1. Find all target points on the ship (the children you placed on the deck)
		if (targetPoints.Length == 0)
		{
			Debug.LogError("No target points assigned! Crew won't move.");
			return;
		}

		// 2. Spawn and configure the crew
		for (int i = 0; i < crewCount; i++)
		{
			GameObject newCrew = Instantiate(capsulePrefab, transform);
			newCrew.name = "Crewman_" + i;

			// Set initial position to a random target point
			int randomIndex = Random.Range(0, targetPoints.Length);
			newCrew.transform.localPosition = targetPoints[randomIndex].localPosition;

			// Pass the targets to the movement script
			CrewMover mover = newCrew.GetComponent<CrewMover>();
			if (mover != null)
			{
				mover.SetTargets(targetPoints);
			}

			crewMembers.Add(newCrew);
		}
	}

	/// <summary>
	/// Called by the ShipHealth script when the ship is destroyed.
	/// Implements Acceptance Criteria 4 (Go away) and Bonus 1 (Jump off).
	/// </summary>
	public void OnShipDestroyed()
	{
		float jumpPower = 5f;

		foreach (GameObject crew in crewMembers)
		{
			if (crew != null)
			{
				// 1. Detach from the ship (ship's movement/rotation no longer affects them)
				crew.transform.parent = null;

				// 2. Prepare Rigidbody for physics simulation
				Rigidbody rb = crew.GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = false; // Turn on physics simulation

					// 3. Apply force (Jump off)
					Vector3 randomForce = new Vector3(
						Random.Range(-2f, 2f),
						jumpPower,
						Random.Range(-2f, 2f));

					rb.AddForce(randomForce, ForceMode.Impulse);
				}

				// 4. Start cleanup timer for the detached crew member
				CrewPhysicsCleanup cleanup = crew.AddComponent<CrewPhysicsCleanup>();
				cleanup.SetCleanupTime(5f); // Destroy the capsule after 5 seconds
			}
		}

		crewMembers.Clear();
		// NOTE: The ship itself should be destroyed after this function runs.
	}
}