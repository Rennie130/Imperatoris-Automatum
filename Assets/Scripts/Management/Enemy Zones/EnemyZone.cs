using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyZone : MonoBehaviour
{
    public List<EnemyPatrolPoint> PatrolPoints;

    public Vector3 GetRandomPatrolPosition()
    {
        var point = PatrolPoints[Random.Range(0, PatrolPoints.Count)];
        var position = point.transform.position;
        var offset = Random.insideUnitSphere * point.Radius;
        offset.y = 0;

        //find nearest point from random position
        if (NavMesh.SamplePosition(position + offset, out NavMeshHit hit, point.Radius, NavMesh.AllAreas))
            return hit.position;
        //if that fails, try to use patrol point position directly
        if (NavMesh.SamplePosition(position, out NavMeshHit hit2, point.Radius, NavMesh.AllAreas))
            return hit2.position;
        //if THAT fails, just use raw patrol point position
        return position;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var point in PatrolPoints)
        {
            if (point)
            {
                Gizmos.DrawLine(transform.position, point.transform.position);
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(point.transform.position, Vector3.up, point.Radius, 3f);
            }
        }
    }

    private void OnValidate()
    {
        PatrolPoints.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).TryGetComponent<EnemyPatrolPoint>(out var point))
                PatrolPoints.Add(point);
        }
    }
}
