using System;
using System.Collections.Generic;
using UnityEngine;

public class GO_UIPanel : MonoBehaviour
{

    private class SubscribedAction
    {
        public string       _buttonName = "";
        public List<Action> _toCall = new();
    }
}
