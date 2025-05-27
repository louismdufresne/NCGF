using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Word Tool
//[][] Allows words to be written from the inspector

//[][] NOTES:
//[][] The prefab of UIB_GO_UIVisual must already contain a sprite editor and have its _spriteRenderer field set to it.

[ExecuteAlways]
public class NCGF_WordTool : MonoBehaviour
{
    [SerializeField] private NCGF_Pools                 _pools;
    [SerializeField] private NCGF_UI_RG_LetterSprites   _letterSprites;
    [SerializeField] private string                     _wordToWrite = "";
    [SerializeField] private GO_Word                    _wordObjectReference;
    [SerializeField] private bool                       _setToWrite = false;
    [SerializeField] private bool                       _hardLock   = true;

    private void Update()
    {
        if (_hardLock) return;
        if (!_setToWrite) return;
        _setToWrite = false;
        if (_wordObjectReference == null)   { Debug.Log("NO WORD OBJECT REFERENCE SET"); return; }
        if (_pools == null)                 { Debug.Log("NO UI POOLS REFERENCE SET"); return; }
        if (_letterSprites == null)         { Debug.Log("NO LETTER SPRITES REFERENCE SET"); return; }
        _wordObjectReference.WriteFromEditor(_wordToWrite, _letterSprites, _pools);
        gameObject.SetActive(false);
    }
}
