using UnityEngine;

public class SongCompleteScreen : MonoBehaviour
{
    public RectTransform mask;
    public float duration = 1f;
    private float targetWidth;
    public GameObject background;
    
    void Awake()
    {
        Canvas.ForceUpdateCanvases();
        targetWidth = mask.rect.width;
        mask.sizeDelta = new Vector2(0, mask.sizeDelta.y);
    }
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        background.SetActive(false);
    }
     
    public void TrackFinish()
    {
        StartCoroutine(Reveal());
    }


    // RevealContainer is revealed from left to right
    System.Collections.IEnumerator Reveal()
    {
        yield return null;
        background.SetActive(true);
        float time = 0f;
        while (time < duration)
        {
            float t = Mathf.SmoothStep(0, 1, time / duration);
            float width = Mathf.Lerp(0, targetWidth, t);
            mask.sizeDelta = new Vector2(width, mask.sizeDelta.y);
            time += Time.deltaTime;
            yield return null;
        }
        mask.sizeDelta = new Vector2(targetWidth, mask.sizeDelta.y);
    }
}
