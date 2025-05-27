using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] UI Parameters
//[][] Values for UI operation
public static class NCGF_UI_R_Parameters
{
    //[][] VALUES

    // Z-Values
    public static readonly float    _UIstandardZ                    = -1.0f;
    public static readonly float    _dialogueBoxZOffset             = -0.001f;
    public static readonly float    _imageZOffset                   = -0.002f;
    public static readonly float    _buttonZOffset                  = -0.003f;
    public static readonly float    _textZOffset                    = -0.005f;

    public static readonly float    _pauseScreenZ                   = -5.0f;

    // Text
    public static readonly int      _textOutlineInPixels            = 1;
    public static readonly float    _textOutlineZOffset             = 0.001f;
    public static readonly int[,]   _textOutlinePositionsInPx       = { { -1, -1 },{ -1, 0 },{ -1, 1 },{ 0, -1 },{ 0, 1 },{ 1, -1 },{ 1, 0 },{ 1, 1 } };

    // UI Elements
    public static readonly float    _windowMinZSeparation           = 0.02f;
    public static readonly float    _stackedElementMinZSeparation   = 0.001f;

    // Key priorities
    public static readonly byte     _cameraKeyPriority              = 100;
    public static readonly byte     _inputBoxKeyPriority            = 105;
    public static readonly byte     _dialogueBoxPriority            = 110;

    // Dialogue
    public static readonly Color32  _defaultTextColor               = Color.white;
}
