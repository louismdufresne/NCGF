using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// UI Handler - Input Collector
// Collects inputs using Unity's default Input system, and fires appropriate events
public class NCGF_UI_H_InputCollector : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private KeyCode[] values;

    private void Awake()
    {
        values = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }
    void Update()
    {
        NCGF_UI_S_Events.IE_MousePos_Go(_camera.ScreenToWorldPoint(Input.mousePosition));

        if (Input.GetMouseButton(0)) NCGF_UI_S_Events.IE_LeftHold_Go();
        if (Input.GetMouseButton(1)) NCGF_UI_S_Events.IE_RightHold_Go();
        NCGF_UI_S_Events.IE_Scroll_Go(Input.mouseScrollDelta.y);
        if (Input.anyKey)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (Input.GetKey(values[i])) NCGF_UI_S_Events.IE_KeyHold_Go(values[i]);
            }
        }
        NCGF_UI_S_Events.IE_InputString_Go(Input.inputString);

        NCGF_UI_S_Events.IE_RunFrame_Go();
    }
}
