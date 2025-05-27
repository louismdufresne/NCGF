using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] GameObject - Animation
//[][] Displays a repeating succession of sprites
public class GO_Animation : MonoBehaviour
{
    [SerializeField] public List<Sprite>                        _allFrames = new List<Sprite>();
    [SerializeField] public Sprite                              _defaultSprite;
    [SerializeField] [Range(0, float.MaxValue)] public float    _framePeriod;
    [SerializeField] public bool                                _alternatingVertFlip = false;
    [SerializeField] public SpriteRenderer                      _spriteRenderer;
    [SerializeField] public Mode                                _mode = Mode.None;

    private float   _clock      = 0;
    private int     _curFrame   = 0;
    private Mode    _modePrevF  = Mode.None;

    public enum Mode : byte
    {
        None                    = 0,
        Repeat                  = 1,
        Repeat_And_Cycle_Random = 2,
        Single_Pass_And_End     = 3,
        Single_Pass_And_Hold    = 4,
    }

    //[][] On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.OE_RunFrame += RunFrame;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.OE_RunFrame -= RunFrame;
    }

    //[][] Auto Functions
    private void Awake()
    {
        SetUp();
    }

    private void RunFrame()
    {
        // DO NOT call RunFrame from inside this script; this may cause an infinite loop.
        // Call ForceUpdateFrame (or similar) instead
        CheckForNoFrames();
        DetermineUpdate();
    }

    //[][] Public Functions
    public void ForceUpdateFrame()
    {
        CheckForNoFrames();
        UpdateFrame();
        _clock = 0;
    }
    public void SetModeAndStart(Mode m)
    {
        _mode = m;
        _clock = _framePeriod + 0.001f;
        _curFrame = -1;
        ForceUpdateFrame();
    }

    //[][] Private Functions
    private void SetUp()
    {
        if (_spriteRenderer == null) _spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) _spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
        CheckForNoFrames();
        SetModeAndStart(_mode);
    }
    private void CheckForNoFrames()
    {
        if (_allFrames == null) _allFrames = new List<Sprite>();
        if (_allFrames.Count == 0)
        {
            if (_defaultSprite != null) _spriteRenderer.sprite = _defaultSprite;
        }
    }
    private void DetermineUpdate()
    {
        if (_modePrevF == _mode)    UpdateFrame();
        else                        SetModeAndStart(_mode);
    }
    private void UpdateFrame()
    {
        if (_allFrames.Count == 0) return;

        _clock += Time.deltaTime;
        if (_clock < _framePeriod) return;
        _clock -= _framePeriod;

        switch (_mode)
        {
            case Mode.None:
                _curFrame = _allFrames.Count;
                break;
            case Mode.Repeat:
                _curFrame = (_curFrame + 1) % _allFrames.Count;
                break;
            case Mode.Repeat_And_Cycle_Random:
                _curFrame = NCGF_Calc.RandIntExcept(0, _allFrames.Count, _curFrame);
                break;
            case Mode.Single_Pass_And_End:
            case Mode.Single_Pass_And_Hold:
                if (_curFrame >= _allFrames.Count - 1) _curFrame = (_mode == Mode.Single_Pass_And_End) ? _allFrames.Count : _curFrame;
                else _curFrame = (_curFrame + 1) % _allFrames.Count;
                break;
            default:
                break;
        }

        _spriteRenderer.sprite = (_curFrame >= _allFrames.Count) ? _defaultSprite : _allFrames[_curFrame];
        if (_alternatingVertFlip) _spriteRenderer.flipY = !_spriteRenderer.flipY;

        _modePrevF = _mode;
    }
}
