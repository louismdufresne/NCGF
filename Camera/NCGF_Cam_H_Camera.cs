using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[][] GameObject - Camera Move
//[][] Camera motion control script using the UIB input control system

/*
 * CamMove is responsible for moving the camera around the scene based on user input.
 * Currently it can drag the camera around and zoom the camera in and out.
 * Drag and zoom sensitivity is based in part on the amout of zoom, i.e. when you are
 * zoomed in closer the sensitivity drops so that the scene doesn't whiz by.
 */
public class NCGF_Cam_H_Camera : MonoBehaviour
{
    public Mode         _mode = Mode.Controlling;
    // Parameters
    [SerializeField] private NCGF_Cam_RG_Data _cd;

    // Keeping
    private Vector3     _goToPoint;
    private Vector2     _mousePos;          // World coordinates
    private Vector3     _dragStart;         // Screen coordinates
    private bool        _dragging           = false;
    private float       _scrollAmount       = 0;
    [SerializeField]
    private Camera      _thisCamera;
    private float       _goToScale;
    private float       _maxX, _maxY;
    private float       _actualMaxScale;
    private Vector2     _cameraWorldCenter;
    private float       _cameraPrevAspect;
    private bool        _isKeySubscribed    = false;

    // UI interfacing
    private O_ClickBox  _clickBox;
    private static readonly Dictionary<KeyCode, int> _keySIndices = new Dictionary<KeyCode, int>
    {
        { KeyCode.W, 0 },
        { KeyCode.A, 1 },
        { KeyCode.S, 2 },
        { KeyCode.D, 3 },
        { KeyCode.UpArrow,      0 },
        { KeyCode.LeftArrow,    1 },
        { KeyCode.DownArrow,    2 },
        { KeyCode.RightArrow,   3 },
        { KeyCode.LeftShift,    4 },
    };
    private bool[] _keyStatuses = new bool[5];

    [SerializeField]
    private bool _doWorldLimit;

    // Technical
    private bool _setUp = false;

    public enum Mode : byte
    {
        Not_Controlling     = 0,
        Controlling         = 1,
    }

    //[][] OnEnable / OnDisable
    #region OnEnable / OnDisable
    private void OnEnable()
    {
        DoSubscribes();
    }
    private void OnDisable()
    {
        DoUnsubscribes();
    }
    #endregion

    //[][] Public Functions
    public void Yoink(Vector2 position, Vector2 goToPosition, Vector2 center, float scale, float goToScale)
    {
        // Center camera and world on position
        _cameraWorldCenter = center;
        _goToPoint = new Vector3(goToPosition.x, goToPosition.y, _cd._cameraZ);
        transform.position = new Vector3(position.x, position.y, _cd._cameraZ);

        // Set camera to "pop" in or out of position
        _thisCamera.orthographicSize = scale;
        _goToScale = goToScale;

        NCGF_UI_S_Events.CE_CameraYoink_Go();
    }
    public void SetMode(Mode newMode)
    {
        _mode = newMode;

        switch (_mode)
        {
            case Mode.Controlling:
                DoKeySubscribes(false);
                break;
            case Mode.Not_Controlling:
                DoKeySubscribes(true);
                break;
            default:
                break;
        }
    }

    //[][] Private Functions
    private void OnInputEnd()
    {
        if (_mode == Mode.Not_Controlling) goto OIE_NotControlling;

        // Update go-to position from WASD values
        CheckAllKeys();

        // Update go-to position from right-click mouse drag
        CheckDrag();

        // Update scale based on scroll wheel movement
        CheckScroll();

    OIE_NotControlling:

        DoApproaches();

        OnEndFrame();
    }
    private void Setup()
    {
        if (_setUp) return;

        _thisCamera ??= GetComponent<Camera>();
        _thisCamera ??= Camera.main;
        NCGF_Res._camera        = _thisCamera;
        NCGF_Res._cameraMove    = this;
        NCGF_Res._camData       = _cd;
        _cd._cameraOriginalSize = _thisCamera.orthographicSize;

        // Initialize camera go-to point
        _goToPoint = new Vector3(transform.position.x, transform.position.y, _cd._cameraZ);
        _cameraWorldCenter = Vector2.zero;

        _thisCamera.orthographicSize = _cd._defaultScale;
        _goToScale = _cd._defaultScale;
        _cameraPrevAspect = _thisCamera.aspect;

        // UI
        _clickBox = NCGF_UI_S_BoxKeep.MakeAndReturnBox(
            transform.position,
            _cd._cameraClickBoxWidthHeight,
            _cd._cameraClickBoxWidthHeight,
            _cd._cameraClickBoxZPos);
        _clickBox._isActive = true;
        _clickBox._soft = true;

        SetMode(_mode);

        _setUp = true;
        NCGF_UI_S_Events.CE_EndOfCameraSetup_Go();
    }
    private void DoApproaches()
    {
        ApproachGoToScale();
        ApproachGoToPoint();
    }

