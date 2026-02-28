using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UltimateSystem : MonoBehaviour
{
    [Header("Inspector")]
    [SerializeField] private Image inspectorUltimateBar;
    [SerializeField] private int inspectorUltimateDivider = 3;
    [SerializeField] private float inspectorUltimateDuration = 10f;

    private GameManager manager;

    // Ultimate settings (static API kept to avoid rewriting other scripts)
    public static Image ultimateBar;
    private static int totalNoteCount;
    private static int ultimateNoteCountProgress = 0;
    private static int ultimateNoteCountThreshold;
    public static int ultimateDivider = 3;
    private static int usedUltimateCount;
    public static float ultimateDuration = 10f;

    public static event Action OnUltimateStarted;
    public static event Action OnUltimateFinished;

    public static UltimateSystem Instance { get; private set; }
    public static bool UltimateActive { get; private set; }

    private static bool warnedMissingBar;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize visuals/settings ASAP (Awake runs before Start)
        ultimateBar = inspectorUltimateBar;
        ultimateDivider = Mathf.Max(1, inspectorUltimateDivider);
        ultimateDuration = Mathf.Max(0.01f, inspectorUltimateDuration);

        if (ultimateBar != null)
            ultimateBar.fillAmount = 0f;
    }

    private void Start()
    {
        // GameManager might not be ready in Awake; grab it here
        manager = GameManager.Instance;

        // If noteCount is set later, this may be 0 at first — so keep it safe
        totalNoteCount = manager != null ? manager.noteCount : 0;
        RebuildThreshold();
    }

    private static void RebuildThreshold()
    {
        ultimateDivider = Mathf.Max(1, ultimateDivider);
        ultimateNoteCountThreshold = Mathf.Max(1, totalNoteCount / ultimateDivider);
    }

    private static bool EnsureInitialized()
    {
        if (Instance == null)
            Instance = FindFirstObjectByType<UltimateSystem>();

        // If we found it late, pull fields
        if (Instance != null && ultimateBar == null)
        {
            ultimateBar = Instance.inspectorUltimateBar;
            ultimateDivider = Mathf.Max(1, Instance.inspectorUltimateDivider);
            ultimateDuration = Mathf.Max(0.01f, Instance.inspectorUltimateDuration);
        }

        if (ultimateBar == null)
        {
            if (!warnedMissingBar)
            {
                warnedMissingBar = true;
                Debug.LogError("[UltimateSystem] ultimateBar is null. Assign inspectorUltimateBar in the scene.");
            }
            return false;
        }

        // If threshold is invalid (often because totalNoteCount was 0), keep it safe
        if (ultimateNoteCountThreshold <= 0)
            ultimateNoteCountThreshold = 1;

        return true;
    }

    public static void IncrementUltimate()
    {
        if (!EnsureInitialized()) return;

        if (usedUltimateCount >= ultimateDivider - 1)
        {
            ultimateBar.enabled = false;
            return;
        }

        if (UltimateActive) return;

        ultimateNoteCountProgress = Mathf.Min(ultimateNoteCountProgress + 1, ultimateNoteCountThreshold);
        ultimateBar.fillAmount = (float)ultimateNoteCountProgress / (float)ultimateNoteCountThreshold;
    }

    public static void ActivateUltimate()
    {
        if (!EnsureInitialized()) return;

        if (ultimateNoteCountProgress >= ultimateNoteCountThreshold)
        {
            ultimateNoteCountProgress = 0;

            OnUltimateStarted?.Invoke();
            UltimateActive = true;

            // Instance is required for coroutines
            Instance.StartCoroutine(UltimateScoreModifier());
        }
    }

    private static IEnumerator UltimateScoreModifier()
    {
        float elapsedTime = 0f;
        float startFill = ultimateBar.fillAmount;

        while (elapsedTime < ultimateDuration)
        {
            elapsedTime += Time.deltaTime;
            float a = elapsedTime / ultimateDuration;
            ultimateBar.fillAmount = Mathf.Lerp(startFill, 0f, a);
            yield return null;
        }

        ultimateBar.fillAmount = 0f;

        OnUltimateFinished?.Invoke();
        UltimateActive = false;
    }
}