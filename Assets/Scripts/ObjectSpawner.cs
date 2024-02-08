using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; //faster than System.Random, statically initialized 

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] ParticleSystem _collectionParticles;
    [SerializeField] private GameObject[] _objectPrefabs;
    [SerializeField] private int _poolSize = 10; // Size of the pool for each object type
    [SerializeField] private float _minSpawnInterval = 1f;
    [SerializeField] private float _maxSpawnInterval = 3f;
    private float _currentSpawnInterval = 1f;
    
    
    private float _timer;
    private Dictionary<CollectablesEnum, Queue<GameObject>> _objectPools;
    private Vector2 _cameraBounds;
    private bool _gameActive = true;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        _currentSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        Camera cam = Camera.main;
        Vector3 bounds = cam.ScreenToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        _cameraBounds = new Vector2(Mathf.Abs(bounds.x), Mathf.Abs(bounds.y));
        _timer = 0f;
        InitializeObjectPools();
    }

    private void OnGameStateChanged(bool active)
    {
        _gameActive = active;
        if(!_gameActive)
        {
            DisableAllCollectables();
        }
    }

    void Update()
    {
        if (_gameActive)
        {
            _timer += Time.deltaTime;

            if (_timer >= _currentSpawnInterval)
            {
                SpawnObject();
                _timer = 0f;
            }
        }
    }

    void InitializeObjectPools()
    {
        _objectPools = new Dictionary<CollectablesEnum, Queue<GameObject>>();
        foreach (var prefab in _objectPrefabs)
        {
            if (prefab.TryGetComponent(out Collectable collectable))
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                _objectPools.Add(collectable.CollectableID, objectPool);

                for (int i = 0; i < _poolSize; i++)
                {
                    SpawnObjectInternal(prefab);
                }
            }
        }
    }

    private void SpawnObjectInternal(GameObject prefab)
    {
        GameObject newObj = Instantiate(prefab, transform);
        if(newObj.TryGetComponent(out Collectable collectable))
        {
            collectable.OnDisabled += OnCollectableDisabled;
            newObj.SetActive(false);
            _objectPools[collectable.CollectableID].Enqueue(newObj);
        }
        else
        {
            Debug.LogError("Object does not have a Collectable component");
        }
    }

    private void DisableAllCollectables()
    {
        foreach (var collectable in GetComponentsInChildren<Collectable>())
        {
            if (collectable.gameObject.activeSelf)
            {
                collectable.Disable(false);
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        foreach (var collectable in GetComponentsInChildren<Collectable>())
        {
            collectable.OnDisabled -= OnCollectableDisabled;
        }
    }

    private void OnCollectableDisabled(Collectable collectable, bool collected)
    {
        if (collected)
        {
            GameManager.Instance.AddToScore(collectable.PointValue);
            _collectionParticles.Play();
        }
        _objectPools[collectable.CollectableID].Enqueue(collectable.gameObject);
    }

    void SpawnObject()
    {
        int randomIndex = Random.Range(0, _objectPrefabs.Length);
        GameObject prefabToSpawn = _objectPrefabs[randomIndex];
        if (prefabToSpawn.TryGetComponent(out Collectable collectable))
        {
            if (_objectPools[collectable.CollectableID].Count == 0)
            {
                ExpandPool(prefabToSpawn);
            }

            GameObject spawnedObject = _objectPools[collectable.CollectableID].Dequeue();
            spawnedObject.SetActive(true);

            // Set a random position for the spawned object
            float randomX = Random.Range(-_cameraBounds.x + 2, _cameraBounds.x - 2);
            spawnedObject.transform.position = new Vector3(randomX, _cameraBounds.y + 3, 0);
            _currentSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }
    }

    void ExpandPool(GameObject prefab)
    {
        SpawnObjectInternal(prefab);
    }
}