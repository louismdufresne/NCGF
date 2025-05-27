using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] GameObject - Text Age Indicator
//[][] Displays a given animation if the dialogue being viewed is old / back in time, or current
public class NCGF_DIA_GO_TextAgeIndicator : MonoBehaviour
{
    [SerializeField] private List<Sprite> _textIsOldSprites;
    [SerializeField] private List<Sprite> _textIsCurrentSprites;
    [SerializeField] private GO_Animation _animator;
    [SerializeField] private float  _floatAwayHeight    = 1f;
    [SerializeField] private float  _floatAwaySpeed     = 1f;
    [SerializeField] private float  _alphaLossPerSecond = 2f;
    [SerializeField] private float  _oldFramePeriod     = 0.24f;
    [SerializeField] private float  _currentFramePeriod = 0.12f;

    // Keeping
    private bool    _isOldMode          = false;
    private Color   _animatorColor      = new Color(1f, 1f, 1f, 0f);
    private Vector3 _animatorLocalPosition;
    private float   _animatorOriginalY;
    private float   _animatorMaxY;
    private float   _animatorAlpha      = 0f;
    private Transform _animatorTransform;

    private bool _isSetUp = false;

    //[][] Parameters
    private static readonly Vector3 r_baseLocalPos = new Vector3(0, 0, -0.01f);

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }
    private void Update()
    {
        PerformOnMode();
    }

    //[][] Public Functions
    public void UpdateStatus(bool isDialogueOld)
    {
        Setup();

        _isOldMode = isDialogueOld;
        DoModeChangeActions();
    }

    //[][] Private Functions
    private void PerformOnMode()
    {
        if (_isOldMode) PerformOldMode();
        else PerformCurrentMode();
    }
    private void PerformOldMode()
    {
        // Nothing lol (just here for organization)
    }
    private void PerformCurrentMode()
    {
        _animatorAlpha = Mathf.Clamp(_animatorAlpha - (_alphaLossPerSecond * Time.deltaTime), 0f, 1f);
        _animatorColor.a = _animatorAlpha;
        _animator._spriteRenderer.color = _animatorColor;

        _animatorLocalPosition.y = _animatorLocalPosition.y + (_floatAwaySpeed * Time.deltaTime);
        _animatorLocalPosition.y = Mathf.Clamp(_animatorLocalPosition.y + (_floatAwaySpeed * Time.deltaTime), _animatorOriginalY, _animatorMaxY);
        _animatorTransform.localPosition = _animatorLocalPosition;
    }

    private void DoModeChangeActions()
    {
        if (_isOldMode)
        {
            _animator._allFrames = _textIsOldSprites;
            _animator._spriteRenderer.sprite = _textIsOldSprites[0];
            _animator._framePeriod = _oldFramePeriod;

            _animatorTransform.localPosition = r_baseLocalPos;
            _animatorAlpha = 1f;
            _animatorColor.a = 1f;
            _animator._spriteRenderer.color = _animatorColor;
        }
        else
        {
            _animator._allFrames = _textIsCurrentSprites;
            _animator._spriteRenderer.sprite = _textIsCurrentSprites[0];
            _animator._framePeriod = _currentFramePeriod;
            _animatorLocalPosition = r_baseLocalPos;
        }
    }
    private void Setup()
    {
        if (_isSetUp) return;

        _animator._allFrames = _textIsCurrentSprites;
        _animator._spriteRenderer.sprite = _textIsCurrentSprites[0];
        _animator._spriteRenderer.color = _animatorColor;

        _animatorTransform = _animator.transform;
        _animatorTransform.parent = transform;
        _animatorTransform.localPosition = r_baseLocalPos;
        _animatorLocalPosition = r_baseLocalPos;

        _animatorOriginalY = r_baseLocalPos.y;
        _animatorMaxY = _animatorOriginalY + _floatAwayHeight;

        _isSetUp = true;
    }
}
