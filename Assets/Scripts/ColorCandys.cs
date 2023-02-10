using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCandys : MonoBehaviour
{
    public enum ColorType
    {
        YELLOW,
        RED,
        GREEN,
        PURPLE,
        ANY
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };


    private ColorType color;
    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    public ColorSprite[] colorSprites;
    public int NumColors
    {
        get { return colorSprites.Length; }
    }

    private SpriteRenderer sprite;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();
        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
}
