using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float inspectorMaxHealth = 100f;
    [SerializeField] private Image inspectorHealthBar;

    public static float HealthValue { get; private set; }
    public static float MaxHealth { get; private set; }
    private static Image bar;

    private static GameSettings gameSettings;

    private void Awake()
    {
        gameSettings = Resources.Load<GameSettings>("GameSettings");

        if (gameSettings.godMode == true)
        {
            MaxHealth = 1000000000;
        }

        else
        {
            MaxHealth = inspectorMaxHealth;
        }

        HealthValue = MaxHealth;
        bar = inspectorHealthBar;
        UpdateUI();
    }

    public static void TakeDamage(float amount)
    {
        if (gameSettings.gameMode == 0)
        {
            HealthValue = Mathf.Max(0f, HealthValue - amount);
        }
        
        else if (gameSettings.gameMode == 1)
        {
            HealthValue = Mathf.Max(0f, HealthValue - (amount * 2));
        }

        else
        {
            HealthValue = Mathf.Max(0f, HealthValue - amount * 3);
        }

        UpdateUI();
    }

    public static void Regen(float amount)
    {
        if (IsDead()) return;

        if (gameSettings.gameMode == 2)
        {
            return;
        }

        HealthValue = Mathf.Min(MaxHealth, HealthValue + amount);

        UpdateUI();
    }

    public static bool IsDead() => HealthValue <= 0f;

    private static void UpdateUI()
    {
        if (bar != null)
            bar.fillAmount = (MaxHealth <= 0f) ? 0f : (HealthValue / MaxHealth);
    }
}