using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCGF_Cam_GO_Follower : MonoBehaviour
{
    // References
    [SerializeField] private Camera             _camera;
    [SerializeField] private NCGF_Cam_RG_Data   _data;

    private Transform _cameraTransform;

    // Technical
    private float   _scaleThisFrame;
    private Vector3 _originalScale;

    // On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.CE_EndOfCameraSetup    += OnEndCameraSetup;
        NCGF_UI_S_Events.CE_EndOfCameraUpdate   += OnEndCameraUpdate;
        NCGF_UI_S_Events.CE_CameraYoink         += OnEndCameraUpdate;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.CE_EndOfCameraSetup    -= OnEndCameraSetup;
        NCGF_UI_S_Events.CE_EndOfCameraUpdate   -= OnEndCameraUpdate;
        NCGF_UI_S_Events.CE_CameraYoink         -= OnEndCameraUpdate;
    }

    // Private Functions
    private void OnEndCameraSetup()
    {
        if (_camera == null) _camera = NCGF_Res._camera;
        if (_data == null) _data = NCGF_Res._camData;

        if (_camera != null) _cameraTransform = _camera.gameObject.transform;
        _originalScale = transform.localScale * (_data._defaultScale / _data._cameraOriginalSize);
    }
    private void OnEndCameraUpdate()
    {
        if (_cameraTransform == null || _data == null) return;

        _scaleThisFrame         = _camera.orthographicSize / _data._defaultScale;
        transform.position      = new Vector3(_cameraTransform.position.x, _cameraTransform.position.y, transform.position.z);
        transform.localScale    = new Vector3(_scaleThisFrame * _originalScale.x, _scaleThisFrame * _originalScale.y, 1);
        transform.rotation      = _cameraTransform.rotation;
    }
}
