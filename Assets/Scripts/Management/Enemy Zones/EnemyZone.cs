using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyZone : MonoBehaviour
{
    public List<EnemyPatrolPoint> PatrolPoints = new();
    public List<EnemyWave> Waves = new();
    public EnemyZone NextZone;
    public bool SpawnOnStart;

    public int CurrentWave;
    public List<Enemy> CurrentRemainingEnemies = new();

    private void Awake() 
    {
        if(SpawnOnStart)
            StartWaves();    
    }

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

        if(NextZone != null)
        {
            Handles.color = Color.green;
            Handles.DrawLine(transform.position, NextZone.transform.position, 4f);
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

    //Tells the zone to start spawning waves
    public void StartWaves()
    {
        CurrentWave = 0;
        SpawnWave(Waves[CurrentWave]);
    }

    //Spawns the given wave
    public void SpawnWave(EnemyWave wave)
    {
        foreach(EnemySpawnSet spawns in wave.Spawns)
        {
            for(int i = 0; i < spawns.Count; i++)
            {
                var spawn = Instantiate(spawns.EnemyPrefab, GetRandomPatrolPosition(), Quaternion.identity);
                spawn.AssociatedEnemyZone = this;
                spawn.OnDeath += OnEnemyDeath;
                CurrentRemainingEnemies.Add(spawn);
            }
        }
    }

    //Invoked on enemy death, if all enemies are dead we end the wave
    private void OnEnemyDeath(Enemy dead)
    {
        dead.OnDeath -= OnEnemyDeath;
        CurrentRemainingEnemies.Remove(dead);
        if(CurrentRemainingEnemies.Count == 0)
        {
            OnWaveEnded();
        }
    }

    //Handle wave and zone switching and win condition
    private void OnWaveEnded()
    {
        CurrentWave++;
        if(CurrentWave < Waves.Count) // next wave
        {
            SpawnWave(Waves[CurrentWave]);
        }
        else if(NextZone != null) //no more waves? next zone
        {
            NextZone.StartWaves();
        }
        else //no more zones? you win!
        {
            //tell game manager that the game is over
        }
    }

}
