using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//[][] UI - Static - Events
//[][] Outlines events and associated function calls pertaining to UI operation

public static class NCGF_UI_S_Events
{
    //[][] Events
    #region Events

    //[][] Input        [Events that control NCFD_UI_H_Actuator]

    public static event Action<Vector2>         IE_MousePos;     // Position
    public static event Action                  IE_LeftHold;     //
    public static event Action                  IE_RightHold;    //
    public static event Action<float>           IE_Scroll;       // Scroll Amount
    public static event Action<KeyCode>         IE_KeyHold;      // Key
    public static event Action<string>          IE_InputString;  //
    //[][] Run
    //[][] This should be the last UI event sent in a given frame
    public static event Action                  IE_RunFrame;     //

    //[][] Operation    [Events fired by NCFD_UI_H_Actuator]
    //[][] "Up" refers to the button being released
    //[][] (the passed ID value is not necessarily the same value as passed when clicked down)

    public static event Action<Vector2>             OE_MousePos;         // Position
    public static event Action<List<uint>>          OE_AnyMouseButton;   // IDs
    public static event Action<List<uint>>          OE_LeftClickBox;     // IDs
    public static event Action<List<uint>>          OE_LeftClickBoxUp;   // IDs
    public static event Action<List<uint>>          OE_RightClickBox;    // IDs
    public static event Action<List<uint>>          OE_RightClickBoxUp;  // IDs
    public static event Action<List<uint>, float>   OE_ScrollBox;        // IDs, Amount
    public static event Action<Vector2, List<uint>> OE_LDragStart;       // Position, IDs
    public static event Action<Vector2, List<uint>> OE_LDragEnd;         // Position, IDs
    public static event Action<Vector2, List<uint>> OE_RDragStart;       // Position, IDs
    public static event Action<Vector2, List<uint>> OE_RDragEnd;         // Position, IDs
    public static event Action<List<uint>>          OE_HoverBoxStart;    // IDs
    public static event Action<List<uint>>          OE_HoverBoxEnd;      // IDs
    public static event Action<KeyCode, List<uint>> OE_KeyPress;         // Key, IDs
    public static event Action<KeyCode, List<uint>> OE_KeyPressUp;       // Key, IDs
    public static event Action<string>              OE_InputString;      //
    //[][] Setup
    public static event Action                      OE_RunUISetups;
    //[][] Run
    //[][] Sent by UI Actuator after sending all other events in a frame
    public static event Action                      OE_RunFrame;

    //[][] Action       [Events fired by UI elements, e.g. buttons]

    public static event Action<uint>                AE_ButtonActivate;   // ID
    public static event Action<uint, string>        AE_InputBoxActivate; // ID, Text

    //[][] Keys         [Events involving NCGF_UI_H_KeyArbiter]

    public static event Action<KeyCode, byte>               KE_Subscribe;
    public static event Action<KeyCode, byte>               KE_Unsubscribe;
    public static event Action<byte, KeyCode, List<uint>>   KE_PrioritizedKeyPress;
    public static event Action<byte, KeyCode, List<uint>>   KE_PrioritizedKeyPressUp;

    //[][] Camera       [Events pertaining to camera operation]

    public static event Action                      CE_EndOfCameraUpdate;
    public static event Action                      CE_EndOfCameraSetup;
    public static event Action                      CE_CameraAspectChange;
    public static event Action                      CE_CameraYoink;

    //[][] Dialogue     [Events for dialogue operation]

    public static event Action<int, List<ushort>>           DE_PerformDialogue;
    #endregion

    //[][] Functions
    #region Event Functions

    //[][] Input
    public static void IE_RunFrame_Go()                                 => IE_RunFrame?.Invoke();
    public static void IE_MousePos_Go(Vector2 pos)                      => IE_MousePos?.Invoke(pos);
    public static void IE_LeftHold_Go()                                 => IE_LeftHold?.Invoke();
    public static void IE_RightHold_Go()                                => IE_RightHold?.Invoke();
    public static void IE_Scroll_Go(float amount)                       => IE_Scroll?.Invoke(amount);
    public static void IE_KeyHold_Go(KeyCode k)                         => IE_KeyHold?.Invoke(k);
    public static void IE_InputString_Go(string s)                      => IE_InputString?.Invoke(s);

