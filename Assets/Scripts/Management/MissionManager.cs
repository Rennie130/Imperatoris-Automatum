using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Spawning")]
    public List<GameObject> enemyPrefabs;
    public Transform[] spawnPoints;
    private GameObject basicEnemy;
    public int maxWaves = 2; //Actually want this to update depending on level requirements.

    [Header("References")]
    public Health mechHealth;
    public Health playerHealth;
    public DistrictManager district;

    public List<GameObject> aliveEnemies = new List<GameObject>(); //should this be made public or private?

    private int waveCount = 0; //is this public or private?
    
    private bool canSpawn = true;
    

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        StartCoroutine(SpawnWave()); // how to stop once enough waves have passed?
    }

    
    IEnumerator SpawnWave()
    {
        while(waveCount < maxWaves) //are all waves completed?
        {
            Debug.Log("Starting wave:" + waveCount);
            //spawn a wave  
            
            for(int i = 0; i < 2; i++)
            {
                SpawnEnemy();
            }

            yield return new WaitUntil(() => aliveEnemies.Count == 0); //is the current wave alive?

            yield return new WaitForSeconds(5f); //wait 5 seconds before the next wave
            waveCount++;
        }

         GameManager.Instance.LevelCompleted();
    }


    private void SpawnEnemy()
    {
        //maybe have to check if the spawn point is already occupied?

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        basicEnemy = Instantiate(enemyPrefabs[0], spawn.position, spawn.rotation);

        aliveEnemies.Add(basicEnemy);

    }

}
