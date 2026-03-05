using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Image background;
    [SerializeField] private Button button;

    public Button Button => button;
    public string Id { get; private set; }

    public void Set(string name, string type)
    {
        nameText.text = name;
        typeText.text = type;
    }

    public void SetSelected(bool selected)
    {
        if (background == null) return;

        var c = background.color;
        c.a = selected ? 0.35f : 0.15f;
        background.color = c;
    }

    public void SetId(string id) => Id = id;
}