    //[][] Operation
    public static void OE_RunFrame_Go()                                 => OE_RunFrame?.Invoke();
    public static void OE_RunUISetups_Go()                              => OE_RunUISetups?.Invoke();
    public static void OE_MousePos_Go(Vector2 pos)                      => OE_MousePos?.Invoke(pos);
    public static void OE_AnyMouseButton_Go(List<uint> IDs)             => OE_AnyMouseButton?.Invoke(IDs);
    public static void OE_LeftClickBox_Go(List<uint> IDs)               => OE_LeftClickBox?.Invoke(IDs);
    public static void OE_LeftClickBoxUp_Go(List<uint> IDs)             => OE_LeftClickBoxUp?.Invoke(IDs);
    public static void OE_RightClickBox_Go(List<uint> IDs)              => OE_RightClickBox?.Invoke(IDs);
    public static void OE_RightClickBoxUp_Go(List<uint> IDs)            => OE_RightClickBoxUp?.Invoke(IDs);
    public static void OE_ScrollBox_Go(List<uint> IDs, float amount)    => OE_ScrollBox?.Invoke(IDs, amount);
    public static void OE_LDragStart_Go(Vector2 pos, List<uint> IDs)    => OE_LDragStart?.Invoke(pos, IDs);
    public static void OE_LDragEnd_Go(Vector2 pos, List<uint> IDs)      => OE_LDragEnd?.Invoke(pos, IDs);
    public static void OE_RDragStart_Go(Vector2 pos, List<uint> IDs)    => OE_RDragStart?.Invoke(pos, IDs);
    public static void OE_RDragEnd_Go(Vector2 pos, List<uint> IDs)      => OE_RDragEnd?.Invoke(pos, IDs);
    public static void OE_HoverBoxStart_Go(List<uint> IDs)              => OE_HoverBoxStart?.Invoke(IDs);
    public static void OE_HoverBoxEnd_Go(List<uint> IDs)                => OE_HoverBoxEnd?.Invoke(IDs);
    public static void OE_KeyPress_Go(KeyCode k, List<uint> IDs)        => OE_KeyPress?.Invoke(k, IDs);
    public static void OE_KeyPressUp_Go(KeyCode k, List<uint> IDs)      => OE_KeyPressUp?.Invoke(k, IDs);
    public static void OE_InputString_Go(string s)                      => OE_InputString?.Invoke(s);

    //[][] Action
    public static void AE_ButtonActivate_Go(uint id)                   => AE_ButtonActivate?.Invoke(id);
    public static void AE_InputBoxActivate_Go(uint id, string text)    => AE_InputBoxActivate?.Invoke(id, text);

    //[][] Keys
    public static void KE_Subscribe_Go(KeyCode key, byte priority)                              => KE_Subscribe?.Invoke(key, priority);
    public static void KE_Unsubscribe_Go(KeyCode key, byte priority)                            => KE_Unsubscribe?.Invoke(key, priority);
    public static void KE_PrioritizedKeyPress_Go(byte priority, KeyCode key, List<uint> IDs)    => KE_PrioritizedKeyPress?.Invoke(priority, key, IDs);
    public static void KE_PrioritizedKeyPressUp_Go(byte priority, KeyCode key, List<uint> IDs)  => KE_PrioritizedKeyPressUp?.Invoke(priority, key, IDs);

    //[][] Camera
    public static void CE_EndOfCameraUpdate_Go()                        => CE_EndOfCameraUpdate?.Invoke();
    public static void CE_EndOfCameraSetup_Go()                         => CE_EndOfCameraSetup?.Invoke();
    public static void CE_CameraAspectChange_Go()                       => CE_CameraAspectChange?.Invoke();
    public static void CE_CameraYoink_Go()                              => CE_CameraYoink?.Invoke();

    //[][] Dialogue
    public static void DE_PerformDialogue_Go(int dialogueID, List<ushort> participantIDs)        => DE_PerformDialogue?.Invoke(dialogueID, participantIDs);
    #endregion
}
