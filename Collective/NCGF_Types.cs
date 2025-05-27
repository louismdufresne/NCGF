using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Types
//[][] Contains enums for various systems
public static class NCGF_Types
{
    #region Dialogue
    public enum EmoteType : byte
    {
        None = 0,
        Neutral = 1,
        Looking_Off = 2,
        Side_Eying = 3,

        Pleased = 10,
        Happy = 11,
        Elated = 12,
        Manic = 13,

        Wistful = 20,
        Sad = 21,
        Mourning = 22,
        Crying = 23,

        Annoyed = 30,
        Angry = 31,
        Enraged = 32,
        Insane = 33,

        Concerned = 40,
        Afraid = 41,
        Fearful = 42,

        Nonplused = 50,
        Confused = 51,
        Angfused = 52,
        Disconnected = 53,
        Disoriented = 54,

        Disdainful = 60,
        Disgusted = 61,
        Hateful = 62,

        Custom_1 = 180,
        Custom_2 = 181,
        Custom_3 = 182,
        Custom_4 = 183,
        Custom_5 = 184,

        Humbered = 250,
        Naged = 251,
        Dorceless = 252,
        Kyned = 253,
    }
    public enum FormatType : byte
    {
        None = 0,
        Italics = 1,
        Bold = 2,
        Underline = 3,

        Color = 10,
        Shake = 11,
        Wave = 12,
        Vibrate = 13,
    }

    #endregion
}
