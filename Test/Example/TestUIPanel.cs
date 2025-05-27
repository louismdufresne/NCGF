using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUIPanel : MonoBehaviour
{
    [SerializeField] private GO_Button      _button1;
    [SerializeField] private GO_Button      _button2;
    [SerializeField] private GO_InputBox    _inputBox;
    private uint    _buttonID1;
    private uint    _buttonID2;
    private uint    _inputBoxID;
    private float   _clock = 0;
    private bool    _greyedIsSet = false;

    private void OnEnable()
    {
        NCGF_UI_S_Events.AE_ButtonActivate   += HandleButtonPress;
        NCGF_UI_S_Events.AE_InputBoxActivate += HandleInputBox;
        NCGF_UI_S_Events.OE_RunUISetups      += Setup;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.AE_ButtonActivate   -= HandleButtonPress;
        NCGF_UI_S_Events.AE_InputBoxActivate -= HandleInputBox;
        NCGF_UI_S_Events.OE_RunUISetups      -= Setup;
    }
    private void Setup()
    {
        _buttonID1      = _button1.Setup();
        _buttonID2      = _button2.Setup();
        _inputBoxID     = _inputBox.Setup();
    }
    private void HandleButtonPress(uint id)
    {
        if (id == _buttonID1) Debug.Log("Button 1 pressed!");
        if (id == _buttonID2) Debug.Log("Button 2 pressed!");
    }
    private void HandleInputBox(uint id, string text)
    {
        if (id == _inputBoxID) Debug.Log($"Text: {text}");
    }
    private void Update()
    {
        _clock += Time.deltaTime;
        if (_clock > 8f) _clock -= 8f;

        if (_clock >= 4 && !_greyedIsSet) { _button1.SetGreyed(true); _greyedIsSet = true; }
        else if (_clock < 4 && _greyedIsSet) { _button1.SetGreyed(false); _greyedIsSet = false; }
    }
}
