using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float inspectorMaxHealth = 100f;
    [SerializeField] private Image inspectorHealthBar;

    public static Image healthBar;
    public static float maxHealth = 100f;
    public static float health;

    void Start()
    {
        maxHealth = inspectorMaxHealth;
        healthBar = inspectorHealthBar;
        health = maxHealth;
    }

    public static void TakeDamage(float damageIncrement)
    {
        if (health - damageIncrement < 0) 
        { 
            health = 0;
            Debug.Log("player should die");
        }
        else
            health -= damageIncrement;
        Debug.Log("miss... hp is now at: " + Health.health);
        healthBar.fillAmount = health / 100f;
    }
    public static void Regen(float regenIncrement)
    {
        if (Health.IsDead()) return;
        if (health + regenIncrement > maxHealth)
            health = maxHealth;
        else
            health += regenIncrement;
        Debug.Log("hit! hp is now at: " + Health.health);
        healthBar.fillAmount = health / 100f;    
    }

    public static bool IsDead()
    {
        return Health.health <= 0;
    }

}
