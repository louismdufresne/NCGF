using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] GameObject - UI Button
//[][] Single instantiable UI Button

//[][] NOTE:
//[][] This button should be instantiated via prefab and/or in the editor and contain:
//[][]      1) Sprite Renderer

public class GO_Button : MonoBehaviour
{
    // Sprites
    [SerializeField] public Sprite  _defaultSprite;
    [SerializeField] public Sprite  _hoverOverSprite;
    [SerializeField] public Sprite  _clickedSprite;
    [SerializeField] public Sprite  _greyedSprite;

    // Specifications
    [SerializeField] public bool    _activateOnClickUp      = true;     // If true, fires event on button up instead of button down
    [SerializeField] public bool    _acceptsLClick          = true;
    [SerializeField] public bool    _acceptsRClick          = false;
    [SerializeField] public bool    _resizesToSprite        = true;
    [SerializeField] public bool    _boxFollowsGOMovement   = true;

    // References
    public SpriteRenderer           _spriteRenderer;
    private O_ClickBox              _clickBox;

    // Statuses
    // NOTE: Three bools are used in favor of an enum here because of the nuances around when _clicked and _hovered are true or not.
    // "Don't fix what ain't broke"
    private bool _hovered   = false;
    private bool _clicked   = false;
    private bool _greyed    = false;

    // Technical
    private bool _isSetUp     = false;


    // Setup
    // This should be called by the UI panel after UIB_H_UIOperator's Awake function
    public uint Setup()     // ID
    {
        _spriteRenderer ??= GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = _defaultSprite;

        if (_clickBox != null)      // I.e. if button was pooled
        {
            NCGF_UI_S_BoxKeep.RemoveBox(_clickBox._ID);  
        }
        _clickBox = NCGF_UI_S_BoxKeep.MakeAndReturnBox(
            transform.position,
            1f, 1f,
            transform.position.z);

        _clickBox._isActive = true;

        // _isSetUp must be set true prior to ResizeBoxToCurrentSprite() because that function checks its value
        _isSetUp = true;
        if (_resizesToSprite) ResizeBoxToCurrentSprite();

        return _clickBox._ID;
    }

    // Public Functions
    public void SetDefaultValues()
    {
        _activateOnClickUp      = true;
        _acceptsLClick          = true;
        _acceptsRClick          = false;
        _resizesToSprite        = true;
        _boxFollowsGOMovement   = true;
        if (_clickBox != null) { _clickBox._soft = false; _clickBox._round = false; }
    }
    public void SetGreyed(bool isGreyed)
    {
        _greyed = isGreyed;
        _clicked = false;
        DetermineSprite();
    }
    public void ManualResize(float halfWidth, float halfHeight)
    {
        if (!_isSetUp) return;

        _clickBox._halfWidth = halfWidth;
        _clickBox._halfHeight = halfHeight;
    }
    public void SetResizeToSprite(bool resizesToSprite)
    {
        _resizesToSprite = resizesToSprite;
        if (resizesToSprite) ResizeBoxToCurrentSprite();
    }
    public void FloodAsSprite(Sprite s)
    {
        _defaultSprite = s;
        _hoverOverSprite = s;
        _clickedSprite = s;
        _greyedSprite = s;
        _spriteRenderer.sprite = s;
    }
    public void SetAllSprites (Sprite normal, Sprite hovered, Sprite clicked, Sprite greyed)
    {
        _defaultSprite      = normal;
        _hoverOverSprite    = hovered;
        _clickedSprite      = clicked;
        _greyedSprite       = greyed;
        DetermineSprite();
    }
    public uint GetID() => _clickBox._ID;
    public void SetSoft(bool isSoft) { _clickBox._soft = isSoft; }
    public void SetRound(bool isRound) { _clickBox._round = isRound; }

    // On Enable / Disable
    private void OnEnable()
    {
        DoSubscribes();

        if (!_isSetUp) return;

        _clickBox._isActive = true;
        
    }
    private void OnDisable()
    {
        DoUnsubscribes();

        if (!_isSetUp) return;

        _clickBox._isActive = false;

        ResetButton();
    }

