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
    
    
    [SerializeField] private float _bonusSpawnForce = 15f;
    
    [SerializeField] private float _minBonusSpawnAngle = 25f;
    [SerializeField] private float _maxBonusSpawnAngle = 60f;
    
    private float _currentSpawnInterval = 1f;
    
    
    private float _timer;
    private Dictionary<CollectablesEnum, Queue<GameObject>> _objectPools; //simple object pooling for collectables
    private Vector2 _cameraBounds;
    private bool _gameActive = true;

    void Start()
    {
        Debug.Log(AngleToVector(0f));
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        _currentSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        Camera cam = Camera.main;
        Vector3 bounds = cam.ScreenToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        _cameraBounds = new Vector2(Mathf.Abs(bounds.x), Mathf.Abs(bounds.y));
        _timer = 0f;
        InitializeObjectPools();
    }

    //called when ending or restarting the game
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
                    ExpandPoolInternal(prefab);
                }
            }
        }
    }

    /// <summary>
    /// Actually instantiates new instances of collectables and adds them to the associated pool by enum ID
    /// </summary>
    /// <param name="prefab"></param>
    private void ExpandPoolInternal(GameObject prefab)
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

    /// <summary>
    /// Used to disable all collectables when the game ends
    /// </summary>
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

    //cleanup and unsubscribe from events
    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        foreach (var collectable in GetComponentsInChildren<Collectable>())
        {
            collectable.OnDisabled -= OnCollectableDisabled;
        }
    }

    /// <summary>
    /// Invoked by a collectable when it is disabled, either by being collected or missed.
    /// Handles score, particle effect and recycling to pool
    /// </summary>
    /// <param name="collectable"></param>
    /// <param name="collected"></param>
    private void OnCollectableDisabled(Collectable collectable, bool collected)
    {
        if (collected)
        {
            GameManager.Instance.AddToScore(collectable.PointValue);
            _collectionParticles.Play();
            
            //decided to add this feature for stars only, could make this a timed powerup or base it off of overloaded behavior
            //but don't want to overcomplicate for the prompt/time
            if(collectable.CollectableID == CollectablesEnum.StarPurple || collectable.CollectableID == CollectablesEnum.StarGold)
            {
                BonusSpawn(collectable.gameObject);
            }
        }
        _objectPools[collectable.CollectableID].Enqueue(collectable.gameObject);
    }

    /// <summary>
    /// Spawns an object using the objectpools, expands if needed
    /// </summary>
    void SpawnObject()
    {
        /*I do this with prefab list instead of using enum values for random selection
        this avoids a dependency between the enum and the prefab list having to be in parity and all enum values used */
        int randomIndex = Random.Range(0, _objectPrefabs.Length);
        GameObject prefabToSpawn = _objectPrefabs[randomIndex];
        if (prefabToSpawn.TryGetComponent(out Collectable collectable)) 
        {
            if (_objectPools[collectable.CollectableID].Count == 0)
            {
                ExpandPoolInternal(prefabToSpawn);
            }

            GameObject spawnedObject = _objectPools[collectable.CollectableID].Dequeue();
            spawnedObject.SetActive(true);

            // Set a random position for the spawned object
            float randomX = Random.Range(-_cameraBounds.x + 2, _cameraBounds.x - 2);
            spawnedObject.transform.position = new Vector3(randomX, _cameraBounds.y + 3, 0);
            _currentSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }
    }
    
    /// <summary>
    /// Late addition feature to spawn bonus objects when collecting an object (currently only used for stars)
    /// </summary>
    /// <param name="prefabToSpawn"></param>
    private void BonusSpawn(GameObject prefabToSpawn)
    {
        int _spawnCount = Random.Range(1, 3); // 1 or 2
        
        int randomSign = Random.Range(0, 2) * 2 - 1; //randomly returns 1 or -1
        float spawnAngle = Random.Range(_minBonusSpawnAngle, _maxBonusSpawnAngle) * randomSign;
        
        if (prefabToSpawn.TryGetComponent(out Collectable collectable)) 
        {
            Vector3 xOffset = Vector3.right * collectable.RadiusBuffer * randomSign;
            
            // a bit of a magic number to put us above the collection collider, this was a calculated hack in the interest of time to add a minor feature
            Vector3 yOffset = Vector3.up * (1.35f + collectable.RadiusBuffer); 
            for(int i = 0; i < _spawnCount; i++)
            {
                int multiplier = i == 0 ? 1 : -1; //simplistic, but we choose either the existing angle or it's negative
                Vector2 spawnDirection = AngleToVector(spawnAngle * multiplier);
                
            
                if (_objectPools[collectable.CollectableID].Count == 0)
                {
                    ExpandPoolInternal(prefabToSpawn);
                }

                GameObject spawnedObject = _objectPools[collectable.CollectableID].Dequeue();
                //i already had the particle system from the player and this is much simpler than creating a new way to grab player's position
                spawnedObject.transform.position = _collectionParticles.transform.parent.position + yOffset + xOffset * multiplier;
                spawnedObject.SetActive(true);
                spawnedObject.GetComponent<Rigidbody2D>().AddForce(spawnDirection * _bonusSpawnForce, ForceMode2D.Impulse);
            }
            
            
            
        }
    }
    
    /// <summary>
    /// Helper function to get a vector from an angle offset from Vector2.Up
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public Vector2 AngleToVector(float angle)
    {
        // Convert angle from degrees to radians
        float radian = angle * Mathf.Deg2Rad;

        // Calculate the x and y components
        float x = Mathf.Sin(radian);
        float y = Mathf.Cos(radian);

        // Create and return the vector
        return new Vector2(x, y).normalized;
    }
}