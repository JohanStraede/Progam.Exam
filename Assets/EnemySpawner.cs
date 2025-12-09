using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private LayerMask whatIsGround;
    private float spawnTimer;

    private void Start()
    {
        Debug.Log("EnemySpawner started");
        
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned!");
            return;
        }

        spawnTimer = spawnInterval;
        Debug.Log("Spawner initialized. Will spawn every " + spawnInterval + " seconds");
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 randomPosition = new Vector3(
            transform.position.x + Random.Range(-spawnRadius, spawnRadius),
            transform.position.y + 100f,
            transform.position.z + Random.Range(-spawnRadius, spawnRadius)
        );

        if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, 200f, whatIsGround))
        {
            Instantiate(enemyPrefab, hit.point + Vector3.up * 1f, Quaternion.identity);
            Debug.Log("Enemy spawned at " + hit.point);
        }
        else
        {
            Debug.LogWarning("Raycast did not hit ground layer at " + randomPosition);
        }
    }
}
