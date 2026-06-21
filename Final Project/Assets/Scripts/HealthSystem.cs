using UnityEngine;
using UnityEngine.UI;
public class HealthSystem : MonoBehaviour
{
    private int health = 100;
    private const int HEALTHMAX = 100;
    public Slider healthUI;
    private void Awake()
    {
        UpdateHealthUI();
    }
    public void DealDamage(int dmg)
    {
        health -= dmg;
        UpdateHealthUI();
        if (health <= 0) Destroy(gameObject);
    }
    public void HealDamage(int heal)
    {
        health = Mathf.Min(health + heal, HEALTHMAX);
        UpdateHealthUI();
    }
    private void UpdateHealthUI() => healthUI.value = health;
}