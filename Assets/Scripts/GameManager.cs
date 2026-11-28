using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum UpgradeType
{
    HarpoonRange,
    ViewRange,
    OxygenCapacity,
    BagCapacity,
    Armor
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int coins = 0;
    [Min(0f)]
    public float maxWeight = 10f;
    [Min(1)]
    public int maxUpgradeLevel = 5;
    public float upgradeCostMultiplier = 1.6f;

    [Tooltip("Порядок: Гарпун, Обзор, Кислород, Мешок, Броня")]
    public int[] upgradeBaseCosts = { 100, 120, 100, 100, 130 };

    [SerializeField]
    private int[] upgradeLevels = new int[5];
    [Min(0f)]
    public float harpoonRangePerLevel = 2f;

    [Min(0f)]
    public float viewSizePerLevel = 0.8f;

    [Min(0f)]
    public float oxygenPerLevel = 12f;

    [Min(0f)]
    public float bagCapacityPerLevel = 2.5f;

    [Range(0f, 0.5f)]
    public float armorReductionPerLevel = 0.1f;

    [Range(0f, 0.8f)]
    public float maxArmorReduction = 0.5f;
    public List<FishType> inventory = new List<FishType>();
    [SerializeField]
    private List<float> inventoryWeights = new List<float>();
    private List<int> inventorySellPrices = new List<int>();

    public int InventoryVersion { get; private set; }
    [Header("Save System")]
    [SerializeField] private List<FishType> fishCatalog = new List<FishType>();

    private const string SaveFileName = "game_save.json";

    private string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);
    private const int MaxSavedCoins = 1000000000;
    private const int MaxSavedFish = 500;
    private const int MaxSavedFishPrice = 1000000000;

    [Serializable]
    private class SaveData
    {
        public int coins;
        public int[] upgradeLevels;

        public List<string> fishNames = new List<string>();
        public List<float> inventoryWeights = new List<float>();
        public List<int> inventorySellPrices = new List<int>();
    }
    public float CurrentMaxWeight =>
        Mathf.Max(0f, maxWeight +
        GetUpgradeLevel(UpgradeType.BagCapacity) * bagCapacityPerLevel);

    public float TotalWeight
    {
        get
        {
            EnsureInventoryData();

            float weight = 0f;

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null)
                    weight += inventoryWeights[i];
            }

            return weight;
        }
    }

    public float ExcessWeight =>
        Mathf.Max(0f, TotalWeight - CurrentMaxWeight);

    public bool IsOverweight => ExcessWeight > 0.001f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureUpgradeArray();
        EnsureInventoryData();

        LoadGame();

        EnsureInventoryData();
    }

    void EnsureUpgradeArray()
    {
        if (upgradeLevels == null || upgradeLevels.Length != 5)
        {
            int[] oldLevels = upgradeLevels;
            upgradeLevels = new int[5];

            if (oldLevels != null)
            {
                for (int i = 0; i < Mathf.Min(oldLevels.Length, 5); i++)
                    upgradeLevels[i] = oldLevels[i];
            }
        }
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        int index = (int)type;

        if (index < 0 || index >= upgradeLevels.Length)
            return 0;

        return Mathf.Clamp(
            upgradeLevels[index],
            0,
            maxUpgradeLevel
        );
    }

    public int GetUpgradeCost(UpgradeType type)
    {
        int index = (int)type;
        int level = GetUpgradeLevel(type);

        if (level >= maxUpgradeLevel)
            return 0;

        int baseCost = 100;

        if (upgradeBaseCosts != null &&
            index >= 0 &&
            index < upgradeBaseCosts.Length)
        {
            baseCost = upgradeBaseCosts[index];
        }

        float multiplier = Mathf.Max(1f, upgradeCostMultiplier);

        return Mathf.RoundToInt(
            baseCost * Mathf.Pow(multiplier, level)
        );
    }

    public bool TryPurchaseUpgrade(UpgradeType type)
    {
        int index = (int)type;

        if (index < 0 || index >= upgradeLevels.Length)
            return false;

        if (GetUpgradeLevel(type) >= maxUpgradeLevel)
            return false;

        int cost = GetUpgradeCost(type);

        if (coins < cost)
            return false;

        coins -= cost;
        upgradeLevels[index]++;

        Player player = FindObjectOfType<Player>();

        if (player != null)
            player.ApplyUpgradeStats();

        SaveGame();

        return true;
    }

    public bool CanAddFish(FishType type)
    {
        return type != null;
    }

    public bool AddFish(FishType type)
    {
        if (type == null)
            return false;

        float averageWeight = Mathf.Max(0.01f, type.weight);

        float randomWeight = UnityEngine.Random.Range(
            averageWeight * 0.75f,
            averageWeight * 1.25f
        );

        return AddFish(type, randomWeight);
    }

    public bool AddFish(FishType type, float actualWeight)
    {
        if (type == null)
            return false;

        EnsureInventoryData();

        float safeWeight = Mathf.Max(0f, actualWeight);
        int rarity = Mathf.Clamp(type.rarity, 1, 7);
        float averageWeight = Mathf.Max(0.01f, type.weight);

        float price =
            50f *
            (safeWeight / averageWeight) *
            Mathf.Pow(2f, rarity) /
            (1.5f * rarity);

        inventory.Add(type);
        inventoryWeights.Add(safeWeight);
        int finalPrice = Mathf.RoundToInt(price);
        inventorySellPrices.Add(finalPrice);

        InventoryVersion++;

        return true;
    }
    public int DropNewestExcessFish()
    {
        EnsureInventoryData();

        int droppedCount = 0;
        float currentWeight = TotalWeight;

        while (currentWeight > CurrentMaxWeight + 0.001f &&
               inventory.Count > 0)
        {
            int lastIndex = inventory.Count - 1;

            currentWeight -= inventoryWeights[lastIndex];

            RemoveInventoryAt(lastIndex);

            droppedCount++;
        }

        if (droppedCount > 0)
            InventoryVersion++;

        return droppedCount;
    }

    public void DropExcessFish()
    {
        DropNewestExcessFish();
    }

    public void DropRandomFish()
    {
        EnsureInventoryData();

        if (inventory.Count == 0)
            return;

        int index = UnityEngine.Random.Range(0, inventory.Count);
        RemoveInventoryAt(index);

        InventoryVersion++;
    }

    public int SellAllFish()
    {
        EnsureInventoryData();

        int total = 0;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
                total += inventorySellPrices[i];
        }

        coins += total;

        if (inventory.Count > 0)
        {
            inventory.Clear();
            inventoryWeights.Clear();
            inventorySellPrices.Clear();

            InventoryVersion++;

            SaveGame();
        }

        return total;
    }

    public void LoseAllFish()
    {
        if (inventory.Count > 0)
        {
            inventory.Clear();
            inventoryWeights.Clear();
            inventorySellPrices.Clear();

            InventoryVersion++;
        }
    }
    void EnsureInventoryData()
    {
        if (inventory == null)
            inventory = new List<FishType>();

        if (inventoryWeights == null)
            inventoryWeights = new List<float>();

        if (inventorySellPrices == null)
            inventorySellPrices = new List<int>();

        while (inventoryWeights.Count < inventory.Count)
        {
            FishType fish = inventory[inventoryWeights.Count];

            float weight = fish != null
                ? Mathf.Max(0f, fish.weight)
                : 0f;

            inventoryWeights.Add(weight);
        }

        while (inventoryWeights.Count > inventory.Count)
            inventoryWeights.RemoveAt(inventoryWeights.Count - 1);

        while (inventorySellPrices.Count < inventory.Count)
        {
            int index = inventorySellPrices.Count;
            FishType fish = inventory[index];

            int price = 0;

            if (fish != null)
            {
                int rarity = Mathf.Clamp(fish.rarity, 1, 7);
                float averageWeight = Mathf.Max(0.01f, fish.weight);

                float calculatedPrice =
                    50f *
                    (inventoryWeights[index] / averageWeight) *
                    Mathf.Pow(2f, rarity) /
                    (1.5f * rarity);

                price = Mathf.RoundToInt(calculatedPrice);
            }

            inventorySellPrices.Add(price);
        }

        while (inventorySellPrices.Count > inventory.Count)
            inventorySellPrices.RemoveAt(inventorySellPrices.Count - 1);
    }

    void RemoveInventoryAt(int index)
    {
        inventory.RemoveAt(index);
        inventoryWeights.RemoveAt(index);
        inventorySellPrices.RemoveAt(index);
    }

    public float GetFishWeight(int index)
    {
        EnsureInventoryData();

        if (index < 0 || index >= inventory.Count)
            return 0;

        return inventoryWeights[index];
    }

    public int GetFishSellPrice(int index)
    {
        EnsureInventoryData();

        if (index < 0 || index >= inventory.Count)
            return 0;

        return inventorySellPrices[index];
    }
    public void SaveGame()
    {
        EnsureUpgradeArray();
        EnsureInventoryData();

        SaveData data = new SaveData();

        data.coins = coins;
        data.upgradeLevels = (int[])upgradeLevels.Clone();

        for (int i = 0; i < inventory.Count; i++)
        {
            FishType fish = inventory[i];

            if (fish == null)
            {
                Debug.LogError(
                    "GameManager: невозможно сохранить пустую ссылку на рыбу!"
                );
                return;
            }

            if (fishCatalog == null || !fishCatalog.Contains(fish))
            {
                Debug.LogError(
                    "GameManager: рыба " + fish.name +
                    " отсутствует в Fish Catalog. Сохранение прервано!"
                );
                return;
            }

            data.fishNames.Add(fish.name);
            data.inventoryWeights.Add(inventoryWeights[i]);
            data.inventorySellPrices.Add(inventorySellPrices[i]);
        }

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SaveFilePath, json);

            Debug.Log("Игра успешно сохранена!");
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка сохранения игры: " + e.Message);
        }
    }


    void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            ResetGameData("Файл сохранения отсутствует.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                ResetGameData("Файл сохранения пустой.");
                return;
            }

            if (!ValidateSaveJsonTypes(json))
            {
                ResetGameData("Неправильные типы или структура JSON.");
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (!TryValidateSaveData(
                data,
                out List<FishType> loadedFish,
                out List<float> loadedWeights,
                out List<int> loadedPrices))
            {
                ResetGameData("Данные сохранения не прошли проверку.");
                return;
            }

            coins = data.coins;
            upgradeLevels = (int[])data.upgradeLevels.Clone();

            inventory.Clear();
            inventoryWeights.Clear();
            inventorySellPrices.Clear();

            inventory.AddRange(loadedFish);
            inventoryWeights.AddRange(loadedWeights);
            inventorySellPrices.AddRange(loadedPrices);

            InventoryVersion++;

            Debug.Log("GameManager: сохранение успешно загружено.");
        }
        catch (Exception e)
        {
            ResetGameData("Ошибка чтения JSON: " + e.Message);
        }
    }

    private bool ValidateSaveJsonTypes(string json)
    {
        // Монеты должны быть целым числом.
        if (!Regex.IsMatch(
            json,
            "\"coins\"\\s*:\\s*-?\\d+\\s*[,}]"))
            return false;

        // Все обязательные поля должны присутствовать.
        string[] requiredFields =
        {
        "upgradeLevels",
        "fishNames",
        "inventoryWeights",
        "inventorySellPrices"
    };

        foreach (string field in requiredFields)
        {
            if (!Regex.IsMatch(
                json,
                "\"" + field + "\"\\s*:\\s*\\["))
                return false;
        }

        // Проверяем типы элементов числовых массивов.
        if (!ValidateNumericArray(json, "upgradeLevels", true))
            return false;

        if (!ValidateNumericArray(json, "inventoryWeights", false))
            return false;

        if (!ValidateNumericArray(json, "inventorySellPrices", true))
            return false;

        return true;
    }

    private bool ValidateNumericArray(
        string json,
        string fieldName,
        bool integersOnly)
    {
        Match match = Regex.Match(
            json,
            "\"" + fieldName + "\"\\s*:\\s*\\[([^\\]]*)\\]"
        );

        if (!match.Success)
            return false;

        string content = match.Groups[1].Value.Trim();

        if (content.Length == 0)
            return true;

        string[] values = content.Split(',');

        foreach (string rawValue in values)
        {
            string value = rawValue.Trim();

            string pattern = integersOnly
                ? @"^-?\d+$"
                : @"^-?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$";

            if (!Regex.IsMatch(value, pattern))
                return false;
        }

        return true;
    }

    private bool TryValidateSaveData(
        SaveData data,
        out List<FishType> loadedFish,
        out List<float> loadedWeights,
        out List<int> loadedPrices)
    {
        loadedFish = new List<FishType>();
        loadedWeights = new List<float>();
        loadedPrices = new List<int>();

        if (data == null)
            return false;

        if (data.coins < 0 || data.coins > MaxSavedCoins)
            return false;

        // Должно быть ровно пять уровней улучшений.
        if (data.upgradeLevels == null ||
            data.upgradeLevels.Length != 5)
            return false;

        foreach (int level in data.upgradeLevels)
        {
            if (level < 0 || level > maxUpgradeLevel)
                return false;
        }

        if (data.fishNames == null ||
            data.inventoryWeights == null ||
            data.inventorySellPrices == null)
            return false;

        int count = data.fishNames.Count;

        if (count > MaxSavedFish ||
            data.inventoryWeights.Count != count ||
            data.inventorySellPrices.Count != count)
            return false;

        if (count > 0 && fishCatalog == null)
            return false;

        for (int i = 0; i < count; i++)
        {
            string fishName = data.fishNames[i];

            if (string.IsNullOrWhiteSpace(fishName))
                return false;

            FishType fish = fishCatalog.Find(
                f => f != null && f.name == fishName
            );

            if (fish == null)
                return false;

            float weight = data.inventoryWeights[i];
            int price = data.inventorySellPrices[i];

            if (float.IsNaN(weight) ||
                float.IsInfinity(weight) ||
                weight < 0f ||
                weight > 1000f)
                return false;

            if (price < 0 || price > MaxSavedFishPrice)
                return false;

            loadedFish.Add(fish);
            loadedWeights.Add(weight);
            loadedPrices.Add(price);
        }

        return true;
    }
    private void ResetGameData(string reason)
    {
        Debug.LogWarning(
            "GameManager: сохранение сброшено. Причина: " + reason
        );

        coins = 0;
        upgradeLevels = new int[5];

        if (inventory == null)
            inventory = new List<FishType>();
        else
            inventory.Clear();

        if (inventoryWeights == null)
            inventoryWeights = new List<float>();
        else
            inventoryWeights.Clear();

        if (inventorySellPrices == null)
            inventorySellPrices = new List<int>();
        else
            inventorySellPrices.Clear();

        InventoryVersion++;

        SaveGame();

        Debug.Log("GameManager: создано новое базовое сохранение.");
    }
    public int InventoryCount => inventory.Count;
}
