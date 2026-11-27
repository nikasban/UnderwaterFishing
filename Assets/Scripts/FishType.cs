using UnityEngine;

public enum FishBehavior
{
    Friendly,
    Neutral,
    Timid,
    Aggressive
}

public enum FishSpecies
{
    Normal,
    Pufferfish,
    Swordfish
}

[CreateAssetMenu(fileName = "FishType", menuName = "Game/Fish Type")]
public class FishType : ScriptableObject
{
    public string fishName = "че то че то";

    [Header("Weight")]
    [Min(0.01f)]
    public float weight = 0.5f;

    [Header("Rarity")]
    [Range(1, 7)]
    public int rarity = 1;

    [Tooltip("Старая фиксированная цена. Новая цена рассчитывается по формуле.")]
    public int sellPrice = 10;

    public Sprite sprite;

    [Header("Behavior")]
    public FishBehavior behavior = FishBehavior.Friendly;

    public float detectRange = 3f;
    public float fleeSpeed = 4f;
    public float chaseSpeed = 3f;

    [Header("Swordfish")]
    public float damage = 15f;

    [Header("Pufferfish")]
    public float poisonDamage = 20f;
    public float poisonDuration = 4f;
    public float poisonTickInterval = 0.5f;
    [Header("Species")]
    public FishSpecies species = FishSpecies.Normal;
}