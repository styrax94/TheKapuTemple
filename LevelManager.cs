using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Serializable]
    public class EnemySummonPrefabs
    {
        public GameObject enemyPrefab;
        public GameObject summonPrefab;
    }

    [Header("Enemy Spawning")]
    public List<EnemySummonPrefabs> enemyPrefabs;
    public float enemyCountToSpawn;
    public float enemyCount;
    public float enemyCountActiveCap;
    float enemyCountActive;
    public int enemyRarity;  
    public float enemySpawnDelay;
    public float spawnCooldown;
    public BoxCollider [] spawnArea;
    public static UnityEvent EnemyWasDefeatedEvent = new UnityEvent();

    [Header("Scripts")]
    public LevelStartTrigger levelTrigger;

    [Header("Player Spawn")]
    public Transform playerSpawn;
    GameObject player;
    TextMeshProUGUI minionCount;
    // Start is called before the first frame update
    void Start()
    {      
        EnemyWasDefeatedEvent.AddListener(EnemyDefeated);
        
        enemyCountActive = 0;
    }
    public void EnemyDefeated()
    {    
        enemyCountActive--;
        enemyCount--;
        minionCount.SetText(enemyCount.ToString());
        if (enemyCount ==0)
            LevelComplete();

    }
   
    [ContextMenu("BeginSpawn")]
    public void BeginSpawn()
    {
        //Spawns the particle system that indicates an enemy spawn
        StartCoroutine(SpawnSummoner());
        Manager.Instance.minionCounterUI.SetActive(true);
        minionCount.SetText(enemyCount.ToString());
    }

    IEnumerator SpawnSummoner()
    {
        while(enemyCountToSpawn > 0  )
        {
            yield return null;

            if(enemyCountActive < enemyCountActiveCap)
            {
                //for loop to wait for the spawn cooldown to finish before proceeding
                for (float i = 0; i < spawnCooldown; i += Time.deltaTime)
                {
                    yield return null;
                }
                int ran = UnityEngine.Random.Range(0,enemyPrefabs.Count);
                //Get Random Enemy GameObject from Pool
                GameObject spawn = ObjectPool.Spawn(enemyPrefabs[ran].summonPrefab);
                Vector3 pos = GetSpawnPosition();
                spawn.transform.position = pos;
                spawn.GetComponent<Summoning>().rarity = enemyRarity;
                spawn.SetActive(true);
                //Coroutine to Spawn the actualy enemy
                StartCoroutine(SpawnEnemy(pos, ran));
                EnemyCount();
            }
        }
        
    }
    IEnumerator SpawnEnemy(Vector3 pos, int type)
    {
        for (float i = 0; i < enemySpawnDelay; i += Time.deltaTime)
        {
            yield return null;
        }
        GameObject enemy = ObjectPool.Spawn(enemyPrefabs[type].enemyPrefab);
        enemy.transform.position = pos;
        enemy.GetComponent<EnemyRarity>().GiveAttributesAccordingToRarity(enemyRarity);      
        enemy.SetActive(true);
        enemy.GetComponent<EnemyBehaviour>().SetStartingState(player);
    }
    public Vector3 GetSpawnPosition()
    {
        int ran = UnityEngine.Random.Range(0, spawnArea.Length);

        Vector3 position;      
        position.x = UnityEngine.Random.Range(spawnArea[ran].bounds.min.x, spawnArea[ran].bounds.max.x);
        position.z = UnityEngine.Random.Range(spawnArea[ran].bounds.min.z, spawnArea[ran].bounds.max.z);
        position.y = 1f;      
        return position;
    }

    public void EnemyCount()
    {
        enemyCountToSpawn--;
        enemyCountActive++;    
    }

    public void LevelComplete()
    {
        Manager.Instance.minionCounterUI.SetActive(false);
        levelTrigger.LevelCompleted();
    }

    public void LevelLoad(GameObject p, int rarity, TextMeshProUGUI text)
    {
        player = p;
        player.transform.position = playerSpawn.position;
        player.transform.rotation = playerSpawn.transform.rotation;
        player.SetActive(true);
        enemyRarity = rarity;
        minionCount = text;
    }
}
