using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCGF_DIA_GO_DiaText : MonoBehaviour
{
    [SerializeField] private NCGF_Pools                 _pools;
    [SerializeField] private NCGF_UI_RG_LetterSprites   _sprites;
    [SerializeField] private bool                       _doSlowLetterReveal = false;
    [SerializeField] private float                      _letterRevealPeriod = 0;
    [SerializeField] private GameObject                 _letterParent;

    // Setable Parameters
    [SerializeField] private float  _lMult = 1;
    [SerializeField] private bool   _ctrVert = false;
    [SerializeField] private bool   _ctrHoriz = false;
    [SerializeField] private float  _extraYSpacePerLine = 0;
    [SerializeField] private float  _extraXSpacePerLetter = 0;
    [SerializeField] private float  _waveHeight = 1;
    [SerializeField] private float  _waveSpeed = 1;
    [SerializeField] private float  _waveLetterFreq = 1;
    [SerializeField] private float  _shakeXAmp = 1;
    [SerializeField] private float  _shakeYAmp = 1;
    [SerializeField] private float  _shakePeriod = 1f;
    [SerializeField] private float  _vibrateXAmp = 1;
    [SerializeField] private float  _vibrateYAmp = 1;

    // Parameters
    private static readonly Vector3 r_italicsRotation = new Vector3(-30f, -30f, 0f);
    private static readonly Vector3 r_italicsScale = new Vector3(1.155f, 1.155f, 1f);
    private static readonly float   r_clockTrip = 205856;       // Appx. multiple of 2*pi

    // Keeping
    private List<DIA_O_DiaLetter> _letters          = new List<DIA_O_DiaLetter>();
    private Bounds      _textBounds;
    private Vector2     _ulc;
    private Vector2     _firstLetterLoc;
    private float       _perLetterX, _perLetterY;
    private int         _columns, _approprateRows;
    private bool        _isSetUp = false;
    private float       _revealClock = 0;
    private int         _allTextLength = 0;
    private float       _thScY, _thScX, _thScXInv;
    private float       _vertCenterAdj = 0;
    private float       _boundsMarginDispersion;
    private bool        _writing = false;

    // Letter Formatting
    private List<DIA_O_DiaLetter> _lettersItalics   = new List<DIA_O_DiaLetter>();
    private List<DIA_O_DiaLetter> _lettersColored   = new List<DIA_O_DiaLetter>();
    private List<DIA_O_DiaLetter> _lettersWaved     = new List<DIA_O_DiaLetter>();
    private List<DIA_O_DiaLetter> _lettersShaken    = new List<DIA_O_DiaLetter>();
    private List<DIA_O_DiaLetter> _lettersVibrate   = new List<DIA_O_DiaLetter>();

    //[][] Auto Functions
    private void Update()
    {
        UpdateReveal();
        PerformActiveFormatting();
    }

    //[][] Public Functions
    public void Setup(Bounds b)
    {
        if (_pools == null)     _pools = NCGF_Res._pools;
        if (_sprites == null)   _sprites = NCGF_Res._letterSprites;
        if (_pools == null ||   _sprites == null) return;

        CalcScaleParams();

        _textBounds = b;
        _ulc = (Vector2)_textBounds.center + new Vector2(-_textBounds.extents.x, _textBounds.extents.y);

        var exSprite = _sprites.SpriteOfChar(' ');
        var sprBS = exSprite.bounds.size;
        _perLetterX = ((sprBS.x * _lMult) + _extraXSpacePerLetter) * _thScX;
        _perLetterY = ((sprBS.y * _lMult) + _extraYSpacePerLine) * _thScY;
        _columns = (int)(_textBounds.size.x / _perLetterX);
        _approprateRows = (int)(_textBounds.size.y / _perLetterY);
        _boundsMarginDispersion = ((_textBounds.size.x % _perLetterX) + _extraXSpacePerLetter) * 0.5f;

        var sprBE = exSprite.bounds.extents;
        _firstLetterLoc = new Vector2(
            _ulc.x + (sprBE.x * _lMult) - transform.position.x,
            (_ctrVert) ? 0 : _ulc.y - (sprBE.y * _lMult) - transform.position.y);

        _isSetUp = true;
    }
    public List<DIA_O_Line> WriteAndReport(DIA_O_Line line)
    {
        if (!_isSetUp) { Debug.Log("DIA_GO_DT_WARE: Not set up!"); return null; }

        var theoreticalFits = GenerateFits(line);
        if (theoreticalFits.Count > _approprateRows)
        {
            var backupLine = new DIA_O_Line(line._characterIndex, line._emotion, line._subLines);
            var appropriateFits = theoreticalFits.GetRange(0, _approprateRows);
            theoreticalFits.RemoveRange(0, _approprateRows);
            var allLines = DivideLineAsAppropriate(line, appropriateFits);
            if (allLines.Count == 2)
            {
                Write(allLines[0], appropriateFits);
                return allLines;
            }
            else if (allLines.Count == 1)
            {
                Debug.Log("What!");
                Write(allLines[0], null);
            }
            else
            {
                Write(backupLine, null);
            }
            return null;
        }
        else
        {
            Write(line, theoreticalFits);
        }
        return null;
    }
    public void WritePublic(DIA_O_Line line) => Write(line, null);
    public void ForceRevealPublic()
    {
        ForceAllReveal();
    }

    //[][] Private Functions
    private void Write(DIA_O_Line line, List<int> fitsIfAvailable)
    {
        if (!_isSetUp) return;
        ClearLetters();
        _writing = true;

        var fits = fitsIfAvailable;
        if (fits == null) fits = GenerateFits(line);
        if (fits == null) { Debug.Log("DIA_GO_DT_W: Failed fits!"); return; }
        if (fits.Count == 0) fits = GenerateFits(line);

        _vertCenterAdj = (_ctrVert) ? (fits.Count - 1) * -0.5f : 0;

        DIA_O_DiaLetter curLetter;
        int curSubLine = 0;
        int curSubLChar = 0;
        float zDif = NCGF_UI_R_Parameters._textZOffset - NCGF_UI_R_Parameters._dialogueBoxZOffset;
        var subLines = line._subLines;
        string curText = subLines[0]._text;
        List<NCGF_Types.FormatType> formatList = subLines[curSubLine]._formats;
        char curChar;
        int rowL;
        int prevRowsSum = 0;
        int allSum = 0;
        int slip = 0;
        int prevSlip;
        Vector3 shiftOnSpace = new Vector3(0.5f * _perLetterX, 0, 0);

        for (int i = 0; i < fits.Count; i++)
        {
            rowL = fits[i];
            if (i != 0) prevRowsSum += fits[i - 1];

            prevSlip = slip;

            for (int j = 0; j < fits[i]; j++)
            {
                if (curSubLChar >= curText.Length)
                {
                    curSubLine++;
                    curText = subLines[curSubLine]._text;
                    formatList = subLines[curSubLine]._formats;
                    curSubLChar = 0;
                }

                curChar = subLines[curSubLine]._text[curSubLChar];

                if (curChar == '\n')
                {
                    slip--;
                    goto Write_SkipMakeLetterObject;
                }

                curLetter = GetLetter();
                curLetter._letter.transform.localPosition = new Vector3(
                    _firstLetterLoc.x
                        + ((j + (slip - prevSlip) + ((_ctrHoriz) ? (_columns - rowL + 1) * 0.5f : 0)) * _perLetterX + ((_ctrHoriz) ? _boundsMarginDispersion : 0)),
                    _firstLetterLoc.y
                        - ((i + _vertCenterAdj) * _perLetterY),
                    zDif);
                curLetter._anchorPtLoc = curLetter._letter.transform.localPosition;
                curLetter._letter.SetSprite(_sprites.SpriteOfChar(curChar));
                curLetter._letter.name = "Letter " + curChar;

                if (formatList != null)
                {
                    foreach (var x in formatList)
                    {
                        switch (x)
                        {
                            case NCGF_Types.FormatType.Italics:
                                _lettersItalics.Add(curLetter);
                                break;
                            case NCGF_Types.FormatType.Color:
                                _lettersColored.Add(curLetter);
                                curLetter._letter._spriteRenderer.color = subLines[curSubLine]._color;
                                break;
                            case NCGF_Types.FormatType.Wave:
                                _lettersWaved.Add(curLetter);
                                break;
                            case NCGF_Types.FormatType.Shake:
                                _lettersShaken.Add(curLetter);
                                break;
                            case NCGF_Types.FormatType.Vibrate:
                                _lettersVibrate.Add(curLetter);
                                break;
                            default:
                                break;
                        }
                    }
                }

            Write_SkipMakeLetterObject:

                if (curChar != ' ' && j == (fits[i] - 1) && _ctrHoriz)
                {
                    for (int q = prevRowsSum + prevSlip; q <= allSum + slip; q++)
                    {
                        _letters[q]._letter.transform.localPosition -= shiftOnSpace;
                        _letters[q].Set(_letters[q]._letter);
                    }
                }

                allSum++;
                curSubLChar++;
            }
        }

        PerformStaticFormatting();

        _allTextLength = CountLineCharLength(line);
        for (int i = 0; i < _letters.Count; i++) _letters[i]._letter.gameObject.SetActive(!_doSlowLetterReveal);
    }

    float UR_delT = 0;
    int UR_curLetter = 0;
    private void UpdateReveal()
    {
        if (_allTextLength == 0) return;
        if (!_doSlowLetterReveal) return;
        if (UR_curLetter >= _allTextLength) { _writing = false; return; }

        UR_delT = Time.deltaTime;
        _revealClock += UR_delT;

        if (_revealClock >= _letterRevealPeriod)
        {
            _revealClock -= ((UR_delT > _letterRevealPeriod) ? UR_delT : _letterRevealPeriod);
            _letters[UR_curLetter]._letter.gameObject.SetActive(true);
            UR_curLetter++;
        }
    }
    private void ForceAllReveal()
    {
        if (!_isSetUp) return;
        if (!_writing) return;

        UR_curLetter = _allTextLength;
        _revealClock = 0;
        foreach (var x in _letters) x._letter.gameObject.SetActive(true);

        _writing = false;
    }
    private void ClearLetters()
    {
        int origCount = _letters.Count;
        for (int i = 1; i <= origCount; i++) _pools.Pool(PoolDiaLetter(_letters[origCount - i]));

        _letters.Clear();
        _lettersItalics.Clear();
        _lettersColored.Clear();
        _lettersWaved.Clear();
        _lettersShaken.Clear();
        _lettersVibrate.Clear();

        UR_curLetter = 0;
        PAF_activeFormatClock = 0;
    }
    private DIA_O_DiaLetter GetLetter()
    {
        var vis = _pools.Obtain(typeof(GO_Visual), false) as GO_Visual;
        vis.transform.parent = _letterParent.transform;
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale = Vector3.one * _lMult;
        var retVal = GetDiaLetter(vis);
        _letters.Add(retVal);
        return retVal;
    }
    private int CountLineCharLength(DIA_O_Line line)
    {
        int retVal = 0;
        string curText;

        for (int i = 0; i < line._subLines.Count; i++)
        {
            curText = line._subLines[i]._text;
            retVal += curText.Length;
            for (int j = 0; j < curText.Length; j++) if (curText[j] == '\n') retVal--;
        }
        return retVal;
    }
    private void PerformStaticFormatting()
    {
        GO_Visual curVis;
        Transform curTrans;

        // Italics
        foreach (var x in _lettersItalics)
        {
            curVis = x._letter;
            curTrans = curVis.transform;
            curTrans.localEulerAngles = r_italicsRotation;
            curTrans.localScale = r_italicsScale * _lMult;
        }
    }
    private List<int> GenerateFits(DIA_O_Line line)    // Essentially generates line breaks
    {
        string allText = "";
        for (int i = 0; i < line._subLines.Count; i++) allText += line._subLines[i]._text;
        int len = allText.Length;
        if (len == 0) return null;

        List<int> retVal = new List<int>();
        int rowStart = 0;
        int prevSpace = 0;
        for (int i = 0; i < len; i++)
        {
            if (i == len - 1) { retVal.Add(i - rowStart + 1); break; }
            else if (allText[i] == ' ')
            {
                prevSpace = i;
            }

            if (i - rowStart == _columns)
            {
                if (prevSpace == rowStart)
                {
                    retVal.Add(_columns);
                    rowStart = prevSpace = i;
                }
                else
                {
                    retVal.Add(prevSpace - rowStart + 1);
                    rowStart = prevSpace + 1;
                    i++;
                }
            }
            else if (allText[i] == '\n')
            {
                retVal.Add(i - rowStart);
                rowStart = prevSpace = i;
            }
        }

        // End
        if (retVal.Count == 0) return null;

        // Check work
        int sum = 0;
        for (int i = 0; i < retVal.Count; i++)
        {
            sum += retVal[i];
        }

        if (sum != allText.Length)
        {
            Debug.Log($"DIA_GO_DT_GF: Dialogue improperly divided between lines!\nLine of length {allText.Length} divided across {DT_LTS(retVal)}; sum {sum}!");
            return null;
        }

        return retVal;
    }
    private List<DIA_O_Line> DivideLineAsAppropriate(DIA_O_Line line, List<int> appropriateLineFits)
    {
        // Returns a list containing two lines: a new "fitted" line, and the remaining portion of the old inputted line
        // If one or the other consumed all the sublines, only that one is returned and the list count is 1. (Probably)

        var retVal = new List<DIA_O_Line>();
        DIA_O_Line appropriateLine = new DIA_O_Line(line._characterIndex, line._emotion, new List<DIA_O_SubLine>());

        int count = 0;
        foreach (var x in appropriateLineFits) { count += x; }
        if (count == 0) Debug.Log("Swistsths!@");

        int oldSubLineCount = line._subLines.Count;
        int curLength;
        DIA_O_SubLine curSubLine;
        for (int i = 0; i < oldSubLineCount; i++)
        {
            curSubLine = line._subLines[0];
            curLength = curSubLine._text.Length;

            if (curLength <= count)
            {
                count -= curLength;
                line._subLines.RemoveAt(0);
                appropriateLine._subLines.Add(curSubLine);
                if (count == 0) break;
            }
            else
            {
                var splitSubLine = SplitSubLine(curSubLine, count);
                if (splitSubLine == null)
                {
                    Debug.Log("DIA_GO_DT_DLAA: This mathematically cannot happen, so who cares!");
                    break;
                }
                line._subLines.RemoveAt(0);
                appropriateLine._subLines.Add(splitSubLine[0]);
                line._subLines.Insert(0, splitSubLine[1]);
                count = 0;
                break;
            }
        }

        if (count != 0) Debug.Log("DIA_GO_DT_DLAA: appropriateLineFits used are oversized for line!");

        if (appropriateLine._subLines.Count != 0) retVal.Add(appropriateLine);
        if (line._subLines.Count != 0) retVal.Add(line);

        return retVal;
    }
    private List<DIA_O_SubLine> SplitSubLine(DIA_O_SubLine subLine, int charsIn)
    {
        // The original subLine is left alone, as it may be required later in a backup line.

        if (charsIn > subLine._text.Length) return null;
        var retVal = new List<DIA_O_SubLine>();

        DIA_O_SubLine newSubLine1 = new DIA_O_SubLine("", subLine._formats);
        DIA_O_SubLine newSubLine2 = new DIA_O_SubLine("", subLine._formats);

        newSubLine1._color = subLine._color;
        newSubLine2._color = subLine._color;

        newSubLine1._text = subLine._text.Substring(0, charsIn);
        newSubLine2._text = subLine._text.Substring(charsIn);

        retVal.Add(newSubLine1);
        retVal.Add(newSubLine2);
        return retVal;
    }
    private string DT_LTS(List<int> l) { var retVal = ""; for (int i = 0; i < l.Count; i++) retVal += (((i == 0) ? "" : ", ") + l[i].ToString()); return retVal; }

    // Active Formatting
    private float PAF_activeFormatClock = 0;
    private void PerformActiveFormatting()
    {
        if (!_isSetUp) return;
        if (_allTextLength == 0) return;

        PAF_activeFormatClock += Time.deltaTime;
        if (PAF_activeFormatClock > r_clockTrip) PAF_activeFormatClock -= r_clockTrip;

        foreach (var x in _letters) x._framePtLoc = x._anchorPtLoc;

        PerformWaveFormatting(PAF_activeFormatClock);
        PerformShakeFormatting();
        PerformVibrateFormatting();

        foreach (var x in _letters) x._letter.transform.localPosition = x._framePtLoc + x._shakeOffset;
    }
    private void PerformWaveFormatting(float clock)
    {
        foreach (var x in _lettersWaved)
        {
            if (x._letter.gameObject.activeSelf)
            {
                x._framePtLoc += new Vector3(
                    0,
                    _waveHeight * _thScY * Mathf.Sin(clock * _waveSpeed - (x._anchorPtLoc.x * _waveLetterFreq * _thScXInv)),
                    0);
            }
        }
    }
    private float PSF_clock = 0f;
    private void PerformShakeFormatting()
    {
        PSF_clock += Time.deltaTime;
        if (PSF_clock < _shakePeriod) return;
        PSF_clock -= _shakePeriod;

        foreach (var x in _lettersShaken)
        {
            if (x._letter.gameObject.activeSelf)
            {
                x._shakeOffset = new Vector3(
                    _shakeXAmp * _thScX * ((Random.value > 0.5f ? -1 : 1) * Random.Range(0.4f, 1f)),
                    _shakeYAmp * _thScY * ((Random.value > 0.5f ? -1 : 1) * Random.Range(0.4f, 1f)),
                    0);
            }
        }
    }
    private void PerformVibrateFormatting()
    {
        foreach (var x in _lettersVibrate)
        {
            if (x._letter.gameObject.activeSelf)
            {
                x._framePtLoc += new Vector3(
                    _vibrateXAmp * _thScX * ((Random.value > 0.5f ? -1 : 1) * Random.Range(0.4f, 1f)),
                    _vibrateYAmp * _thScY * ((Random.value > 0.5f ? -1 : 1) * Random.Range(0.4f, 1f)),
                    0);
            }
        }
    }

    // DIA_DO_DiaLetter pool
    private List<DIA_O_DiaLetter> _dLP = new List<DIA_O_DiaLetter>();
    private DIA_O_DiaLetter GetDiaLetter(GO_Visual letter)
    {
        DIA_O_DiaLetter retVal;
        if (_dLP.Count == 0) retVal = new DIA_O_DiaLetter();
        else
        {
            int pos = _dLP.Count - 1;
            retVal = _dLP[pos];
            _dLP.RemoveAt(pos);
        }
        retVal.Set(letter);
        return retVal;
    }
    private GO_Visual PoolDiaLetter(DIA_O_DiaLetter toPool)
    {
        var retVal = toPool._letter;
        toPool.Sweep();
        _dLP.Add(toPool);
        return retVal;
    }

    // Misc
    private void CalcScaleParams()
    {
        _thScX = transform.lossyScale.x;
        _thScY = transform.lossyScale.y;
        _thScXInv = 1f / _thScX;
    }
    private void Explicate(DIA_O_Line line, List<int> fitsIfAvailable)
    {
        string toPrint = "Explication!\n\nLine contents:\n";

        if (line._subLines == null) { toPrint += "SubLines null!"; goto E_Print; }

        for (int i = 0; i < line._subLines.Count; i++)
        {
            if (line._subLines[i] != null)
            {
                toPrint += (i + ": [" + line._subLines[i]._text + "] of count " + line._subLines[i]._text.Length + '\n');
                if (line._subLines[i]._formats != null)
                {
                    toPrint += ("Formats: ");
                    for (int j = 0; j < line._subLines[i]._formats.Count; j++)
                    {
                        toPrint += line._subLines[i]._formats[j].ToString() + " ";
                    }
                    toPrint += '\n';
                }
                else toPrint += ("Formats: NULL!\n");
            }
            else toPrint += (i + ": NULL!\n");
        }
        if (fitsIfAvailable != null)
        {
            toPrint += "\nFits: ";
            for (int i = 0; i < fitsIfAvailable.Count; i++)
            {
                toPrint += fitsIfAvailable[i] + " ";
            }
            toPrint += '\n';
        }

    E_Print:
        Debug.Log(toPrint);
    }
}
