using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] GameObject - Visual
//[][] Light framework for a respritable object

//[][] NOTES:
//[][] The value of _spriteRenderer must be manually set in the prefab in order for WordTool to work
public class GO_Visual : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>() ?? this.gameObject.AddComponent<SpriteRenderer>();
    }
    public void SetSprite(Sprite sprite) => _spriteRenderer.sprite = sprite;
    public void Clear()
    {
        _spriteRenderer.color = Color.white;
        SetSprite(null);
    }
}
