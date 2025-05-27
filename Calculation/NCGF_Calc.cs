using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// NCGF - Calculator
// Provides implementation for various functions pertaining to operation of modules in NCGF
public static class NCGF_Calc
{
    // Public Functions
    public static bool IsPointInBox(Vector2 point, O_ClickBox box)
    {
        return
               (point.y <= box._centerY + box._halfHeight)
            && (point.y >= box._centerY - box._halfHeight)
            && (point.x <= box._centerX + box._halfWidth)
            && (point.x >= box._centerX - box._halfWidth);
    }
    public static bool IsPointWithSquareAOFInBox(Vector2 point, float squareAOF, O_ClickBox box)
    {
        // Does the box centered at 'point' with sides of half-length 'squareAOF' overlap the given clickBox
        return 
               (point.x - squareAOF <= box._centerX + box._halfWidth)
            && (point.y - squareAOF <= box._centerY + box._halfHeight)
            && (point.x + squareAOF >= box._centerX - box._halfWidth)
            && (point.y + squareAOF >= box._centerY - box._halfHeight);
    }
    private static float IIC_rad;
    public static bool IsPointInCircle(Vector2 point, O_ClickBox box)
    {
        IIC_rad = box._halfHeight;
        if (box._halfWidth < IIC_rad) IIC_rad = box._halfWidth;
        return (Vector2.Distance(point, new Vector2(box._centerX, box._centerY)) < IIC_rad);
    }
    public static int RandIntExcept(int minInclusive, int maxExclusive, int except)
    {
        int retVal = Random.Range(minInclusive, maxExclusive - 1);
        if (retVal >= except) retVal++;
        return retVal;
    }
}
