using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reader : MonoBehaviour
{
    public bool     _readKeyPressAsString       = false;
    public bool     _readKeyPressPrioritized    = false;
    public KeyCode  _keyToPrioritize            = KeyCode.None;
    public byte     _keyPriority                = 255;
    public Vector2  _boxPos                     = Vector2.zero;
    public Vector2  _boxExtents                 = Vector2.one;
    public float    _boxPriority                = 0;

    // Keeping
    private O_ClickBox _box = null;

    // On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.KE_PrioritizedKeyPress += OnPrioritizedKeyPress;
        NCGF_UI_S_Events.OE_KeyPress            += OnKeyPress;
        NCGF_UI_S_Events.OE_InputString         += OnInputString;
        NCGF_UI_S_Events.OE_LeftClickBox        += OnBoxClick;
        NCGF_UI_S_Events.OE_RightClickBox       += OnBoxClick;
        NCGF_UI_S_Events.OE_AnyMouseButton      += OnAnyMouseButton;
        NCGF_UI_S_Events.OE_RunUISetups         += OnUISetupComplete;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.KE_PrioritizedKeyPress -= OnPrioritizedKeyPress;
        NCGF_UI_S_Events.OE_KeyPress            -= OnKeyPress;
        NCGF_UI_S_Events.OE_InputString         -= OnInputString;
        NCGF_UI_S_Events.OE_LeftClickBox        -= OnBoxClick;
        NCGF_UI_S_Events.OE_RightClickBox       -= OnBoxClick;
        NCGF_UI_S_Events.OE_AnyMouseButton      -= OnAnyMouseButton;
        NCGF_UI_S_Events.OE_RunUISetups         -= OnUISetupComplete;
    }

    // Auto Functions

    private void Awake()
    {
    }
    // Private Functions
    private void OnUISetupComplete()
    {
        NCGF_UI_S_Events.KE_Subscribe_Go(_keyToPrioritize, _keyPriority);
        _box = NCGF_UI_S_BoxKeep.MakeAndReturnBox(_boxPos, _boxExtents.x, _boxExtents.y, _boxPriority);
    }
    private void OnKeyPress(KeyCode k, List<uint> IDs)
    {
        if (_readKeyPressAsString || _readKeyPressPrioritized) return;
        Debug.Log($"Key pressed: {k}!");
    }
    private void OnPrioritizedKeyPress(byte priority, KeyCode k, List<uint> IDs)
    {
        if (_readKeyPressAsString || !_readKeyPressPrioritized) return;
        if (priority != _keyPriority || k != _keyToPrioritize) return;
        Debug.Log($"Prioritized key pressed: '{k}' with priority {priority}!");
    }
    private void OnInputString(string s)
    {
        if (!_readKeyPressAsString) return;
        string log = "";
        log += $"Input String: {s}! Characters have unicode values:";
        foreach (char c in s)
        {
            log += $" {(int)c}";
        }
        Debug.Log(log);
    }
    private void OnBoxClick(List<uint> IDs)
    {
        if (_box == null) return;
        if (!IDs.Contains(_box._ID)) return;
        Debug.Log($"ClickBox Pressed with ID {_box._ID}!");
    }
    private void OnAnyMouseButton(List<uint> IDs)
    {
        Debug.Log($"Mouse clicked!");
    }
}
