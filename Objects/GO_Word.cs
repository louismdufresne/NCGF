using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] GameObject - Word
//[][] Represents a rewritable word object that can display basic text

//[][] NOTES:
//[][] "Words" can contain numbers, spaces, and punctuation.
//[][] This object can be used to represent entire single lines of text.
public class GO_Word : MonoBehaviour
{
    public bool                         _doOutline = false;

    // Lists
    private List<GO_Visual>             _letterObjects;
    private List<GO_Visual>             _letterOutlines;
    // Word
    private string                      _wordText = "";
    // Keeping
    private NCGF_UI_RG_LetterSprites    _letterSprites;
    private NCGF_Pools                  _pools;

    private float _wordLength = 0;
    private bool _isSetUp = false;

    // Parameters
    private static float _letterExtraKerning = 0.0f;    // Space between letters in addition do sprite width

    public void Setup()
    {
        if (_isSetUp) return;

        _letterSprites  = NCGF_Res._letterSprites;
        _pools          = NCGF_Res._pools;

        if (_letterObjects == null) _letterObjects = new List<GO_Visual>();
        if (_letterOutlines == null) _letterOutlines = new List<GO_Visual>();

        _isSetUp = true;
    }
    public float GetWorldUnitLength()   => _wordLength;
    public string GetWord()             => _wordText;
    public List<GO_Visual>[] GetVisuals()
    {
        var retVal  = new List<GO_Visual>[2];
        retVal[0]   = new List<GO_Visual>(_letterObjects);
        retVal[1]   = new List<GO_Visual>(_letterOutlines);
        return retVal;
    }
    public void WriteFromEditor(string word, NCGF_UI_RG_LetterSprites letterSprites, NCGF_Pools pools)
    {
        Setup();

        _letterSprites = letterSprites;
        _pools = pools;

        WriteWord(word);
    }
    public void WriteWord(string word)
    {
        Setup();

        if (_letterSprites == null || _pools == null) return;

        _wordText = word;
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.one;

        // Unwrite
        for (int i = 0; i < _letterObjects.Count; i++)
        {
            _pools.Pool(_letterObjects[i]);
        }
        _letterObjects.Clear();
        for (int i = 0; i < _letterOutlines.Count; i++)
        {
            _pools.Pool(_letterOutlines[i]);
        }
        _letterOutlines.Clear();

        // Write
        float totalWidth = 0;
        Sprite curSprite;
        GO_Visual curVisual;
        foreach (var x in _wordText)
        {
            curSprite = _letterSprites.SpriteOfChar(x);
            curVisual = (GO_Visual)NCGF_Operations.SetAndReturnCleanedChild(this, (MonoBehaviour)_pools.Obtain(typeof(GO_Visual), false));
            curVisual.transform.localScale = transform.localScale;
            curVisual.SetSprite(curSprite);
            curVisual._spriteRenderer.color = Color.white;
            curVisual.name = $"Letter {x}";
            curVisual.transform.localPosition += new Vector3(totalWidth + (curSprite.bounds.size.x * 0.5f), 0, 0);

            _letterObjects.Add(curVisual);
            totalWidth += (curSprite.bounds.size.x + _letterExtraKerning);
        }
        totalWidth -= _letterExtraKerning;      // This is because the previous loop adds this per letter, not per space between

        float outlineInUnits = _letterObjects.Count == 0 ? 0 :
            NCGF_UI_R_Parameters._textOutlineInPixels
            * _letterObjects[0]._spriteRenderer.sprite.bounds.size.x
            / _letterObjects[0]._spriteRenderer.sprite.rect.width;

        Vector3 _centerJustify = new Vector3(-(totalWidth / 2), 0, 0);
        int[,] poses = NCGF_UI_R_Parameters._textOutlinePositionsInPx;
        for (int i = 0; i < _letterObjects.Count; i++)
        {
            _letterObjects[i].transform.localPosition += _centerJustify;
            if (_doOutline) for (int j = 0; j < 8; j++)
                {
                    curVisual = (GO_Visual)NCGF_Operations.SetAndReturnCleanedChild(
                        _letterObjects[i], (MonoBehaviour)_pools.Obtain(typeof(GO_Visual), false));
                    curVisual._spriteRenderer.sprite = _letterObjects[i]._spriteRenderer.sprite;
                    curVisual.name = _letterObjects[i].name + $" Outline Pt. {j}";
                    curVisual.transform.localPosition = new Vector3
                        (outlineInUnits * poses[j, 0],
                        outlineInUnits * poses[j, 1],
                        NCGF_UI_R_Parameters._textOutlineZOffset);
                    
                    curVisual._spriteRenderer.color = Color.black;
                    _letterOutlines.Add(curVisual);
                }
        }
        transform.localScale = originalScale;

        _wordLength = totalWidth;
    }
}
