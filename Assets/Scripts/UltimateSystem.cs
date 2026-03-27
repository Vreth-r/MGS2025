using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UltimateSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public Image ultimateBar;
    [SerializeField] public GameObject fullUlt;

    [Header("Charge Rules")]
    [SerializeField] private float maxCharge = 10f;
    [SerializeField] private float passiveChargeIntervalSeconds = 2f; 
    [SerializeField] private float passiveChargePerTick = 1f;         
    [SerializeField] private float chargePerNoteHit = 0.5f;           

    [Header("Ultimate")]
    [SerializeField] private float ultimateDuration = 10f;

    public static event Action OnUltimateStarted;
    public static event Action OnUltimateFinished;

    public static bool UltimateActive { get; private set; }

    
    public static float Charge { get; private set; }

    private static UltimateSystem instance;
    private float passiveTimer;

    private void Awake()
    {
        instance = this;
        Charge = 0f;
        UltimateActive = false;
        passiveTimer = 0f;
        UpdateBar();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnUltimatePressed += TryActivate;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnUltimatePressed -= TryActivate;
    }

    private void Update()
    {
        if (UltimateActive) return;

        
        if (GameManager.Instance != null && GameManager.Instance.GameIsDone())
            return;

        if (Charge >= maxCharge) return;

        passiveTimer += Time.deltaTime;

        if (passiveTimer >= passiveChargeIntervalSeconds)
        {
            int ticks = Mathf.FloorToInt(passiveTimer / passiveChargeIntervalSeconds);
            passiveTimer -= ticks * passiveChargeIntervalSeconds;

            AddChargeInternal(ticks * passiveChargePerTick);
        }
    }

    public static void AddChargeForSuccessfulNote()
    {
        if (instance == null) return;
        if (UltimateActive) return;

        instance.AddChargeInternal(instance.chargePerNoteHit);
    }

    private void AddChargeInternal(float amount)
    {
        if (amount <= 0f) return;

        Charge = Mathf.Clamp(Charge + amount, 0f, maxCharge);
        UpdateBar();
    }

    private void TryActivate()
    {
        if (UltimateActive) return;
        if (Charge < maxCharge) return;

        
        Charge = 0f;
        passiveTimer = 0f;
        UpdateBar();

        UltimateActive = true;
        OnUltimateStarted?.Invoke();
        StartCoroutine(UltRoutine());
    }

    private IEnumerator UltRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, ultimateDuration));
        UltimateActive = false;
        OnUltimateFinished?.Invoke();
    }

    private void UpdateBar()
    {
        if (ultimateBar == null) return;
        ultimateBar.fillAmount = (maxCharge <= 0f) ? 0f : (Charge / maxCharge);
        if (Charge == maxCharge)
        {
            fullUlt.SetActive(true);
        }
        else
        {
            fullUlt.SetActive(false);
        }

    }
}