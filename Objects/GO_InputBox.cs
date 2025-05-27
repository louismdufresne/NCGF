using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] GameObject - UI Input Box
//[][] Single instantiable object for the input of text and numbers; displays back what is typed or set

//[][] NOTES:
//[][] The width of the input box will be (if possible) set to the real width of its sprite (bounds.x * transform.localScale.x)
public class GO_InputBox : MonoBehaviour
{
    [SerializeField] private GO_Button      _buttonPrefab;
    [SerializeField] private GO_Word        _wordPrefab;

    [SerializeField] private bool   _noLetters      = false;
    [SerializeField] private bool   _noNumbers      = false;
    [SerializeField] private bool   _noPunc         = false;
    [SerializeField] private bool   _displayOnly    = false;
    [SerializeField] private float  _textScale      = 0.5f;
    [SerializeField] private float  _textPadding    = 0.1f;
    [SerializeField] private byte   _maxChars       = 16;

    [SerializeField] private bool   _isNumberField          = false;
    [SerializeField] private bool   _numberAllowsDecimal    = false;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite         _disarmedSprite;
    [SerializeField] private Sprite         _armedSprite;

    private GO_Word         _wordObject;
    private GO_Button       _buttonObject;
    private string          _text = "";
    private float           _boxHalfWidth = 0;
    private float           _boxHalfHeight = 0;
    private uint            _buttonID;

    private bool    _armed = false;
    private float   _xScalePrevF = 0;

    private bool    _isSetUp = false;

    // Parameters
    private static readonly float _defaultBoxWidth  = 1;
    private static readonly float _defaultBoxHeight = 1;

