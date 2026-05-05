using Unity.VisualScripting;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    int health = 100;
    public void DealDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Destroy(gameObject);
            Destroy(this);
        }
    }
    public void HealDamage(int heal)
    {
        health = (health + heal > 100) ? 100 : health + heal;
    }
}
