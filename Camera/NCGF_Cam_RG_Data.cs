using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[][] All - Camera Data
//[][] Contains some aspects of camera operation
public class NCGF_Cam_RG_Data : MonoBehaviour
{
    public float _cameraOriginalSize                = 0;

    public float _defaultScale                      = 9.0f;
    public float _camSpeed                          = 18.0f;    // Base speed using WASD + arrows
    public float _camAdherence                      = 10.0f;
    public float _dragSensitivity                   = 0.1f;     // Base speed on drag
    public float _scrollSensitivity                 = -0.15f;   // Base speed of scrolling zoom
    public float _scrollAdherence                   = 7.0f;
    public float _camSizeImpactOnSpeed              = 1.0f;
    public float _camSizeImpactOnScrollSensitivity  = 1.0f;
    public float _camSizeImpactOnDragSensitivity    = 20.0f;
    public float _maximumAdherence                  = 0.9f;     // Wins against minimum adherence
    public float _minimumAdherence                  = 0.0f;
    public float _maximumScale                      = 30.0f;    // Largest zoom-out amount
    public float _minimumScale                      = 1.0f;     // Smallest ''''''''''''''
    public float _leftShiftSlowdown                 = 2.0f;     // Calculated speed /= (1+val)

    public float _cameraZ                           = -10;
    public float _cameraClickBoxWidthHeight         = 1000;
    public float _cameraClickBoxZPos                = -2;
    public float _worldSize                         = 999999;
}
