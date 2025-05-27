using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//[][] Handler - Actuator
//[][] Handles decision-making for user input

public class NCGF_UI_H_Actuator : MonoBehaviour
{
    //[][] Input Tracking Vars
    private Vector2             _mousePos       = Vector2.zero;
    private List<O_ClickBox>    _curBoxes       = new List<O_ClickBox>();
    private List<uint>          _curIDs         = new List<uint>();
    private bool                _lmbDown        = false;
    private bool                _rmbDown        = false;
    private bool                _isLDragging    = false;
    private bool                _isRDragging    = false;
    private float               _scrollAmt      = 0;
    private KeyCode[]           _keyPressed     = new KeyCode[_keyPressBufferSize];
    private bool[]              _keyDown        = new bool[_keyPressBufferSize];
    private bool[]              _keyDownPrevF   = new bool[_keyPressBufferSize];
    private string              _inputString    = "";

    //[][] Parameters
    private static byte _keyPressBufferSize = 32;

    //[][] Keeping
    // _maxBox is added to _curBoxes so that, for fired events, subscribers which do not require specific boxes can match IDs with uint.MaxValue
    private O_ClickBox _maxBox = new O_ClickBox(Vector2.zero, 0, 0, uint.MaxValue, float.MaxValue);

    //[][] Auto Functions
    private void Start()
    {
        Setup();
    }

