using UnityEngine;

/// <summary>
/// Base class for all in-game menus. Provides open/close logic and 
/// virtual input handlers that child menus can override.
/// </summary>
public abstract class BaseMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    [Tooltip("Root canvas group for fading and enabling/disabling input.")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected Canvas canvas;

    protected bool isOpen = false;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvas == null)
            canvas = GetComponent<Canvas>();
    }

    public virtual void Open()
    {
        isOpen = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        OnOpen();
    }

    public virtual void Close()
    {
        isOpen = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        OnClose();
    }

    public virtual void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (canvas != null)
            canvas.sortingOrder -= 1;
    }

    public virtual void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        if (canvas != null)
            canvas.sortingOrder += 1;
    }

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }

    // --- Input handling ---
    public virtual void HandleNavigate(Vector2 direction) { }
    public virtual void HandleSubmit() { }
    public virtual void HandleCancel() { }
}
