using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NCGF - Objects
// Script containing non-MonoBehaviour objects and structs used across NCGF

// Objects
public class O_ClickBox
{
    public float _centerX, _centerY, _halfWidth, _halfHeight;

    public uint     _ID         = 0;
    public float    _priority   = 0;
    public bool     _isActive   = false;
    public bool     _soft       = false;    // Soft ClickBoxes do not prevent ClickBoxes below them from activating
    public bool     _round      = false;
    public O_ClickBox(Vector2 center, float halfWidth, float halfHeight, uint ID, float priority)
        { Setup(center, halfWidth, halfHeight, ID, priority); }
    public void Setup(Vector2 center, float halfWidth, float halfHeight, uint ID, float priority)
    {
        _centerX    = center.x;
        _centerY    = center.y;
        _halfWidth  = halfWidth;
        _halfHeight = halfHeight;
        _ID         = ID;
        _priority   = priority;
        _isActive   = false;
    }
}

// Structs
public struct Int2
{
    public int _x;
    public int _y;
    public Int2(int x, int y) { _x = x; _y = y; }
    public static Int2 operator +(Int2 a, Int2 b) => new Int2(a._x + b._x, a._y + b._y);
    public static Int2 operator -(Int2 a, Int2 b) => new Int2(a._x - b._x, a._y - b._y);
    public static bool operator ==(Int2 a, Int2 b) => (a._x == b._x && a._y == b._y);
    public static bool operator !=(Int2 a, Int2 b) => (a._x != b._x || a._y != b._y);
    public override bool Equals(object x)
    {
        if (!(x is Int2)) return false;
        return (Int2)x == this;
    }
    public override int GetHashCode()
    {
        return _x + (_y << 16 + _y >> 16);
    }
    public override string ToString()
    {
        return $"({_x}, {_y})";
    }
}
public struct LineSegment
{
    public Vector2 _p1;
    public Vector2 _p2;
    public LineSegment(Vector2 p1, Vector2 p2) { _p1 = p1; _p2 = p2; }
    public override string ToString()
    {
        return $"({_p1.x:0.000}, {_p1.y:0.000}) -> ({_p2.x:0.000}, {_p2.y:0.000})";
    }
}

//
public class NCGF_Objects
{
}
