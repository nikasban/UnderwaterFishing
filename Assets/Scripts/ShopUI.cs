
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeRowUI
    {
        public UpgradeType type;
        public TMP_Text infoText;
        public Button buyButton;

        [HideInInspector]
        public TMP_Text buyButtonText;
    }

    [SerializeField] private GameObject salePage;
    [SerializeField] private GameObject upgradePage;

    [SerializeField] private TMP_Text pageTitle;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private TMP_Text saleInfoText;
    [SerializeField] private Button sellAllButton;

    [SerializeField] private Button saleMenuButton;
    [SerializeField] private Button upgradeMenuButton;
    [SerializeField] private Button exitMenuButton;
    [SerializeField]
    private UpgradeRowUI[] upgradeRows =
        new UpgradeRowUI[5];
    [SerializeField] private bool pauseGameWhileOpen = true;

    private bool initialized;
    private bool isOpen;
    private bool showingUpgradePage;

    private float previousTimeScale = 1f;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        if (saleMenuButton != null)
            saleMenuButton.onClick.AddListener(ShowSalePage);

        if (upgradeMenuButton != null)
            upgradeMenuButton.onClick.AddListener(ShowUpgradePage);

        if (exitMenuButton != null)
            exitMenuButton.onClick.AddListener(CloseShop);

        if (sellAllButton != null)
            sellAllButton.onClick.AddListener(SellAllFish);

        if (upgradeRows != null)
        {
            foreach (UpgradeRowUI row in upgradeRows)
            {
                if (row == null || row.buyButton == null)
                    continue;

                UpgradeType capturedType = row.type;

                row.buyButton.onClick.AddListener(
                    () => PurchaseUpgrade(capturedType)
                );

                row.buyButtonText =
                    row.buyButton.GetComponentInChildren<TMP_Text>(true);
            }
        }

        ShowSalePage();
    }

    public void OpenShop()
    {
        Initialize();

        if (!isOpen && pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        isOpen = true;

        gameObject.SetActive(true);

        ShowSalePage();
    }

    public void CloseShop()
    {
        if (isOpen && pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        isOpen = false;
        gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        CloseShop();
    }

    void OnDisable()
    {
        if (isOpen && pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        isOpen = false;
    }

    void Update()
    {
        if (!isOpen || GameManager.Instance == null)
            return;

        RefreshCoins();

        if (showingUpgradePage)
            RefreshUpgradePage();
        else
            RefreshSalePage();
    }

    public void ShowSalePage()
    {
        showingUpgradePage = false;

        if (salePage != null)
            salePage.SetActive(true);

        if (upgradePage != null)
            upgradePage.SetActive(false);

        if (pageTitle != null)
            pageTitle.text = "ПРОДАЖА";

        if (statusText != null)
            statusText.text = "";

        RefreshSalePage();
    }

    public void ShowUpgradePage()
    {
        showingUpgradePage = true;

        if (salePage != null)
            salePage.SetActive(false);

        if (upgradePage != null)
            upgradePage.SetActive(true);

        if (pageTitle != null)
            pageTitle.text = "ПРОКАЧКА";

        if (statusText != null)
            statusText.text = "";

        RefreshUpgradePage();
    }

    void RefreshCoins()
    {
        if (coinsText == null || GameManager.Instance == null)
            return;

        coinsText.text =
            "МОНЕТЫ: " + GameManager.Instance.coins;
    }

    void RefreshSalePage()
    {
        if (GameManager.Instance == null ||
            saleInfoText == null)
            return;

        GameManager gm = GameManager.Instance;

        Dictionary<string, int> fishCounts =
            new Dictionary<string, int>();

        int totalValue = 0;

        for (int i = 0; i < gm.inventory.Count; i++)
        {
            FishType fish = gm.inventory[i];

            if (fish == null)
                continue;

            string name = string.IsNullOrEmpty(fish.fishName)
                ? "Рыба"
                : fish.fishName;

            if (!fishCounts.ContainsKey(name))
                fishCounts[name] = 0;

            fishCounts[name]++;
            totalValue += gm.GetFishSellPrice(i);
        }

        StringBuilder text = new StringBuilder();

        text.AppendLine("ТВОЙ УЛОВ");
        text.AppendLine();

        if (fishCounts.Count == 0)
        {
            text.AppendLine("У тебя пока нет рыбы.");
        }
        else
        {
            foreach (var fish in fishCounts)
            {
                text.AppendLine(
                    fish.Key + " x" + fish.Value
                );
            }
        }

        text.AppendLine();
        text.AppendLine(
            "Вес: " + gm.TotalWeight.ToString("F2") +
            " / " + gm.CurrentMaxWeight.ToString("F2") + " кг"
        );

        text.AppendLine(
            "Общая стоимость: " + totalValue + " монет"
        );

        saleInfoText.text = text.ToString();

        if (sellAllButton != null)
            sellAllButton.interactable = gm.inventory.Count > 0;
    }

    void SellAllFish()
    {
        if (GameManager.Instance == null)
            return;

        int earned = GameManager.Instance.SellAllFish();

        if (statusText != null)
        {
            statusText.text = earned > 0
                ? "ПРОДАНО! +" + earned + " МОНЕТ"
                : "НЕЧЕГО ПРОДАВАТЬ";
        }

        RefreshSalePage();
        RefreshCoins();
    }

    void RefreshUpgradePage()
    {
        if (GameManager.Instance == null ||
            upgradeRows == null)
            return;

        GameManager gm = GameManager.Instance;

        foreach (UpgradeRowUI row in upgradeRows)
        {
            if (row == null)
                continue;

            int level = gm.GetUpgradeLevel(row.type);
            int cost = gm.GetUpgradeCost(row.type);

            if (row.infoText != null)
            {
                row.infoText.text =
                    GetUpgradeName(row.type) +
                    "\nУровень: " + level +
                    "/" + gm.maxUpgradeLevel +
                    "\n" + GetUpgradeDescription(row.type, gm) +
                    "\n" + GetCurrentEffect(row.type, gm);
            }

            bool isMaxLevel = level >= gm.maxUpgradeLevel;

            if (row.buyButton != null)
                row.buyButton.interactable =
                    !isMaxLevel && gm.coins >= cost;

            if (row.buyButtonText != null)
            {
                row.buyButtonText.text = isMaxLevel
                    ? "МАКС."
                    : "КУПИТЬ\n" + cost;
            }
        }
    }

    void PurchaseUpgrade(UpgradeType type)
    {
        if (GameManager.Instance == null)
            return;

        bool success =
            GameManager.Instance.TryPurchaseUpgrade(type);

        if (statusText != null)
        {
            statusText.text = success
                ? "УЛУЧШЕНИЕ ПРИОБРЕТЕНО!"
                : "НЕДОСТАТОЧНО МОНЕТ!";
        }

        RefreshUpgradePage();
        RefreshCoins();
    }

    string GetUpgradeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.HarpoonRange:
                return "ДАЛЬНОСТЬ ГАРПУНА";

            case UpgradeType.ViewRange:
                return "ДАЛЬНОСТЬ ОБЗОРА";

            case UpgradeType.OxygenCapacity:
                return "КИСЛОРОДНЫЙ БАЛЛОН";

            case UpgradeType.BagCapacity:
                return "ВМЕСТИМОСТЬ МЕШКА";

            case UpgradeType.Armor:
                return "БРОНЯ";
        }

        return "УЛУЧШЕНИЕ";
    }

    string GetUpgradeDescription(
        UpgradeType type,
        GameManager gm)
    {
        switch (type)
        {
            case UpgradeType.HarpoonRange:
                return "+" +
                    gm.harpoonRangePerLevel +
                    " к дальности гарпуна";

            case UpgradeType.ViewRange:
                return "Дополнительный обзор на глубине";

            case UpgradeType.OxygenCapacity:
                return "+" +
                    gm.oxygenPerLevel +
                    " секунд кислорода";

            case UpgradeType.BagCapacity:
                return "+" +
                    gm.bagCapacityPerLevel +
                    " кг к вместимости";

            case UpgradeType.Armor:
                return "Снижение входящего урона";
        }

        return "";
    }

    string GetCurrentEffect(
        UpgradeType type,
        GameManager gm)
    {
        int level = gm.GetUpgradeLevel(type);

        switch (type)
        {
            case UpgradeType.HarpoonRange:
                return "Текущий бонус: +" +
                    (level * gm.harpoonRangePerLevel);

            case UpgradeType.ViewRange:
                return "Уровень обзора: " + level;

            case UpgradeType.OxygenCapacity:
                return "Дополнительно: +" +
                    (level * gm.oxygenPerLevel) + " сек.";

            case UpgradeType.BagCapacity:
                return "Дополнительно: +" +
                    (level * gm.bagCapacityPerLevel) + " кг";

            case UpgradeType.Armor:
                float reduction = Mathf.Clamp(
                    level * gm.armorReductionPerLevel,
                    0f,
                    gm.maxArmorReduction
                );

                return "Снижение урона: -" +
                    (reduction * 100f).ToString("F0") + "%";
        }

        return "";
    }
}
