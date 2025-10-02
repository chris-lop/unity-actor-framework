using LastDescent.Gameplay.Combat;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private LifeState life;
    private ProgressBar bar;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        bar = root.Q<ProgressBar>("HealthBar");
        if (bar == null || life == null) return; // Configure bar range to match max HP

        bar.lowValue = 0f;
        bar.highValue = life.MaxHealth;
        bar.value = life.CurrentHealth;
        life.OnHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        if (life != null) life.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(float current, float max)
    {
        if (bar == null) return;

        if (!Mathf.Approximately(bar.highValue, max))
            bar.highValue = max;

        bar.value = current;
        bar.title = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
}
