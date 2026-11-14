using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RemoveDuplicateLightProbes : EditorWindow
{
    [MenuItem("Tools/Lighting/Remove Duplicate Light Probes")]
    static void RemoveDuplicates()
    {
        LightProbeGroup[] groups =
            FindObjectsOfType<LightProbeGroup>(true);

        int totalRemoved = 0;

        foreach (LightProbeGroup group in groups)
        {
            Vector3[] positions = group.probePositions;
            HashSet<Vector3> uniquePositions = new HashSet<Vector3>();
            List<Vector3> cleanedPositions = new List<Vector3>();

            foreach (Vector3 position in positions)
            {
                if (uniquePositions.Add(position))
                {
                    cleanedPositions.Add(position);
                }
                else
                {
                    totalRemoved++;
                }
            }
        }

        if (totalRemoved == 0)
        {
            EditorUtility.DisplayDialog(
                "Light Probes",
                "No duplicate probes found.",
                "OK"
            );
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Duplicate Light Probes Found",
            $"Found {totalRemoved} duplicate probes. Remove them?",
            "Remove",
            "Cancel"))
            return;

        foreach (LightProbeGroup group in groups)
        {
            Vector3[] positions = group.probePositions;
            HashSet<Vector3> uniquePositions = new HashSet<Vector3>();
            List<Vector3> cleanedPositions = new List<Vector3>();

            foreach (Vector3 position in positions)
            {
                if (uniquePositions.Add(position))
                    cleanedPositions.Add(position);
            }

            if (cleanedPositions.Count != positions.Length)
            {
                Undo.RecordObject(group, "Remove Duplicate Light Probes");
                group.probePositions = cleanedPositions.ToArray();
                EditorUtility.SetDirty(group);
            }
        }

        Debug.Log($"Removed {totalRemoved} duplicate Light Probes.");
        EditorUtility.DisplayDialog(
            "Complete",
            $"Removed {totalRemoved} duplicate probes.",
            "OK"
        );
    }
}