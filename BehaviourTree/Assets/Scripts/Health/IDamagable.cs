using UnityEngine;

public interface IDamagable
{
    float MaxHealth { get; }
    float Health { get; set; }
    void TakeDamage(float damage);
    void HandleDeath();
}
