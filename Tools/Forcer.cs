using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Forcer : MonoBehaviour
{
    [SerializeField] private bool _go = false;
    [SerializeField] private NCGF_UI_RG_LetterSprites _letterSprites;

    private void Update()
    {
        if (!_go) return;
        _go = false;
        if (_letterSprites == null) return;
        _letterSprites.Convert();
    }
}
