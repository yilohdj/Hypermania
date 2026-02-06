using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    public Sprite spriteDefault;
    public Sprite spritePressed;

    private Image imageComp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        imageComp = GetComponent<Image>();
        if (imageComp != null)
        {
            imageComp.sprite = spriteDefault;
        }
    }

    // Update is called once per frame
    public void ChangeSprite(bool pressed)
    {
        if (pressed)
        {
            imageComp.sprite = spritePressed;
        }
        else
        {
            imageComp.sprite = spriteDefault;
        }
    }
}