    // Functionality
    #region Functionality
    private void DoUpdateFunctions()
    {
        if (!_isSetUp) return;

        if (_boxFollowsGOMovement) RecenterBoxOnObject();
        if (_boxFollowsGOMovement && _resizesToSprite) ResizeBoxToCurrentSprite();
    }
    private void ResizeBoxToCurrentSprite()
    {
        if (!_isSetUp) return;

        if (_spriteRenderer == null) return;
        if (_spriteRenderer.sprite == null) return;

        Bounds bounds = _spriteRenderer.sprite.bounds;
        _clickBox._halfWidth    = bounds.extents.x * transform.lossyScale.x;
        _clickBox._halfHeight   = bounds.extents.y * transform.lossyScale.y;
        RecenterBoxOnObject();
    }
    private void DetermineSprite()
    {
        if (_spriteRenderer == null) return;
        // In this order
        if (_greyed)        _spriteRenderer.sprite = _greyedSprite;
        else if (_clicked)  _spriteRenderer.sprite = _clickedSprite;
        else if (_hovered)  _spriteRenderer.sprite = _hoverOverSprite;
        else                _spriteRenderer.sprite = _defaultSprite;

        if (_resizesToSprite) ResizeBoxToCurrentSprite();
    }
    private void RecenterBoxOnObject()
    {
        _clickBox._centerX = transform.position.x;
        _clickBox._centerY = transform.position.y;
    }
    #endregion

    // Event Handling
    #region Event Handling

    // Hover
    private void HandleHoverStart(List<uint> IDs)
    {
        if (!_isSetUp) return;

        if (!IDs.Contains(_clickBox._ID)) return;
        _hovered = true;
        DetermineSprite();
    }
    private void HandleHoverEnd(List<uint> IDs)
    {
        if (!_isSetUp) return;

        if (!IDs.Contains(_clickBox._ID) || !_hovered) return;
        _hovered = false;
        DetermineSprite();
    }

    // Click
    private void HandleLClickDown(List<uint> IDs)  { if (_acceptsLClick) HandleClickDown(IDs); }
    private void HandleLClickUp(List<uint> IDs)    { if (_acceptsLClick) HandleClickUp(IDs); }
    private void HandleRClickDown(List<uint> IDs)  { if (_acceptsRClick) HandleClickDown(IDs); }
    private void HandleRClickUp(List<uint> IDs)    { if (_acceptsRClick) HandleClickUp(IDs); }
    private void HandleClickDown(List<uint> IDs)
    {
        if (!_isSetUp) return;

        if (!IDs.Contains(_clickBox._ID)) return;
        if (!_activateOnClickUp && !_greyed) NCGF_UI_S_Events.AE_ButtonActivate_Go(_clickBox._ID);
        _clicked = true;
        DetermineSprite();
    }
    private void HandleClickUp(List<uint> IDs)
    {
        if (!_isSetUp) return;

        if (!_clicked) return;
        if (_activateOnClickUp && !_greyed) NCGF_UI_S_Events.AE_ButtonActivate_Go(_clickBox._ID);
        _clicked = false;
        DetermineSprite();
    }
    private void HandleRunUIFrame()
    {
        DoUpdateFunctions();
    }

    // Reset
    private void ResetButton()
    {
        _greyed = _hovered = _clicked = false;
        DetermineSprite();
    }
    #endregion

    // Subscription Management
    #region Subscription Management
    private void DoSubscribes()
    {
        NCGF_UI_S_Events.OE_HoverBoxStart    += HandleHoverStart;
        NCGF_UI_S_Events.OE_HoverBoxEnd      += HandleHoverEnd;
        NCGF_UI_S_Events.OE_LeftClickBox     += HandleLClickDown;
        NCGF_UI_S_Events.OE_RightClickBox    += HandleRClickDown;
        NCGF_UI_S_Events.OE_LeftClickBoxUp   += HandleLClickUp;
        NCGF_UI_S_Events.OE_RightClickBoxUp  += HandleRClickUp;
        NCGF_UI_S_Events.OE_RunFrame         += HandleRunUIFrame;
    }
    private void DoUnsubscribes()
    {
        NCGF_UI_S_Events.OE_HoverBoxStart    -= HandleHoverStart;
        NCGF_UI_S_Events.OE_HoverBoxEnd      -= HandleHoverEnd;
        NCGF_UI_S_Events.OE_LeftClickBox     -= HandleLClickDown;
        NCGF_UI_S_Events.OE_RightClickBox    -= HandleRClickDown;
        NCGF_UI_S_Events.OE_LeftClickBoxUp   -= HandleLClickUp;
        NCGF_UI_S_Events.OE_RightClickBoxUp  -= HandleRClickUp;
        NCGF_UI_S_Events.OE_RunFrame         -= HandleRunUIFrame;
    }
    #endregion 

}