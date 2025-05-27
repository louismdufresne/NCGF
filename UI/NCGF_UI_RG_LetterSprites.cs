using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Resource GameObject - UI Letter Sprites
//[][] Contains all sprites associated with the printing of letters in words

//[][] Uppercase and lowercase letter sprites should be added in alphabetical order, exactly 26 per list
//[][] Numbers are in numerical order from 0 to 9
//[][] Punctuation occurs in this order: (notfound)(space)!?,.'"()[]{};:+-*/\=_

public class NCGF_UI_RG_LetterSprites : MonoBehaviour
{
    [SerializeField] private List<NCFG_Struct_LetterListItem> _letters;

    [SerializeField] private List<Sprite> _uppercaseLetters;
    [SerializeField] private List<Sprite> _lowercaseLetters;
    [SerializeField] private List<Sprite> _numbers;
    [SerializeField] private List<Sprite> _punctuation;

    private Dictionary<char, int> _letterIndices = new Dictionary<char, int>();

    // Punctuation Dictionary
    public readonly Dictionary<char, int> _puncCharSpriteIndices = new Dictionary<char, int>
    {
        { '\uFFFD', 0 },
        { ' ',      1 },
        { '!',      2 },
        { '?',      3 },
        { ',',      4 },
        { '.',      5 },
        { '\'',     6 },
        { '’',      6 },
        { '‘',      6 },
        { '\"',     7 },
        { '”',      7 },
        { '“',      7 },
        { '(',      8 },
        { ')',      9 },
        { '[',     10 },
        { ']',     11 },
        { '{',     12 },
        { '}',     13 },
        { ';',     14 },
        { ':',     15 },
        { '+',     16 },
        { '-',     17 },
        { '*',     18 },
        { '/',     19 },
        { '\\',    20 },
        { '=',     21 },
        { '_',     22 },
        { '@',     23 },
        { '#',     24 },
        { '$',     25 },
        { '%',     26 },
        { '^',     27 },
        { '&',     28 },
        { '\u2190', 29 },   // Left Arrow
        { '\u21E6', 29 },   // Left Thick Arrow
        { '\u2192', 30 },   // Right Arrow
        { '\u21E8', 30 },   // Right Thick Arrow
        { '<',     31 },
        { '>',     32 },
        { '|',     33 },
    };

    private bool _isSetUp = false;

    private void Awake()
    {
        Setup();
    }
    public Sprite SpriteOfChar(char c)
    {
        Setup();

        // Best option
        if (_letterIndices.ContainsKey(c)) return _letters[_letterIndices[c]]._sprite;
        if (_letterIndices.ContainsKey('\uFFFD')) return _letters[_letterIndices['\uFFFD']]._sprite;

        // Reserve option
        Debug.Log("NCGF_UI_RG_LetterSprites.SpriteOfChar: Dictionary did not contain key!");
        for (int i = 0; i < _letters.Count; i++) if (_letters[i]._letter == c) return _letters[i]._sprite;

        // Backup
        if (c >= 65 && c <= 90) return _uppercaseLetters[c - 65];
        if (c >= 97 && c <= 122) return _lowercaseLetters[c - 97];
        if (c >= 48 && c <= 57) return _numbers[c - 48];
        if (_puncCharSpriteIndices.ContainsKey(c)) return _punctuation[_puncCharSpriteIndices[c]];
        return _punctuation[0];
    }
    public void Convert()
    {
        // For the purposes of not nuking my own list by accident, this entire function is commented out.

        //_letters.Clear();
        //string lc = "abcdefghijklmnopqrstuvwxyz";
        //string uc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //string p = '\uFFFD' + " !?,.'\"()[]{};:+-*/\\=_@#$%^&" + '\u2190' + '\u21E6' + '\u2192' + '\u21E8' + "<>|";
        //for (int i = 0; i < 10; i++) _letters.Add(new NCFG_Struct_LetterListItem(i.ToString()[0], _numbers[i]));
        //for (int i = 0; i < 26; i++) _letters.Add(new NCFG_Struct_LetterListItem(uc[i], SpriteOfChar(uc[i])));
        //for (int i = 0; i < 26; i++) _letters.Add(new NCFG_Struct_LetterListItem(lc[i], SpriteOfChar(lc[i])));
        //for (int i = 0; i < p.Length; i++)
        //    _letters.Add(new NCFG_Struct_LetterListItem(p[i], SpriteOfChar(p[i])));
    }
    private void Setup()
    {
        if (_isSetUp) return;
        _isSetUp = true;

        NCGF_Res._letterSprites = this;

        _letterIndices.Clear();
        for (int i = 0; i < _letters.Count; i++)
        {
            if (_letterIndices.ContainsKey(_letters[i]._letter)) continue;
            _letterIndices.Add(_letters[i]._letter, i);
        }
    }
}

[System.Serializable]
public struct NCFG_Struct_LetterListItem
{
    public char     _letter;
    public Sprite   _sprite;
    public NCFG_Struct_LetterListItem(char letter, Sprite sprite)
    {
        _letter = letter;
        _sprite = sprite;
    }
}