    //[][] Enable / Disable
    #region Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.IE_RunFrame    += RunFrame;
        NCGF_UI_S_Events.IE_MousePos    += MousePosHandle;
        NCGF_UI_S_Events.IE_LeftHold    += LeftHoldHandle;
        NCGF_UI_S_Events.IE_RightHold   += RightHoldHandle;
        NCGF_UI_S_Events.IE_Scroll      += ScrollHandle;
        NCGF_UI_S_Events.IE_KeyHold     += KeyHandle;
        NCGF_UI_S_Events.IE_InputString += InputStringHandle;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.IE_RunFrame    -= RunFrame;
        NCGF_UI_S_Events.IE_MousePos    -= MousePosHandle;
        NCGF_UI_S_Events.IE_LeftHold    -= LeftHoldHandle;
        NCGF_UI_S_Events.IE_RightHold   -= RightHoldHandle;
        NCGF_UI_S_Events.IE_Scroll      -= ScrollHandle;
        NCGF_UI_S_Events.IE_KeyHold     -= KeyHandle;
        NCGF_UI_S_Events.IE_InputString -= InputStringHandle;
    }
    #endregion

    //[][] Event Handle Functions
    #region Event Handle Functions
    private void RunFrame()
    {
        SetBoxes();
        HandleButtons();

        NCGF_UI_S_Events.OE_RunFrame_Go();
    }
    private void MousePosHandle(Vector2 pos) => _mousePos = pos;
    private void LeftHoldHandle()
    {
        _lmbDown = true;
    }
    private void RightHoldHandle()
    {
        _rmbDown = true;
    }
    private void ScrollHandle(float amount)
    {
        _scrollAmt = amount;
    }
    private void KeyHandle(KeyCode k)
    {
        for (int i = 0; i < _keyPressBufferSize; i++)
        {
            if (_keyPressed[i] == k && _keyDownPrevF[i]) { _keyDown[i] = true; return; }
        }
        for (int i = 0; i < _keyPressBufferSize; i++)
        {
            if (!_keyDownPrevF[i])
            {
                _keyDown[i] = true;
                _keyPressed[i] = k;
                break;
            }
        }
    }
    private void InputStringHandle(string s)
    {
        _inputString = s;
    }
    #endregion

    //[][] Operational Functions
    #region Operational Functions
    private void Setup()
    {
        if (NCGF_Res._actuator != this && NCGF_Res._actuator != null)
        {
            NCGF_Res._actuator.gameObject.SetActive(false);
            Destroy(NCGF_Res._actuator);
        }
        NCGF_Res._actuator = this;

        for (int i = 0; i < _keyPressBufferSize; i++)
        {
            _keyDown[i] = false;
            _keyDownPrevF[i] = false;
            _keyPressed[i] = KeyCode.None;
        }
        // Now that this has set up, so can everything else
        NCGF_UI_S_Events.OE_RunUISetups_Go();
    }
    private void SetBoxes()
    {
        _curBoxes.Remove(_maxBox);

        var newBoxes = GetTopBoxesAt(_mousePos);
        List<uint> olds = new List<uint>();
        List<uint> news = new List<uint>();

        foreach (var x in _curBoxes) if (newBoxes.FirstOrDefault(y => x._ID == y._ID) == null) olds.Add(x._ID);
        foreach (var x in newBoxes) if (_curBoxes.FirstOrDefault(y => x._ID == y._ID) == null) news.Add(x._ID);

        if (olds.Count != 0) NCGF_UI_S_Events.OE_HoverBoxEnd_Go(olds);
        if (news.Count != 0) NCGF_UI_S_Events.OE_HoverBoxStart_Go(news);

        _curBoxes.Clear();
        _curBoxes = newBoxes;
        _curBoxes.Add(_maxBox);

        _curIDs.Clear();
        foreach (var x in _curBoxes) _curIDs.Add(x._ID);
    }
    private void HandleButtons()
    {
        // Pos
        NCGF_UI_S_Events.OE_MousePos_Go(_mousePos);

        // Down
        if (_lmbDown && !_isLDragging)
        {
            _isLDragging = true;
            DoLeftClickDown();
        }
        if (_rmbDown && !_isRDragging)
        {
            _isRDragging = true;
            DoRightClickDown();
        }

        // Up
        if (!_lmbDown && _isLDragging)
        {
            _isLDragging = false;
            DoLeftClickUp();
        }
        if (!_rmbDown && _isRDragging)
        {
            _isRDragging = false;
            DoRightClickUp();
        }

        // Scroll
        NCGF_UI_S_Events.OE_ScrollBox_Go(_curIDs, _scrollAmt);

        // Key
        for (int i = 0; i < _keyPressBufferSize; i++)
        {
            if (_keyDown[i] && !_keyDownPrevF[i])
            {
                NCGF_UI_S_Events.OE_KeyPress_Go(_keyPressed[i], _curIDs);
                _keyDownPrevF[i] = true;
            }
            else if (!_keyDown[i] && _keyDownPrevF[i])
            {
                NCGF_UI_S_Events.OE_KeyPressUp_Go(_keyPressed[i], _curIDs);
                _keyDownPrevF[i] = false;
            }
        }

        // String
        if (_inputString.Length != 0) NCGF_UI_S_Events.OE_InputString_Go(_inputString);


        // Reset buttons; they will be set back to true / values through events if still held
        _lmbDown = false;
        _rmbDown = false;
        _scrollAmt = 0;
        for (int i = 0; i < _keyPressBufferSize; i++)
        {
            if (!_keyDown[i]) _keyDownPrevF[i] = false;
            _keyDown[i] = false;
        }
        _inputString = "";
    }
    private void DoLeftClickDown()
    {
        NCGF_UI_S_Events.OE_AnyMouseButton_Go(_curIDs);
        NCGF_UI_S_Events.OE_LDragStart_Go(_mousePos, _curIDs);
        NCGF_UI_S_Events.OE_LeftClickBox_Go(_curIDs);
    }
    private void DoLeftClickUp()
    {
        NCGF_UI_S_Events.OE_LDragEnd_Go(_mousePos, _curIDs);
        NCGF_UI_S_Events.OE_LeftClickBoxUp_Go(_curIDs);
    }
    private void DoRightClickDown()
    {
        NCGF_UI_S_Events.OE_AnyMouseButton_Go(_curIDs);
        NCGF_UI_S_Events.OE_RDragStart_Go(_mousePos, _curIDs);
        NCGF_UI_S_Events.OE_RightClickBox_Go(_curIDs);
    }
    private void DoRightClickUp()
    {
        NCGF_UI_S_Events.OE_RDragEnd_Go(_mousePos, _curIDs);
        NCGF_UI_S_Events.OE_RightClickBoxUp_Go(_curIDs);
    }
    private List<O_ClickBox> GetTopBoxesAt(Vector2 pos) => NCGF_UI_S_BoxKeep.GetTopBoxesAt(pos);

    #endregion
}
