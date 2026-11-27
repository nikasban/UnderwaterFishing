using UnityEngine;

public class FloatingSquareSpawner : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameObject squarePrefab;

    [Header("Spawn Area")]
    public float minX = -10f;
    public float maxX = 10f;
    public float spawnY = -10f;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.4f;

    [Header("Movement Speed")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.2f;

    [Header("Size")]
    public float minSize = 0.12f;
    public float maxSize = 0.25f;
    [Range(0.05f, 1f)]
    public float shrinkMultiplier = 0.5f;

    [Header("Appearance")]
    [Range(0f, 1f)]
    public float alpha = 0.25f;

    public float surfaceOffset = 0.1f;

    private float spawnTimer;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        if (player == null ||
            !player.IsGameplayEnabled ||
            player.IsOnPier ||
            squarePrefab == null)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnSquare();
        }
    }

    void SpawnSquare()
    {
        float surfaceY =
            player.waterSurfaceY - surfaceOffset;

        if (spawnY >= surfaceY)
            return;

        float x = Random.Range(
            Mathf.Min(minX, maxX),
            Mathf.Max(minX, maxX)
        );

        Vector3 spawnPosition = new Vector3(
            x,
            spawnY,
            0f
        );

        GameObject newSquare = Instantiate(
            squarePrefab,
            spawnPosition,
            Quaternion.identity
        );

        FloatingSquare square =
            newSquare.GetComponent<FloatingSquare>();

        if (square == null)
        {
            Debug.LogError(
                "На FloatingSquare отсутствует скрипт FloatingSquare!"
            );

            Destroy(newSquare);
            return;
        }

        float size = Random.Range(minSize, maxSize);
        float speed = Random.Range(minSpeed, maxSpeed);

        square.Initialize(
            surfaceY,
            size,
            size * shrinkMultiplier,
            speed,
            alpha
        );
    }
}