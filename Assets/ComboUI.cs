using UnityEngine;
using TMPro;
using System.Collections;

public class ComboUI : MonoBehaviour
{
    [Header("Kéo thả UI vào đây")]
    public TextMeshProUGUI comboText;   // Text "3x COMBO!"
    public TextMeshProUGUI bonusText;   // Text "+150"
    public GameObject comboPanel; // Panel chứa cả 2 text

    [Header("Animation")]
    public float showDuration = 1.2f;
    public float punchScale = 1.4f;

    void OnEnable()
    {
        if (ComboManager.instance == null) return;
        ComboManager.instance.OnComboUpdated += HandleComboUpdated;
        ComboManager.instance.OnComboReset += HandleComboReset;
        ComboManager.instance.OnComboBonus += HandleComboBonus;
    }

    void OnDisable()
    {
        if (ComboManager.instance == null) return;
        ComboManager.instance.OnComboUpdated -= HandleComboUpdated;
        ComboManager.instance.OnComboReset -= HandleComboReset;
        ComboManager.instance.OnComboBonus -= HandleComboBonus;
    }

    void HandleComboUpdated(int count)
    {
        int threshold = ComboManager.instance.comboThreshold;
        if (count < threshold)
        {
            comboText.text = $"{count} / {threshold}";
            bonusText.text = "";
            comboPanel.SetActive(true);
        }
    }

    void HandleComboBonus(int bonus, int count)
    {
        comboText.text = $"{count}x COMBO!";
        bonusText.text = $"+{bonus}";
        comboPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PunchAndHide());
    }

    void HandleComboReset()
    {
        StopAllCoroutines();
        comboPanel.SetActive(false);
    }

    IEnumerator PunchAndHide()
    {
        // Phóng to
        float t = 0f;
        while (t < 0.12f)
        {
            comboPanel.transform.localScale =
                Vector3.one * Mathf.Lerp(1f, punchScale, t / 0.12f);
            t += Time.deltaTime;
            yield return null;
        }
        // Thu về
        t = 0f;
        while (t < 0.1f)
        {
            comboPanel.transform.localScale =
                Vector3.one * Mathf.Lerp(punchScale, 1f, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        comboPanel.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(showDuration);
        comboPanel.SetActive(false);
    }
}