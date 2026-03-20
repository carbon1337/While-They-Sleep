/*

Updates the noise meter UI based on the NoiseMeter value.
Controls slider fill amount, fill color, UI visibility fading,
and screen shake when noise is dangerously high.

*/
using UnityEngine;
using UnityEngine.UI;

public class NoiseMeterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("The Image component used as the Slider Fill.")]
    [SerializeField] private Image fillImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInSpeed = 8f;
    [SerializeField] private float fadeOutSpeed = 4f;

    [Tooltip("How long the UI stays visible after the last noise event.")]
    [SerializeField] private float visibleHoldTime = 3f;

    [Header("Color Settings")]
    [SerializeField] private Gradient fillGradient;

    [Header("Shake Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float shakeStart = 0.75f;

    [SerializeField] private float maxShakePixels = 6f;
    [SerializeField] private float shakeSpeed = 30f;

    private RectTransform rect;
    private Vector2 baseAnchoredPos;

    private float visibleTimer = 0f;
    private float shakeT = 0f;

    #region Initialization
    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

        rect = GetComponent<RectTransform>();

        if (rect != null)
            baseAnchoredPos = rect.anchoredPosition;

        SetupDefaultGradient();
    }

    private void SetupDefaultGradient()
    {
        //Fallback gradient if none assigned
        if (fillGradient == null || fillGradient.colorKeys.Length == 0)
        {
            fillGradient = new Gradient();

            fillGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.green, 0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(new Color(0.6f,0f,0f),1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f,0f),
                    new GradientAlphaKey(1f,1f)
                }
            );
        }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (NoiseMeter.Instance == null || slider == null || canvasGroup == null)
            return;

        float noise = NoiseMeter.Instance.NormalizedNoise;

        UpdateSlider(noise);
        UpdateFillColor(noise);
        UpdateVisibility();
        UpdateFade();
        ApplyShake(noise);
    }
    #endregion

    #region UI
    private void UpdateSlider(float normalizedNoise)
    {
        slider.value = normalizedNoise;
    }

    private void UpdateFillColor(float normalizedNoise)
    {
        if (fillImage == null)
            return;

        fillImage.color = fillGradient.Evaluate(normalizedNoise);
    }

    private void UpdateVisibility()
    {
        if (!NoiseMeter.Instance.IsSilent)
        {
            visibleTimer = visibleHoldTime;
        }
        else
        {
            visibleTimer = Mathf.Max(0f, visibleTimer - Time.deltaTime);
        }
    }

    private void UpdateFade()
    {
        bool shouldShow = visibleTimer > 0f;

        float targetAlpha = shouldShow ? 1f : 0f;
        float speed = shouldShow ? fadeInSpeed : fadeOutSpeed;

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, speed * Time.deltaTime);

        bool visible = canvasGroup.alpha > 0.01f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
    #endregion

    #region Shake
    private void ApplyShake(float normalizedNoise)
    {
        if (rect == null)
            return;

        if (canvasGroup.alpha < 0.01f)
        {
            rect.anchoredPosition = baseAnchoredPos;
            return;
        }

        if (normalizedNoise < shakeStart)
        {
            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                baseAnchoredPos,
                20f * Time.deltaTime
            );
            return;
        }

        float t = Mathf.InverseLerp(shakeStart, 1f, normalizedNoise);
        float amplitude = Mathf.Lerp(0f, maxShakePixels, t);

        shakeT += Time.deltaTime * shakeSpeed;

        float x = (Mathf.PerlinNoise(shakeT, 0.1f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0.1f, shakeT) - 0.5f) * 2f;

        rect.anchoredPosition = baseAnchoredPos + new Vector2(x, y) * amplitude;
    }
    #endregion
}