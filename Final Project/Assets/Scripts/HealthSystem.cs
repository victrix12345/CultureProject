using UnityEngine;
using UnityEngine.UI;
public class HealthSystem : MonoBehaviour
{
    private int health = 100;
    private const int HEALTHMAX = 100;
    public Slider healthUI;
    private bool isPlayer = false;
    private void Awake()
    {
        if (GetComponentInParent<PlayerSystems>() != null) isPlayer = true;
        if (isPlayer) UpdateHealthUI();
    }
    public void DealDamage(int dmg)
    {
        health -= dmg;
        if (isPlayer) UpdateHealthUI();
        if (health <= 0) Destroy(gameObject);
    }
    public void HealDamage(int heal)
    {
        health = Mathf.Min(health + heal, HEALTHMAX);
        if (isPlayer) UpdateHealthUI();
    }
    private void UpdateHealthUI() => healthUI.value = health;
}