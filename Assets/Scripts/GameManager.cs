using System.Collections.Generic;
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

        float randomWeight = Random.Range(
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

        int index = Random.Range(0, inventory.Count);

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
    public int InventoryCount => inventory.Count;
}
