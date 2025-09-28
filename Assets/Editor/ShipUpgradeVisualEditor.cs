#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Core.Ship.Editor
{
    /// <summary>
    /// Custom editor for the ShipUpgradeVisual class.
    /// Adds a button to automatically find and assign child GameObjects
    /// based on their names.
    /// </summary>
    [CustomEditor(typeof(ShipUpgradeVisual))]
    public class ShipUpgradeVisualEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector fields (movementUpgrade, attackUpgrade, etc.)
            DrawDefaultInspector();

            // Get a reference to the script this editor is inspecting.
            ShipUpgradeVisual script = (ShipUpgradeVisual)target;

            // Add a button to the inspector.
            if (GUILayout.Button("Auto-Assign Visuals from Children"))
            {
                // When the button is pressed, run the assignment logic.
                FindAndAssignVisuals(script);
            }
        }

        /// <summary>
        /// Searches through the children of the script's GameObject and assigns them
        /// to the appropriate fields in the ShipUpgradeElement classes.
        /// </summary>
        /// <param name="script">The ShipUpgradeVisual instance to modify.</param>
        private void FindAndAssignVisuals(ShipUpgradeVisual script)
        {
            // We use SerializedObject and SerializedProperty to make changes
            // to the script's fields. This is the standard and safest way,
            // as it handles undo/redo and marks the scene as dirty for saving.
            SerializedObject serializedScript = new SerializedObject(script);

            // Find the properties for each upgrade type.
            SerializedProperty movementProp = serializedScript.FindProperty("movementUpgrade");
            SerializedProperty attackProp = serializedScript.FindProperty("attackUpgrade");
            SerializedProperty armorProp = serializedScript.FindProperty("armorUpgrade");
            SerializedProperty specialProp = serializedScript.FindProperty("specialAbilityUpgrade");

            int assignments = 0;

            // Get all Transform components in children, including inactive ones, to search recursively.
            Transform[] allChildren = script.GetComponentsInChildren<Transform>(true);

            // Loop through every descendant of the GameObject this script is on.
            foreach (Transform child in allChildren)
            {
                // Skip the parent object itself to avoid assigning it.
                if (child == script.transform)
                {
                    continue;
                }

                string childName = child.gameObject.name.ToLower(); // Use lowercase for case-insensitive search

                // Check for the upgrade type in the child's name
                if (childName.Contains("movement"))
                {
                    if (AssignToLevel(movementProp, child.gameObject, childName)) assignments++;
                }
                else if (childName.Contains("attack"))
                {
                    if (AssignToLevel(attackProp, child.gameObject, childName)) assignments++;
                }
                else if (childName.Contains("armor"))
                {
                    if (AssignToLevel(armorProp, child.gameObject, childName)) assignments++;
                }
                else if (childName.Contains("special"))
                {
                    if (AssignToLevel(specialProp, child.gameObject, childName)) assignments++;
                }
            }

            // Apply all the changes we've made to the serialized properties.
            serializedScript.ApplyModifiedProperties();

            Debug.Log($"Auto-assignment complete. Found and assigned {assignments} GameObjects.");
        }

        /// <summary>
        /// Assigns a GameObject to the correct level field within a ShipUpgradeElement property.
        /// </summary>
        /// <param name="upgradeProp">The SerializedProperty representing the ShipUpgradeElement (e.g., movementUpgrade).</param>
        /// <param name="objToAssign">The child GameObject to assign.</param>
        /// <param name="name">The lowercase name of the child GameObject.</param>
        /// <returns>True if an assignment was made, otherwise false.</returns>
        private bool AssignToLevel(SerializedProperty upgradeProp, GameObject objToAssign, string name)
        {
            if (name.Contains("lvl1"))
            {
                upgradeProp.FindPropertyRelative("level1").objectReferenceValue = objToAssign;
                return true;
            }
            if (name.Contains("lvl2"))
            {
                upgradeProp.FindPropertyRelative("level2").objectReferenceValue = objToAssign;
                return true;
            }
            if (name.Contains("lvl3"))
            {
                upgradeProp.FindPropertyRelative("level3").objectReferenceValue = objToAssign;
                return true;
            }
            return false;
        }
    }
}
#endif