    // On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.AE_ButtonActivate   += HandleButtonClick;
        NCGF_UI_S_Events.KE_PrioritizedKeyPress     += HandleKey;
        NCGF_UI_S_Events.OE_AnyMouseButton   += HandleClick;
        NCGF_UI_S_Events.OE_RunFrame         += HandleEndUIFrame;
        NCGF_UI_S_Events.OE_InputString      += HandleInputString;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.AE_ButtonActivate   -= HandleButtonClick;
        NCGF_UI_S_Events.KE_PrioritizedKeyPress     -= HandleKey;
        NCGF_UI_S_Events.OE_AnyMouseButton   -= HandleClick;
        NCGF_UI_S_Events.OE_RunFrame         -= HandleEndUIFrame;
        NCGF_UI_S_Events.OE_InputString      -= HandleInputString;
    }

    // Auto Functions


    // Setup (called by parent UI panel)
    public uint Setup()     // ID (uses ID of button)
    {
        _buttonObject   = (GO_Button)NCGF_Operations.SetAndReturnCleanedChild(this, Instantiate(_buttonPrefab));
        _wordObject     = (GO_Word)NCGF_Operations.SetAndReturnCleanedChild(this, Instantiate(_wordPrefab));

        _buttonObject.transform.localPosition = new Vector3(0, 0, -NCGF_UI_R_Parameters._stackedElementMinZSeparation);
        _wordObject.transform.localPosition = new Vector3(0, 0, -2 * NCGF_UI_R_Parameters._stackedElementMinZSeparation);

        _spriteRenderer ??= GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = _disarmedSprite;

        _buttonID = _buttonObject.Setup();
        _buttonObject._activateOnClickUp = false;
        _buttonObject._spriteRenderer.color = new Color(1, 1, 1, 0);
        _buttonObject.FloodAsSprite(_spriteRenderer.sprite);
        RecalculateBoxSize();

        _wordObject.Setup();
        ResizeText();

        // Cleanup
        if (_isNumberField)
        {
            _noLetters  = true;
            _noNumbers  = false;
            _noPunc     = true;     // Period is extenuating (see IsPuncAllowed)
        }

        DirectWriteTo(_text);       // Seems silly, but re-finalizes the box appropriately
        _isSetUp = true;
        return _buttonID;
    }
    public void DirectWriteTo(string s)
    {
        _text = s;
        FinalizeWrite();
    }
    public void ForceResize()
    {
        _xScalePrevF = 0;
        CheckForResize();
    }

    // Event Handling
    private void HandleEndUIFrame()
    {
        if (!_isSetUp) return;

        CheckForResize();
    }

    private void HandleKey(byte priority, KeyCode k, List<uint> IDs)
    {
        if (priority != NCGF_UI_R_Parameters._inputBoxKeyPriority) return;
        if (!_armed || _displayOnly) return;

        if (k == KeyCode.Delete) DirectWriteTo("");
    }
    private void HandleInputString(string s)
    {
        if (!_armed || _displayOnly) return;
        foreach (char c in s)
        {
            switch (c)
            {
                case '\b':
                    if (_text.Length != 0) _text = _text.Substring(0, _text.Length - 1);
                    break;
                case '\n':
                case '\r':
                    Disarm();
                    return;
                default:
                    if (IsCharNumber(c))
                        { if (_noNumbers) continue; }
                    else if (NCGF_Res._letterSprites._puncCharSpriteIndices.ContainsKey(c))
                        { if (!IsPuncAllowed(c)) continue; }
                    else
                        { if (_noLetters) continue; }

                    AddCharacterAsAppropriate(c);
                    break;
            }
        }
        FinalizeWrite();
    }
    private void HandleClick(List<uint> IDs)
    {
        if (!IDs.Contains(_buttonID)) Disarm();
    }
    private void HandleButtonClick(uint ID)
    {
        if (ID != _buttonID) Disarm();
        else Arm();
    }

    // Operation
    private void Arm()
    {
        if (_displayOnly) return;
        if (_armed) return;

        NCGF_UI_S_Events.KE_Subscribe_Go(KeyCode.None, NCGF_UI_R_Parameters._inputBoxKeyPriority);
        _spriteRenderer.sprite = _armedSprite;
        _armed = true;
    }
    private void Disarm()
    {
        if (!_armed) return;

        NCGF_UI_S_Events.KE_Unsubscribe_Go(KeyCode.None, NCGF_UI_R_Parameters._inputBoxKeyPriority);
        _spriteRenderer.sprite = _disarmedSprite;
        _armed = false;
        NCGF_UI_S_Events.AE_InputBoxActivate_Go(_buttonID, _text);
    }
    private void ResizeText()
    {
        float space = 2 * (_boxHalfWidth - _textPadding);
        float wordSize = _textScale * transform.lossyScale.x * _wordObject.GetWorldUnitLength();
        float scale = (wordSize > space) ? (space / wordSize) * _textScale : _textScale;
        _wordObject.transform.localScale = new Vector3(scale, scale, 1);
    }
    private void RecalculateBoxSize()
    {
        _boxHalfWidth = (_spriteRenderer.sprite == null) ?
            _defaultBoxWidth
            : (_spriteRenderer.sprite.bounds.extents.x * this.transform.lossyScale.x);
        _boxHalfHeight = (_spriteRenderer.sprite == null) ?
            _defaultBoxHeight
            : (_spriteRenderer.sprite.bounds.extents.y * this.transform.lossyScale.y);

        // Old
        //_buttonObject.SetResizeToSprite(false);
        //_buttonObject.ManualResize(_boxHalfWidth, _boxHalfHeight);
    }
    private void CheckForResize()
    {
        var xSc = transform.lossyScale.x;
        if (_xScalePrevF == xSc) return;

        RecalculateBoxSize();
        ResizeText();
        _xScalePrevF = xSc;
    }
    private bool IsCharNumber(char c) => c >= 48 && c <= 57;
    private void FinalizeWrite()
    {
        SetToDefaultIfEmpty();
        _wordObject.WriteWord(_text);
        ResizeText();
    }
    private void SetToDefaultIfEmpty()
    {
        // Runs last-minute checks on the text
        if (DoesTextEqualZero()) ActOnTextEqualsZero();
    }
    private bool IsPuncAllowed(char c)
    {
        if (c == '.')
        {
            if (_isNumberField && _numberAllowsDecimal && !_text.Contains(".")) return true;
        }

        return !_noPunc;
    }
    private void AddCharacterAsAppropriate(char c)
    {
        if (DoesTextEqualZero()) { if (!_text.Contains(".")) _text = (c == '.') ? "0" : ""; }
        if (_text.Length >= _maxChars) return;
        _text += c;
    }
    private bool DoesTextEqualZero()
    {
        if (!_isNumberField) return false;
        if (_text.Length == 0) return true;
        if (_text.Contains(".")) goto DTEZ_AsFloat;

        int i;
        if (!int.TryParse(_text, out i)) return false;
        return i == 0;

    DTEZ_AsFloat:
        float f;
        if (!float.TryParse(_text, out f)) return false;
        return f == 0;
    }
    private void ActOnTextEqualsZero()
    {
        if (_text.Length == 0) { _text = "0"; return; }
        if (_text.Contains(".")) { if (_text[0] == '.') _text = "0" + _text; return; }
        _text = "0";
    }
}