    private void CheckAllKeys()
    {
        // This function does NOT need to compensate for screen resolution.

        float localSpeed = _cd._camSpeed;
        localSpeed += _thisCamera.orthographicSize * _cd._camSizeImpactOnSpeed;
        localSpeed *= Time.deltaTime;
        localSpeed /= (1 + _cd._leftShiftSlowdown * BoolByte(_keyStatuses[_keySIndices[KeyCode.LeftShift]]));

        _goToPoint += new Vector3(
            (BoolByte(_keyStatuses[_keySIndices[KeyCode.D]])
            - BoolByte(_keyStatuses[_keySIndices[KeyCode.A]])) * localSpeed,
            (BoolByte(_keyStatuses[_keySIndices[KeyCode.W]])
            - BoolByte(_keyStatuses[_keySIndices[KeyCode.S]])) * localSpeed,
            0);
    }

    private void CheckDrag()
    {
        // This function DOES need to compensate for screen resolution.

        if (_dragging)
        {
            Vector3 goToPointDelta = -1 * (_thisCamera.WorldToScreenPoint(_mousePos) - _dragStart) * _cd._dragSensitivity;
            goToPointDelta *= _thisCamera.orthographicSize * _cd._camSizeImpactOnDragSensitivity;
            goToPointDelta /= _thisCamera.pixelHeight;
            goToPointDelta /= (1 + _cd._leftShiftSlowdown * BoolByte(_keyStatuses[_keySIndices[KeyCode.LeftShift]]));


            _goToPoint += goToPointDelta;
            _dragStart = _thisCamera.WorldToScreenPoint(_mousePos);
        }
    }

    private void CheckScroll()
    {
        // This function does NOT need to compensate for screen resolution.
        if (_scrollAmount == 0) return;

        _actualMaxScale = _cd._worldSize;
        if (_actualMaxScale < 0.1f || _actualMaxScale > _cd._maximumScale) _actualMaxScale = _cd._maximumScale;

        float goToScaleDelta = _scrollAmount * _cd._scrollSensitivity;
        goToScaleDelta *= _thisCamera.orthographicSize * _cd._camSizeImpactOnScrollSensitivity;
        goToScaleDelta /= (1 + _cd._leftShiftSlowdown * BoolByte(_keyStatuses[_keySIndices[KeyCode.LeftShift]]));

        _goToScale += goToScaleDelta;

        if (_goToScale < _cd._minimumScale) _goToScale = _cd._minimumScale;
        else if (_goToScale > _actualMaxScale) _goToScale = _actualMaxScale;
    }

    private void ApproachGoToScale()
    {
        float localAdherence = _cd._scrollAdherence * Time.deltaTime;
        if (localAdherence > _cd._maximumAdherence) localAdherence = _cd._maximumAdherence;

        _thisCamera.orthographicSize += (_goToScale - _thisCamera.orthographicSize) * localAdherence;
    }

    private void ApproachGoToPoint()
    {
        float localAdherence = _cd._camAdherence * Time.deltaTime;
        if (localAdherence < _cd._minimumAdherence) localAdherence = _cd._minimumAdherence;
        if (localAdherence > _cd._maximumAdherence) localAdherence = _cd._maximumAdherence;

        // Keep go-to point within camera world boundary
        if (!_doWorldLimit) goto SkipLimit;

        _maxX = _cd._worldSize - _thisCamera.orthographicSize;
        _maxY = _cd._worldSize - (_thisCamera.orthographicSize / _thisCamera.aspect);

        if (_goToPoint.x > _maxX + _cameraWorldCenter.x) _goToPoint.x = _maxX + _cameraWorldCenter.x;
        else if (_goToPoint.x < -1f * _maxX + _cameraWorldCenter.x) _goToPoint.x = -1f * _maxX + _cameraWorldCenter.x;

        if (_goToPoint.y > _maxY + _cameraWorldCenter.y) _goToPoint.y = _maxY + _cameraWorldCenter.y;
        else if (_goToPoint.y < -1f * _maxY + _cameraWorldCenter.y) _goToPoint.y = -1f * _maxY + _cameraWorldCenter.y;

        SkipLimit:

        transform.position += (_goToPoint - transform.position) * localAdherence;
    }
    private void OnEndFrame()
    {
        _clickBox._centerX = _mousePos.x;
        _clickBox._centerY = _mousePos.y;

        // Check for new aspect
        if (_thisCamera.aspect != _cameraPrevAspect)
        {
           NCGF_UI_S_Events.CE_CameraAspectChange_Go();
            _cameraPrevAspect = _thisCamera.aspect;
        }

        // Launch camera update frame end event - necessary for proper operation of camera-following UI
        NCGF_UI_S_Events.CE_EndOfCameraUpdate_Go();
    }
    byte BoolByte(bool b) { if (b) return 1; else return 0; }

