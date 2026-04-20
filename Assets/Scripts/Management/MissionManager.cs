using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Spawning")]
    public List<GameObject> enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("References")]
    public Health mechHealth;
    public Health playerHealth;
    public DistrictManager district;

    List<GameObject> aliveEnemies = new List<GameObject>();

    int wave = 0;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            wave++;

            SpawnWave();

            //wait until all enemies are dead
            yield return new WaitUntil(() => aliveEnemies.Count == 0);
            yield return new WaitForSeconds(5f);
        }
    }
    
    void SpawnWave()
    {
        int count = 3 + wave;

        for (int i = 0; i < count; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject enemy = Instantiate(enemyPrefabs[0], spawn.position, spawn.rotation);

            aliveEnemies.Add(enemy);

            enemy.GetComponent<Health>().OnDeath += () =>
            {
                aliveEnemies.Remove(enemy);
            };
        }
    }
}
