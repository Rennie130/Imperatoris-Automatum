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

    List<GameObject> aliveEnemies = new List<GameObject>(); //should this be made public or private?

    int wave = 0; //is this public or private?

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        StartCoroutine(WaveLoop()); // how to stop once enough waves have passed?
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

            enemy.GetComponent<Health>().OnDeath += () => // unsure what += () => means
            {
                aliveEnemies.Remove(enemy);
            }; 
        }
    }

    public void EndOfMission()
    {
        // check to see if all waves are completed and the current wave aliveEnemies.Count == 0.
        // If above correct, can either trigger game win screen/ui, or set a bool AllEnemiesDead to true which can be used in function to call UI and update player score?
        // if (aliveEnemies.Count == 0 /* && end of wave*/)
        // {
        //     GameManager.Instance.LevelCompleted();
        // }

    }
}
