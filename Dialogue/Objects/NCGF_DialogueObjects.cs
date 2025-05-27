using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Dialogue Objects
//[][] Script containing classes used in dialogue operation

public class NCGF_DialogueObjects 
{
}

public class DIA_O_Line
{
    public int                  _characterIndex;
    public NCGF_Types.EmoteType _emotion;
    public List<DIA_O_SubLine>  _subLines;
    public DIA_O_Line(
        int characterIndex,
        NCGF_Types.EmoteType emotion,
        List<DIA_O_SubLine> subLines)
    {
        _characterIndex = characterIndex;
        _emotion = emotion;
        _subLines = subLines;
    }
}
public class DIA_O_SubLine
{
    public string _text;
    public List<NCGF_Types.FormatType> _formats;
    public Color32 _color;
    public DIA_O_SubLine(string text, List<NCGF_Types.FormatType> formats)
    {
        _text = text;
        _formats = formats;
        _color = NCGF_UI_R_Parameters._defaultTextColor;
    }
}

[System.Serializable]
public class DIA_O_CharInfoItem
{
    public ushort                   _charID;
    public List<DIA_O_CharEmotes>   _emoteAnimations;
    [HideInInspector] public Dictionary<NCGF_Types.EmoteType, DIA_O_CharEmotes> _emoteDict;

    public void CreateDictionary()
    {
        if (_emoteAnimations == null) return;
        if (_emoteDict != null) _emoteDict.Clear();
        _emoteDict = new Dictionary<NCGF_Types.EmoteType, DIA_O_CharEmotes>();
        foreach (var x in _emoteAnimations)
        {
            if (!_emoteDict.ContainsKey(x._emotion)) _emoteDict.Add(x._emotion, x);
        }
    }
}

[System.Serializable]
public class DIA_O_CharEmotes
{
    public NCGF_Types.EmoteType     _emotion;
    public List<Sprite>             _animationFrames;
}

public class DIA_O_DiaLetter
{
    public Vector3      _anchorPtLoc;
    public Vector3      _framePtLoc;
    public Vector3      _shakeOffset;
    public GO_Visual    _letter;

    public void Set(GO_Visual letter)
    {
        _letter = letter;
        _anchorPtLoc = _framePtLoc = letter.transform.localPosition;
        _shakeOffset = Vector3.zero;
    }
    public void Sweep() { _anchorPtLoc = _framePtLoc = _shakeOffset = Vector3.zero; _letter = null; }
}

