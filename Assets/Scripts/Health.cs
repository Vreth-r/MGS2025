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
        //normal + easy
        if (gameSettings.gameMode == 1 || gameSettings.gameMode == 0)
        {
            HealthValue = Mathf.Max(0f, HealthValue - amount);
        }

        //hard
        else if (gameSettings.gameMode == 2)
        {
            HealthValue = Mathf.Max(0f, HealthValue - (amount * 2));
        }

        //nightmare (3)
        else
        {
            HealthValue = Mathf.Max(0f, HealthValue - amount * 3);
        }

        UpdateUI();
    }

    public static void Regen(float amount)
    {
        if (IsDead()) return;

        //hard mode 3 regen
        if (gameSettings.gameMode == 2)
        {
           // Debug.Log("hard mode diff");
            HealthValue = Mathf.Min(MaxHealth, HealthValue + 3);
        }

        //nightmare mode 0 regen
        else if (gameSettings.gameMode == 3)
        {
           // Debug.Log("nightmare mode diff");
            return;
        }

        //easy mode double regen
        else if (gameSettings.gameMode == 0)
        {
            //Debug.Log("easy mode diff");
            HealthValue = Mathf.Min(MaxHealth, HealthValue + (amount * 2));
        }

        //normal mode (1) + any other non-specified mode gets normal regen
        else
        {
           // Debug.Log("normal mode diff");
            HealthValue = Mathf.Min(MaxHealth, HealthValue + amount);
        }

        UpdateUI();
    }

    //specifically the healing when ult is active
    //this is its own thing so it doesnt get affected by gamemode multipliers (aka check above)
    public static void UltHealthRegen(float amount)
    {
        if (IsDead()) return;

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