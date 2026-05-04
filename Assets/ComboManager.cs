using UnityEngine;
using System;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("Cài đặt combo")]
    public int comboThreshold = 3;    // Xuyên 3 tầng liên tiếp = kích hoạt
    public float comboMultiplier = 1.5f; // Hệ số nhân điểm thưởng
    public float comboResetTime = 2f;   // Giây đứng yên thì mất combo

    public event Action<int> OnComboUpdated; // Combo tăng
    public event Action OnComboReset;   // Combo về 0
    public event Action<int, int> OnComboBonus;   // (bonusScore, comboCount)

    int comboCount = 0;
    float lastPassTime = 0f;
    bool timerRunning = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (timerRunning && comboCount > 0)
            if (Time.time - lastPassTime > comboResetTime)
                ResetCombo();
    }

    // Gọi từ Ring.cs mỗi khi xuyên qua tầng
    public void RegisterPass(int baseScore)
    {
        comboCount++;
        lastPassTime = Time.time;
        timerRunning = true;

        OnComboUpdated?.Invoke(comboCount);

        if (comboCount >= comboThreshold)
        {
            int bonus = Mathf.RoundToInt(
                baseScore * comboMultiplier * (comboCount - comboThreshold + 1)
            );
            GameManager.instance?.AddScore(bonus);
            OnComboBonus?.Invoke(bonus, comboCount);

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Whoosh"); // đổi thành "ComboStreak" nếu có
        }
    }

    // Gọi khi bóng chạm tầng
    public void ResetCombo()
    {
        if (comboCount == 0) return;
        comboCount = 0;
        timerRunning = false;
        OnComboReset?.Invoke();
    }

    public int GetCombo() => comboCount;
}