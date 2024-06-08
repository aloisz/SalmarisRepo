using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviour
{
    private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();
    [SerializeField] private List<KeyPool> keyPools = new List<KeyPool>();
    
    

    public static Pooling instance;
    private void Awake()
    {
        instance = this;

        InitiPools();
        PopulatePools();
    }

    private void Start()
    {
        InitRefresh();
    }

    private void InitiPools()
    {
        foreach (var keyPool in keyPools)
        {
            pools.Add(keyPool.key, keyPool.pool);
        }
    }

    private void PopulatePools()
    {
        foreach (var pool in pools)
        {
            PopulatePool(pool.Value);
        }
    }

    private int i;
    private void PopulatePool(Pool pool)
    {
        for (i = 0; i < pool.baseCount; i++)
        {
            AddInstance(pool);
        }
    }

    private GameObject obj;
    private void AddInstance(Pool pool)
    {
        obj = Instantiate(pool.prefab, transform);
        obj.SetActive(false);
        
        pool.queue.Enqueue(obj);
    }

    public GameObject Pop(string key)
    {
        if (pools[key].queue.Count == 0)
        {
            Debug.LogWarning("Pool of " + key + " is empty");
            AddInstance(pools[key]);
        }
        obj = pools[key].queue.Dequeue();
        obj.SetActive(true);
        
        return obj;
    }

    public void DePop(string key, GameObject go)
    {
        pools[key].queue.Enqueue(go);
        go.transform.parent = transform;
        go.SetActive(false);
    }

    public void DelayedDePop(string key, GameObject go, float time)
    {
        StartCoroutine(DelayedCoroutineDePop(key, go, time));
    }

    IEnumerator DelayedCoroutineDePop(string key, GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        DePop(key, go);
    }

    private void InitRefresh()
    {
        foreach (var keyPool in keyPools)
        {
            StartCoroutine(RefreshPoolCoroutine(keyPool.pool, keyPool.pool.refreshSpeed));
        }
    }

    IEnumerator RefreshPoolCoroutine(Pool pool, float time)
    {
        yield return new WaitForSeconds(time);
        if (pool.queue.Count < pool.baseCount)
        {
            AddInstance(pool);
            pool.refreshSpeed = pool.baseRefreshSpeed * pool.queue.Count / pool.baseCount;
        }
        StartCoroutine(RefreshPoolCoroutine(pool, pool.refreshSpeed));
    }
    

    [Serializable]
    public class Pool
    {
        public GameObject prefab;
        public Queue<GameObject> queue = new Queue<GameObject>();
        public int baseCount;
        public float baseRefreshSpeed = 5;
        public float refreshSpeed;
    }
    
    [Serializable]
    public class KeyPool
    {
        public string key;
        public Pool pool;
    }
}


