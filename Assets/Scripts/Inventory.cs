using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject slotPrefab;
    public TMP_Text label;

    private int lastInventoryVersion = -1;

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        GameManager gm = GameManager.Instance;

        if (label != null)
        {
            label.text =
                $"{gm.TotalWeight:F2}/{gm.CurrentMaxWeight:F2}";
        }

        if (gm.InventoryVersion == lastInventoryVersion)
            return;

        lastInventoryVersion = gm.InventoryVersion;
        Rebuild(gm);
    }

    void Rebuild(GameManager gm)
    {
        if (slotsParent == null || slotPrefab == null)
            return;

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotsParent.GetChild(i).gameObject);
        }

        foreach (FishType fish in gm.inventory)
        {
            GameObject slot = Instantiate(
                slotPrefab,
                slotsParent
            );

            Image image = slot.GetComponent<Image>();

            if (image != null &&
                fish != null &&
                fish.sprite != null)
            {
                image.sprite = fish.sprite;
            }
        }
        for (int i = 0; i < gm.inventory.Count; i++)
        {
            Debug.Log(
                $"Рыба: {gm.inventory[i].fishName}, " +
                $"вес: {gm.GetFishWeight(i):F3}"
            );
        }

        Debug.Log($"Общий вес: {gm.TotalWeight:F3}");
    }
}
