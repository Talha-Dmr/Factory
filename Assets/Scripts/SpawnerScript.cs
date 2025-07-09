using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;    
    public float spawnInterval = 2f;    
    public Transform spawnPoint;        

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Prefab to spawn is not assigned.");
            return;
        }
        Vector3 position = spawnPoint ? spawnPoint.position : transform.position;
        Instantiate(prefabToSpawn, position, Quaternion.identity);
    }
}
