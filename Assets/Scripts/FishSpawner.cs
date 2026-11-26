using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishSpawnZone
{
    public string zoneName = "Поверхность";
    public float minY = -5f;
    public float maxY = 3f;
    [Min(0f)] public float commonChance = 70f;
    [Min(0f)] public float rareChance = 22f;
    [Min(0f)] public float veryRareChance = 5f;
    [Min(0f)] public float evilChance = 3f;
}

public class FishSpawner : MonoBehaviour
{
    public Transform player;
    public float minX = -30f;
    public float maxX = 30f;
    public float spawnInterval = 2f;
    public int maxFish = 20;
    public float minSpawnDistance = 8f;
    public List<FishSpawnZone> zones = new List<FishSpawnZone>
    {
        new FishSpawnZone
        {
            zoneName = "Поверхность",
            minY = -5f,
            maxY = 3f,
            commonChance = 70f,
            rareChance = 22f,
            veryRareChance = 5f,
            evilChance = 3f
        },
        new FishSpawnZone
        {
            zoneName = "Середина",
            minY = -25f,
            maxY = -5f,
            commonChance = 45f,
            rareChance = 30f,
            veryRareChance = 18f,
            evilChance = 7f
        },
        new FishSpawnZone
        {
            zoneName = "Дно",
            minY = -50f,
            maxY = -25f,
            commonChance = 15f,
            rareChance = 30f,
            veryRareChance = 35f,
            evilChance = 20f
        }
    };
    public List<GameObject> commonFish = new List<GameObject>();
    public List<GameObject> rareFish = new List<GameObject>();
    public List<GameObject> veryRareFish = new List<GameObject>();
    public List<GameObject> evilFish = new List<GameObject>();

    private float timer;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (FindObjectsOfType<Fish>().Length >= maxFish)
            return;

        if (!TryPickSpawnPosition(out Vector3 position, out FishSpawnZone zone))
            return;

        GameObject prefab = PickWeightedPrefab(zone);
        if (prefab != null)
            Instantiate(prefab, position, Quaternion.identity);
    }

    bool TryPickSpawnPosition(out Vector3 position, out FishSpawnZone selectedZone)
    {
        position = Vector3.zero;
        selectedZone = null;

        if (zones == null || zones.Count == 0)
            return false;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            FishSpawnZone zone = PickZoneByHeight();
            if (zone == null) return false;

            float lowY = Mathf.Min(zone.minY, zone.maxY);
            float highY = Mathf.Max(zone.minY, zone.maxY);

            Vector3 candidate = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(lowY, highY),
                0f
            );

            if (Vector2.Distance(candidate, player.position) >= minSpawnDistance)
            {
                position = candidate;
                selectedZone = zone;
                return true;
            }
        }

        return false;
    }

    FishSpawnZone PickZoneByHeight()
    {
        float totalHeight = 0f;
        foreach (FishSpawnZone zone in zones)
        {
            if (zone == null) continue;
            totalHeight += Mathf.Abs(zone.maxY - zone.minY);
        }

        if (totalHeight <= 0f) return null;

        float roll = Random.Range(0f, totalHeight);
        foreach (FishSpawnZone zone in zones)
        {
            if (zone == null) continue;

            float height = Mathf.Abs(zone.maxY - zone.minY);
            if (height <= 0f) continue;

            if (roll < height)
                return zone;

            roll -= height;
        }

        for (int i = zones.Count - 1; i >= 0; i--)
        {
            if (zones[i] != null && Mathf.Abs(zones[i].maxY - zones[i].minY) > 0f)
                return zones[i];
        }

        return null;
    }

    GameObject PickWeightedPrefab(FishSpawnZone zone)
    {
        if (zone == null) return null;

        float common = HasFish(commonFish) ? Mathf.Max(0f, zone.commonChance) : 0f;
        float rare = HasFish(rareFish) ? Mathf.Max(0f, zone.rareChance) : 0f;
        float veryRare = HasFish(veryRareFish) ? Mathf.Max(0f, zone.veryRareChance) : 0f;
        float evil = HasFish(evilFish) ? Mathf.Max(0f, zone.evilChance) : 0f;

        float total = common + rare + veryRare + evil;
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total);

        if (roll < common) return GetRandomFish(commonFish);
        roll -= common;

        if (roll < rare) return GetRandomFish(rareFish);
        roll -= rare;

        if (roll < veryRare) return GetRandomFish(veryRareFish);
        roll -= veryRare;

        if (roll < evil) return GetRandomFish(evilFish);

        return null;
    }

    bool HasFish(List<GameObject> list)
    {
        if (list == null) return false;
        foreach (GameObject fish in list)
        {
            if (fish != null)
                return true;
        }
        return false;
    }

    GameObject GetRandomFish(List<GameObject> list)
    {
        if (list == null) return null;

        int validCount = 0;
        foreach (GameObject fish in list)
        {
            if (fish != null)
                validCount++;
        }

        if (validCount == 0) return null;

        int chosenIndex = Random.Range(0, validCount);
        foreach (GameObject fish in list)
        {
            if (fish == null) continue;
            if (chosenIndex == 0) return fish;
            chosenIndex--;
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (FishSpawnZone zone in zones)
        {
            if (zone == null) continue;

            float lowY = Mathf.Min(zone.minY, zone.maxY);
            float highY = Mathf.Max(zone.minY, zone.maxY);

            Gizmos.DrawLine(new Vector3(minX, lowY, 0f), new Vector3(maxX, lowY, 0f));
            Gizmos.DrawLine(new Vector3(minX, highY, 0f), new Vector3(maxX, highY, 0f));
        }

        Gizmos.DrawWireCube(
            new Vector3((minX + maxX) * 0.5f, GetZonesCenterY(), 0f),
            new Vector3(maxX - minX, GetZonesHeight(), 0.1f)
        );
    }

    float GetZonesCenterY()
    {
        if (zones == null || zones.Count == 0) return 0f;
        float min = float.MaxValue;
        float max = float.MinValue;
        foreach (FishSpawnZone zone in zones)
        {
            if (zone == null) continue;
            min = Mathf.Min(min, zone.minY, zone.maxY);
            max = Mathf.Max(max, zone.minY, zone.maxY);
        }
        return min == float.MaxValue ? 0f : (min + max) * 0.5f;
    }

    float GetZonesHeight()
    {
        if (zones == null || zones.Count == 0) return 0f;
        float min = float.MaxValue;
        float max = float.MinValue;
        foreach (FishSpawnZone zone in zones)
        {
            if (zone == null) continue;
            min = Mathf.Min(min, zone.minY, zone.maxY);
            max = Mathf.Max(max, zone.minY, zone.maxY);
        }
        return min == float.MaxValue ? 0f : max - min;
    }
}