    // Event Handling
    private void HandleKey(byte priority, KeyCode k, List<uint> IDs)
    {
        if (priority != NCGF_UI_R_Parameters._cameraKeyPriority) return;
        if (!IDs.Contains(_clickBox._ID)) return;
        if (_keySIndices.ContainsKey(k)) _keyStatuses[_keySIndices[k]] = true;
    }
    private void HandleKeyUp(byte priority, KeyCode k, List<uint> IDs)
    {
        // ID / Priority is not checked; keys should always release
        if (_keySIndices.ContainsKey(k)) _keyStatuses[_keySIndices[k]] = false;
    }
    private void HandleDragStart(Vector2 pos, List<uint> IDs)
    {
        if (!IDs.Contains(_clickBox._ID)) return;
        _dragStart = _thisCamera.WorldToScreenPoint(pos);
        _dragging = true;
    }
    private void HandleDragEnd(Vector2 pos, List<uint> IDs)
    {
        // ID is not checked because dragging should always end
        _dragging = false;
    }
    private void HandleMousePos(Vector2 pos) => _mousePos = pos;
    private void HandleScroll(List<uint> IDs, float amount)
    {
        if (IDs.Contains(_clickBox._ID)) _scrollAmount = amount;
        else _scrollAmount = 0;
    }
    private void DoKeySubscribes(bool isUnsubscribing)
    {
        KeyCode[] keys = new KeyCode[_keySIndices.Keys.Count];
        _keySIndices.Keys.CopyTo(keys, 0);
        byte priority = NCGF_UI_R_Parameters._cameraKeyPriority;

        for (int i = 0; i < keys.Length; i++)
        {
            if (isUnsubscribing && _isKeySubscribed) NCGF_UI_S_Events.KE_Unsubscribe_Go(keys[i], priority);
            else if (!isUnsubscribing && !_isKeySubscribed) NCGF_UI_S_Events.KE_Subscribe_Go(keys[i], priority);
        }
        _isKeySubscribed = !isUnsubscribing;
    }
    private void DoSubscribes()
    {
        NCGF_UI_S_Events.OE_RDragStart   += HandleDragStart;
        NCGF_UI_S_Events.OE_RDragEnd     += HandleDragEnd;
        NCGF_UI_S_Events.OE_MousePos     += HandleMousePos;
        NCGF_UI_S_Events.KE_PrioritizedKeyPress     += HandleKey;
        NCGF_UI_S_Events.KE_PrioritizedKeyPressUp   += HandleKeyUp;
        NCGF_UI_S_Events.OE_RunFrame     += OnInputEnd;
        NCGF_UI_S_Events.OE_ScrollBox    += HandleScroll;
        NCGF_UI_S_Events.OE_RunUISetups  += Setup;
    }
    private void DoUnsubscribes()
    {
        NCGF_UI_S_Events.OE_RDragStart   -= HandleDragStart;
        NCGF_UI_S_Events.OE_RDragEnd     -= HandleDragEnd;
        NCGF_UI_S_Events.OE_MousePos     -= HandleMousePos;
        NCGF_UI_S_Events.KE_PrioritizedKeyPress     -= HandleKey;
        NCGF_UI_S_Events.KE_PrioritizedKeyPressUp   -= HandleKeyUp;
        NCGF_UI_S_Events.OE_RunFrame     -= OnInputEnd;
        NCGF_UI_S_Events.OE_ScrollBox    -= HandleScroll;
        NCGF_UI_S_Events.OE_RunUISetups  -= Setup;
    }

}