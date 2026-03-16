using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    private IDamagable damagable;

    private void Awake()
    {
        damagable = GetComponent<IDamagable>();
        healthBar.fillAmount = damagable.MaxHealth;
    }

    private void OnEnable()
    {
        GameEvents.OnUpdateHealth += UpdateHealth;
    }

    private void OnDisable()
    {
        GameEvents.OnUpdateHealth -= UpdateHealth;
    }

    public void UpdateHealth()
    {
        healthBar.fillAmount = damagable.Health / damagable.MaxHealth;
    }
}
