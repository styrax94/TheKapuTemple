using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Serializable]
    public class PrefabsToPool
    {
        public GameObject prefab;
        public int poolAmount;
    }

    public  List<PrefabsToPool> prefabsToPool;
    public static Dictionary<int, Transform> InactivePools = new Dictionary<int, Transform>();
    public static Dictionary<GameObject, Transform> PrefabPoolRelationInactive = new Dictionary<GameObject, Transform>();
    public static Dictionary<GameObject, Transform> PrefabPoolRelationActive = new Dictionary<GameObject, Transform>();
    

    public static Transform InactiveParent;
    public static Transform ActiveParent;

    public void Start()
    {      
        InstantiatePools();
    }
   
    public  void InstantiatePools()
    {
        if (!InactiveParent)
        {
            InactiveParent = new GameObject("InactiveObjects").transform;
            InactiveParent.SetParent(transform);
        }
        if (!ActiveParent)
        {
            ActiveParent = new GameObject("ActiveObjects").transform;
            ActiveParent.SetParent(transform);
        }

        for (int i = 0; i < prefabsToPool.Count; i++)
        {
            for (int b = 0; b < prefabsToPool[i].poolAmount; b++)
            {
                //Tries to get the pool transfrom from prefab
                if (PrefabPoolRelationInactive.TryGetValue(prefabsToPool[i].prefab, out Transform pool))
                {
                    InstantiateObjToInactiveParent(prefabsToPool[i].prefab, pool);
                }
                else
                {
                    //create new parent transform

                    Transform enemyPool = CreateNewParentTransform(InactiveParent, prefabsToPool[i].prefab, PrefabPoolRelationInactive);
                    InstantiateObjToInactiveParent(prefabsToPool[i].prefab, enemyPool.transform);
               }
            }
        }

    }

    public static GameObject Spawn(GameObject objectToSpawn)
    {

        if (PrefabPoolRelationInactive.TryGetValue(objectToSpawn, out Transform objPool))
        {
            if (objPool.childCount > 0)
            {
              return SpawnToActivePool(objPool, objectToSpawn);
            }
            else
            {
                InstantiateObjToInactiveParent(objectToSpawn, objPool);
                return SpawnToActivePool(objPool, objectToSpawn);
            }
        }
        else
        {
            Transform enemyPool = CreateNewParentTransform(InactiveParent, objectToSpawn, PrefabPoolRelationInactive);
            InstantiateObjToInactiveParent(objectToSpawn, enemyPool.transform);
            return SpawnToActivePool(enemyPool, objectToSpawn);
        }
    }

    public static void Despawn(GameObject obj)
    {
        if (InactivePools.TryGetValue(obj.GetInstanceID(), out Transform pool))
        {
            obj.transform.SetParent(pool);
            obj.SetActive(false);
        }
        else
        {
            Debug.Log("destroyed");
            Destroy(obj);
        }
    }

    public static void InstantiateObjToInactiveParent(GameObject prefab, Transform pool)
    {
        var obj = GameObject.Instantiate(prefab, pool);
        InactivePools.Add(obj.GetInstanceID(), pool);
        obj.SetActive(false);
    }

    public static Transform CreateNewParentTransform(Transform rootParent, GameObject prefab, Dictionary<GameObject, Transform> poolRelation)
    {
        Transform enemyPool = new GameObject("Pool: " + prefab.name).transform;
        enemyPool.SetParent(rootParent);
        poolRelation.Add(prefab, enemyPool.transform);
        return enemyPool;
    }

    public static GameObject SpawnToActivePool(Transform objPool, GameObject objectToSpawn)
    {
        GameObject obj = objPool.GetChild(0).gameObject;

        if (PrefabPoolRelationActive.TryGetValue(objectToSpawn, out Transform activePool))
        {
            obj.transform.SetParent(activePool);
        }
        else
        {
            Transform poolParent = CreateNewParentTransform(ActiveParent, objectToSpawn, PrefabPoolRelationActive);          
            obj.transform.SetParent(poolParent);          
        }

        return obj;
    }
}
