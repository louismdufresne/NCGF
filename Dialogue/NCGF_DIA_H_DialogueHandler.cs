using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Handler - Dialogue Handler
//[][] Handles event calls to print dialogues
public class NCGF_DIA_H_DialogueHandler : MonoBehaviour
{

    [SerializeField] private TextAsset  _allFormattedDialogue;
    [SerializeField] private char       _beginFormattingChar    = '\u2132'; // Turned Capital F
    [SerializeField] private char       _endFormattingChar      = '\u0250'; // Latin Small Letter Turned A
    [SerializeField] private char       _newLineChar            = '\u01B9'; // Latin Small Letter Ezh Reversed
    [SerializeField] private NCGF_DIA_GO_DialogueBox _dialogueBox;

    // Keeping
    private char _bf, _ef, _nl;
    private List<string>                _allLines = new List<string>();
    private List<List<DIA_O_Line>>     _allDialogues = new List<List<DIA_O_Line>>();

    //[][] On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.DE_PerformDialogue += HandlePerformDialogue;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.DE_PerformDialogue -= HandlePerformDialogue;
    }

    //[][] Auto Functions
    private void Awake()
    {
        _bf = _beginFormattingChar;
        _ef = _endFormattingChar;
        _nl = _newLineChar;

        ParseAllText();
    }


    //[][] Private Functions

    private void ParseAllText()
    {
        GenerateDialogueStrings();
        ParseDialogueStrings();
        GenerateReport();
    }
    private void GenerateDialogueStrings()
    {
        var all = _allFormattedDialogue.text;
        int pos1 = 0;
        int pos2 = 1;
        int allLen = all.Length;
        int allLenM = allLen - 1;
        bool isNewl;
        bool isEnd;

        for (; pos2 < allLen; pos2++)
        {
            isNewl =    (all[pos2] == '\n');
            isEnd =     (pos2 == allLenM);

            if (isNewl || isEnd)
            {
                if (pos1 < pos2) _allLines.Add(all.Substring(pos1, pos2 - pos1 + ((!isNewl) ? 1 : -1)));   // '-1' Scrapes off carriage return (last char)
                pos1 = pos2 + 1;
            }
        }
    }

    #region Parse
    private void ParseDialogueStrings()
    {
        float delTime = Time.realtimeSinceStartup;
        for (int i = 0; i < _allLines.Count; i++)
        {
            ParsePerDialogueString(_allLines[i]);
        }
        delTime = Time.realtimeSinceStartup - delTime;
        Debug.Log($"All dialogue parsed in {delTime} seconds.");
    }

    // PPDS Vars
    int                         _loc1, _loc2, _convNum, _allDCount, _temp, _msL, _nLPre, _nLPost;
    int                         _curChar;
    NCGF_Types.EmoteType        _curEmote;
    string                      _message;
    char                        _curC;
    Color32                     _curColor;
    DIA_O_Line                  _curLine;
    DIA_O_SubLine               _curSubLine;
    private void ParsePerDialogueString(string line)
    {
        _loc1 = line.IndexOf(' ');
        if (_loc1 < 0)                                                              { Debug.Log("DIA_H_DH_PPDS: Failed to locate first space in line!"); return; }
        if (!int.TryParse(line.Substring(0, _loc1), out _convNum))                  { Debug.Log("DIA_H_DH_PPDS: Failed TryParse at Conversation ID!"); return; }

        _allDCount = _allDialogues.Count;
        if (_convNum >= _allDCount) for (int i = _allDCount; i <= _convNum; i++) _allDialogues.Add(null);
        if (_allDialogues[_convNum] == null) _allDialogues[_convNum] = new List<DIA_O_Line>();

        _loc2 = line.IndexOf(' ', _loc1 + 1);
        if (_loc2 < 0)                                                              { Debug.Log("DIA_H_DH_PPDS: Failed to locate second space in line!"); return; }
        if (!int.TryParse(line.Substring(_loc1 + 1, _loc2 - _loc1), out _curChar))  { Debug.Log("DIA_H_DH_PPDS: Failed TryParse at Character Index!"); return; }

        //if (!System.Enum.IsDefined(typeof(CHA_A_Characters.CharID), _curChar))      { Debug.Log("DIA_H_DH_PPDS: Failed character ID cast!"); return; }

        _loc1 = line.IndexOf(' ', _loc2 + 1);
        if (_loc1 < 0)                                                              { Debug.Log("DIA_H_DH_PPDS: Failed to locate third space in line!"); return; }
        if (!int.TryParse(line.Substring(_loc2 + 1, _loc1 - _loc2), out _temp))     { Debug.Log("DIA_H_DH_PPDS: Failed TryParse at Emote ID!"); return; }
        _curEmote = (NCGF_Types.EmoteType)(byte)_temp;
        if (!System.Enum.IsDefined(typeof(NCGF_Types.EmoteType), _curEmote))        { Debug.Log("DIA_H_DH_PPDS: Failed emote ID cast!"); return; }

        _message = line.Substring(_loc1 + 1);
        _msL = _message.Length;
        if (_msL == 0)                                              { Debug.Log("DIA_H_DH_PPDS: Message is empty!"); return; }

        _curLine = new DIA_O_Line(_curChar, _curEmote, new List<DIA_O_SubLine>());
        _curSubLine = new DIA_O_SubLine("", null);

        _loc1 = _loc2 = _temp = 0;
        for (; _loc2 < _msL; _loc2++)
        {
            _curC = _message[_loc2];
            if (_curC == _bf)
            {
                if (_loc1 != _loc2)
                {
                    _curSubLine._text = _message.Substring(_loc1, _loc2 - _loc1);
                    _curLine._subLines.Add(_curSubLine);
                    _curSubLine = new DIA_O_SubLine("", null);
                }
                _curSubLine._formats = PPDS_GetFormats(_message, _loc2);
                if (_curSubLine._formats.Contains(NCGF_Types.FormatType.Color)) _curSubLine._color = _curColor;
                _loc2 = _message.IndexOf(' ', _loc2) + 1;                                               // Space within formatting is skipped
                _loc1 = _loc2;
            }
            else if (_curC == _ef)
            {
                if (_loc1 == _loc2) { _curSubLine._formats = null; _loc1++; continue; }                // Possible empty message between formatting chars
                _temp = (_message[_loc2 - 1] == ' ') ? -1 : 0;
                _curSubLine._text = _message.Substring(_loc1, _loc2 - _loc1 + _temp);
                _curLine._subLines.Add(_curSubLine);

                _loc1 = _loc2 + 1;
                if (_loc2 < _message.Length - 1) _curSubLine = new DIA_O_SubLine("", null);
            }
            else if (_curC == _nl)
            {
                if (_loc2 > 0) _nLPre = (_message[_loc2 - 1] == ' ') ? 1 : 0; else _nLPre = 0;
                if (_loc2 < _msL - 1) _nLPost = (_message[_loc2 + 1] == ' ') ? 2 : 1; else _nLPost = 1;

                _message = _message.Substring(0, _loc2 - _nLPre) + '\n' + ((_loc2 >= _msL - _nLPost) ? "" : _message.Substring(_loc2 + _nLPost));
                _msL = _message.Length;
                if (_nLPre != 0) _loc2--;

                PPDS_CheckNew();
            }
            else PPDS_CheckNew();
        }

        _allDialogues[_convNum].Add(_curLine);
    }
    private void PPDS_CheckNew()
    {
        if (_loc2 == _message.Length - 1)
        {
            _curSubLine._text = _message.Substring(_loc1, _loc2 - _loc1 + 1);
            _curLine._subLines.Add(_curSubLine);
        }
    }
    private List<NCGF_Types.FormatType> PPDS_GetFormats(string s, int startInd)
    {
        _curColor = NCGF_UI_R_Parameters._defaultTextColor;
        if (s[startInd] == _bf) startInd++;

        if (startInd >= s.Length) return null;
        var retVal = new List<NCGF_Types.FormatType>();

        for (; startInd < s.Length; startInd++)
        {
            if (s[startInd] == ' ') break;
            switch (s[startInd])
            {
                case 'I':
                case 'i':
                    if (!retVal.Contains(NCGF_Types.FormatType.Italics)) retVal.Add(NCGF_Types.FormatType.Italics);
                    break;
                case 'W':
                case 'w':
                    if (!retVal.Contains(NCGF_Types.FormatType.Wave)) retVal.Add(NCGF_Types.FormatType.Wave);
                    break;
                case 'S':
                case 's':
                    if (!retVal.Contains(NCGF_Types.FormatType.Shake)) retVal.Add(NCGF_Types.FormatType.Shake);
                    break;
                case 'C':
                case 'c':
                    if (!retVal.Contains(NCGF_Types.FormatType.Color)) retVal.Add(NCGF_Types.FormatType.Color);
                    else break;
                    PPDS_MakeColor(s.Substring(startInd + 1, 6));
                    startInd += 6;
                    break;
                case 'U':
                case 'u':
                    if (!retVal.Contains(NCGF_Types.FormatType.Underline)) retVal.Add(NCGF_Types.FormatType.Underline);
                    break;
                case 'B':
                case 'b':
                    if (!retVal.Contains(NCGF_Types.FormatType.Bold)) retVal.Add(NCGF_Types.FormatType.Bold);
                    break;
                case 'V':
                case 'v':
                    if (!retVal.Contains(NCGF_Types.FormatType.Vibrate)) retVal.Add(NCGF_Types.FormatType.Vibrate);
                    break;
                default:
                    Debug.Log("NCGF_DIA_H_DialogueHandler.PPDS_GetFormats: Unexpected character; formatting failed! Returning.");
                    return null;
            }
        }
        return (retVal.Count == 0) ? null : retVal;
    }
    private void PPDS_MakeColor(string sixDigHex)
    {
        if (sixDigHex.Length != 6) return;
        byte r, g, b;
        r = PPDS_BfromHex(sixDigHex.Substring(0, 2));
        g = PPDS_BfromHex(sixDigHex.Substring(2, 2));
        b = PPDS_BfromHex(sixDigHex.Substring(4, 2));
        _curColor = new Color32(r, g, b, 255);
    }
    private byte PPDS_BfromHex(string twoDigHex)
    {
        if (twoDigHex.Length != 2) return 0;
        return (byte)(16 * PPDS_HCharNumVal(twoDigHex[0]) + PPDS_HCharNumVal(twoDigHex[1]));
    }
    private int PPDS_HCharNumVal(char c) => c < 58 ? c - 48 : c - 55;
    #endregion

    private void GenerateReport()
    {
        string report = "";

        report += $"Number of conversations: {_allDialogues.Count}\n\n";
        for (int i = 0; i < _allDialogues.Count; i++)
        {
            if (_allDialogues[i] == null) continue;
            report += $"> Conversation {i} line count: {_allDialogues[i].Count}\n\n";
            for (int j = 0; j < _allDialogues[i].Count; j++)
            {
                report += $">> Line {j}\n";
                report += $">> Character speaking: {_allDialogues[i][j]._characterIndex}\n";
                report += $">> Character Emotion: {_allDialogues[i][j]._emotion}\n";

                for (int k = 0; k < _allDialogues[i][j]._subLines.Count; k++)
                {
                    report += $">>* Text: [{_allDialogues[i][j]._subLines[k]._text}]\n";
                    report += $">>* Formats:";
                    if (_allDialogues[i][j]._subLines[k]._formats == null) { report += $" NULL"; goto GR_NoFormats; }
                    if (_allDialogues[i][j]._subLines[k]._formats.Count == 0) { report += $" EMPTY"; goto GR_NoFormats; }

                    for (int q = 0; q < _allDialogues[i][j]._subLines[k]._formats.Count; q++)
                    {
                        report += $" {_allDialogues[i][j]._subLines[k]._formats[q]}";
                    }
                    if (_allDialogues[i][j]._subLines[k]._formats.Contains(NCGF_Types.FormatType.Color))
                        report += $"\nColor: {_allDialogues[i][j]._subLines[k]._color}";
                    GR_NoFormats:
                    report += '\n';
                }
                report += '\n';
            }
        }

        report += "Raw lines:\n";
        for (int i = 0; i < _allLines.Count; i++)
        {
            report += "[" + _allLines[i] + "]\n";
        }
        Debug.Log(report);
    }

    //[][] Event Handling
    private void HandlePerformDialogue(int dialogueID, List<ushort> participants)
    {
        if (dialogueID >= _allDialogues.Count)
        {
            Debug.Log("DIA_H_DH_HPD: Dialogue index exceeds number of dialogues available!");
        }

        var dialogue = _allDialogues[dialogueID];
        _dialogueBox.PerformDialogue(dialogue, participants);
    }
}
