using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Character Info
//[][] Contains relevant information for all characters for use in dialogues
public class NCGF_DIA_RG_CharacterInfo : MonoBehaviour
{
    [SerializeField] private bool                       _charsUsePresetIDs  = false;
    [SerializeField] private List<DIA_O_CharInfoItem>   _charInfos          = new List<DIA_O_CharInfoItem>();

    // Keeping
    private Dictionary<ushort, DIA_O_CharInfoItem> _charDict
        = new Dictionary<ushort, DIA_O_CharInfoItem>();
    private ushort _curID = 0;

    private bool _isSetUp = false;

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }

    //[][] Public Functions
    public void Setup()
    {
        if (_isSetUp) return;

        foreach (var x in _charInfos)
        {
            // Either reset current internal ID, or reset all character IDs
            if (_charsUsePresetIDs) { if (x._charID >= _curID) _curID = (ushort)(x._charID + 1); }
            else x._charID = _curID++;

            if (!_charDict.ContainsKey(x._charID)) _charDict.Add(x._charID, x);
            x.CreateDictionary();
        }

        _isSetUp = true;
    }
    public bool AddCharacter(List<DIA_O_CharEmotes> emotes, out ushort ID)
    {
        Setup();

        if (emotes == null)     goto AC_Fail;
        if (emotes.Count == 0)  goto AC_Fail;

        var newChar = new DIA_O_CharInfoItem
        {
            _charID = _curID++,
            _emoteAnimations = emotes
        };
        newChar.CreateDictionary();

        ID = newChar._charID;
        return true;

    AC_Fail:
        ID = ushort.MaxValue;
        return false;
    }
    public List<Sprite> GetCharSprites(ushort charID, NCGF_Types.EmoteType emotion)
    {
        Setup();

        var charEm = GetCharEmote(charID, emotion);
        return (charEm != null) ? charEm._animationFrames : null;
    }
    public DIA_O_CharEmotes GetCharEmote(ushort charID, NCGF_Types.EmoteType emotion)
    {
        Setup();

        var cur = GetCharInfo(charID);
        if (cur == null) return null;

        var dict = cur._emoteDict;

        if (dict != null)
        {
            if (System.Enum.IsDefined(typeof(NCGF_Types.EmoteType), emotion))
                { if (dict.ContainsKey(emotion))                                return dict[emotion]; }
            if (dict.ContainsKey(NCGF_Types.EmoteType.Neutral))                 return dict[NCGF_Types.EmoteType.Neutral];
            if (dict.ContainsKey(NCGF_Types.EmoteType.None))                    return dict[NCGF_Types.EmoteType.None];
            if (cur._emoteAnimations.Count != 0)                                return cur._emoteAnimations[0];
        }

        return null;
    }
    public DIA_O_CharInfoItem GetCharInfo(ushort charID)
    {
        Setup();

        if (_charDict.ContainsKey(charID)) return _charDict[charID];
        return null;
    }

    //[][] Private Functions
}
