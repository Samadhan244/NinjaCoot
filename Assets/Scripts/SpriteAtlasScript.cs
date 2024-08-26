using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SpriteAtlasScript : MonoBehaviour
{
    [SerializeField] SpriteAtlas spriteAtlas;
    [SerializeField] string spriteName;

    void Start()
    {
        GetComponent<Image>().sprite = spriteAtlas.GetSprite(spriteName);
    }
}
