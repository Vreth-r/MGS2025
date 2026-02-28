using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float inspectorMaxHealth = 100f;
    [SerializeField] private Image inspectorHealthBar;
    [SerializeField] private static bool logChanges = false;

    public static float maxHealth { get; private set; } = 100f;
    public static float health { get; private set; }

    private static Image healthBar;

    void Awake()
    {
        maxHealth = inspectorMaxHealth;
        healthBar = inspectorHealthBar;
        health = maxHealth;
        UpdateUI();
    }

    public static void TakeDamage(float amount)
    {
        if (IsDead()) return;

        health = Mathf.Max(0f, health - amount);
        UpdateUI();

        if (logChanges) Debug.Log($"[Health] -{amount} => {health}/{maxHealth}");
        if (health <= 0f && logChanges) Debug.Log("[Health] Player died");
    }

    public static void Regen(float amount)
    {
        if (IsDead()) return;

        health = Mathf.Min(maxHealth, health + amount);
        UpdateUI();

        if (logChanges) Debug.Log($"[Health] +{amount} => {health}/{maxHealth}");
    }

    public static bool IsDead() => health <= 0f;

    private static void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = (maxHealth <= 0f) ? 0f : (health / maxHealth);
    }
}