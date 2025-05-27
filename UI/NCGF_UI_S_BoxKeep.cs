using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] UI - Static - ClickBox Keeper
//[][] Stores and manages clickboxes for actuation of mouse clicks

//[][] NOTES:
//[][] - For overlapping boxes, the box with the LOWEST value of priority is chosen.
//[][] - This is to allow a GameObject's z-axis to be used as priority, as lower z-axis values are "on top".

public static class NCGF_UI_S_BoxKeep
{
    //[][] Variables
    private static List<O_ClickBox> _cBoxList = new List<O_ClickBox>();

    //[][] Keeping
    private static uint _curID = 1;


    //[][] Public Functions
    public static O_ClickBox MakeAndReturnBox(Vector2 center, float halfWidth, float halfHeight, float priority)
    {
        O_ClickBox retVal;

        if (NCGF_Res._pools.Has(typeof(O_ClickBox)))
        {
            retVal = (O_ClickBox)NCGF_Res._pools.Obtain(typeof(O_ClickBox), true);
            if (retVal != null)
            {
                retVal.Setup(center, halfWidth, halfHeight, _curID++, priority);
                goto MARB_Created;
            }
        }
        retVal = new O_ClickBox(center, halfWidth, halfHeight, _curID++, priority);

    MARB_Created:
        _cBoxList.Add(retVal);
        return retVal;
    }
    public static void RemoveBox(uint ID)
    {
        O_ClickBox cur;

        for (int i = 0; i < _cBoxList.Count; i++)
        {
            cur = _cBoxList[i];
            if (cur == null) { Debug.Log("NCGF_UI_S_BoxKeep.RemoveBox: Null list entrant!"); continue; }
            if (cur._ID == ID)
            {
                _cBoxList.RemoveAt(i);
                cur._isActive = false;
                cur._ID = 0;
                NCGF_Res._pools.Pool(cur);
                return;
            }
        }
    }

    public static List<O_ClickBox> GetTopBoxesAt(Vector2 point)
    {
        // Returns a list containing the topmost hard ClickBox and any soft ClickBoxes above it, in that order

        O_ClickBox hard = null;
        List<O_ClickBox> retVal = new List<O_ClickBox>();
        List<O_ClickBox> options = new List<O_ClickBox>();

        float aof = NCGF_UI_D_InputOptions._cursorSquareAOF;
        if (aof <= 0f)
            for (int i = 0; i < _cBoxList.Count; i++) { if (NCGF_Calc.IsPointInBox(point, _cBoxList[i])) options.Add(_cBoxList[i]); }
        else
            for (int i = 0; i < _cBoxList.Count; i++) { if (NCGF_Calc.IsPointWithSquareAOFInBox(point, aof, _cBoxList[i])) options.Add(_cBoxList[i]); }

        if (options.Count == 0) return retVal;

        for (int i = options.Count - 1; i >= 0; i--)
        {
            if (options[i]._round) if (!NCGF_Calc.IsPointInCircle(point, options[i])) options.RemoveAt(i);
        }

        float curPriority = float.MaxValue;
        O_ClickBox cur;
        for (int i = options.Count - 1; i >= 0; i--)
        {
            cur = options[i];
            if (!cur._isActive || cur._soft) continue;
            if (cur._priority < curPriority) { hard = cur; curPriority = hard._priority; }
        }
        if (hard != null) retVal.Add(hard);

        for (int i = options.Count - 1; i >= 0; i--)
        {
            cur = options[i];
            if (!cur._isActive) continue;
            if (cur._soft)
            {
                if (hard == null) { retVal.Add(cur); continue; }
                if (cur._priority <= hard._priority) retVal.Add(cur);
            }
        }

        return retVal;
    }
    public static void ReportState()
    {
        string s = "NCGF_UI_S_BoxKeep STATUS REPORT:";
        foreach (var x in _cBoxList)
        {
            s += $"\nBox with ID: {x._ID} has state: ISACTIVE={x._isActive} PRIORITY={x._priority:0.0000} SOFT={x._soft}";
        }
        Debug.Log(s);
    }
}
