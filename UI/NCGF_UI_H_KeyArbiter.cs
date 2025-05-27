using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Handler - Key Arbiter
//[][] Handles the operation and distribution of prioritized key presses

public class NCGF_UI_H_KeyArbiter : MonoBehaviour
{
    private List<NCGF_KeyArbiterListItem> _keyPriorities = new List<NCGF_KeyArbiterListItem>();

    private bool _isSetUp = false;

    //[][] On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.OE_KeyPress += HandleKeyPress;
        NCGF_UI_S_Events.OE_KeyPressUp += HandleKeyPressUp;
        NCGF_UI_S_Events.KE_Subscribe += AddPriority;
        NCGF_UI_S_Events.KE_Unsubscribe += RemovePriority;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.OE_KeyPress -= HandleKeyPress;
        NCGF_UI_S_Events.OE_KeyPressUp -= HandleKeyPressUp;
        NCGF_UI_S_Events.KE_Subscribe -= AddPriority;
        NCGF_UI_S_Events.KE_Unsubscribe -= RemovePriority;
    }

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }

    //[][] Public Functions

    //[][] Private Functions
    // Handle
    private void HandleKeyPress(KeyCode k, List<uint> IDs)
    {
        Setup();
        NCGF_UI_S_Events.KE_PrioritizedKeyPress_Go(DeterminePriority(k), k, IDs);
    }
    private void HandleKeyPressUp(KeyCode k, List<uint> IDs)
    {
        Setup();
        NCGF_UI_S_Events.KE_PrioritizedKeyPressUp_Go(DeterminePriority(k), k, IDs);
    }
    private byte DeterminePriority(KeyCode k)
    {
        byte priority = GetHighestPriority(k, out bool fail);
        byte priorityGeneric = GetHighestPriority(KeyCode.None, out bool genericFail);

        if (!genericFail) if (priorityGeneric > priority || fail) priority = priorityGeneric;
        return priority;
    }
    private void AddPriority(KeyCode key, byte priority)
    {
        Setup();

        NCGF_KeyArbiterListItem keyItem;
        NCGF_KeyArbiterPriorityItem priorityItem;

        int i = GetIndexOfKey(key);
        if (i == -1)
        {
            keyItem = new NCGF_KeyArbiterListItem(key);
            _keyPriorities.Add(keyItem);
        }
        else keyItem = _keyPriorities[i];

        i = GetIndexOfPriorityItem(keyItem, priority);
        if (i == -1)
        {
            priorityItem = new NCGF_KeyArbiterPriorityItem(priority);
            keyItem._priorities.Add(priorityItem);
        }
        else priorityItem = keyItem._priorities[i];

        priorityItem._subscribers++;
    }
    private byte GetHighestPriority(KeyCode key, out bool fail)
    {
        Setup();

        NCGF_KeyArbiterListItem keyItem;

        int i = GetIndexOfKey(key);
        if (i == -1) { fail = true; return byte.MaxValue; }
        keyItem = _keyPriorities[i];
        var priorities = keyItem._priorities;

        byte winningPriority = 0;
        for (int q = 0; q < priorities.Count; q++)
        {
            if (priorities[q]._priority > winningPriority) winningPriority = priorities[q]._priority;
        }

        fail = false;
        return winningPriority;
    }
    private void RemovePriority(KeyCode key, byte priority)
    {
        Setup();
        
        NCGF_KeyArbiterListItem keyItem;
        NCGF_KeyArbiterPriorityItem priorityItem;

        int i, j;
        i = GetIndexOfKey(key);
        if (i == -1) return;
        keyItem = _keyPriorities[i];
        j = GetIndexOfPriorityItem(keyItem, priority);
        if (j == -1) return;
        priorityItem = keyItem._priorities[j];

        priorityItem._subscribers--;
        if (priorityItem._subscribers == 0) keyItem._priorities.RemoveAt(j);
        if (keyItem._priorities.Count == 0) _keyPriorities.RemoveAt(i);
    }
    private void Setup()
    {
        if (_isSetUp) return;
        // Vestigial
        _isSetUp = true;
    }
    private int GetIndexOfKey(KeyCode k)
    {
        for (int i = 0; i < _keyPriorities.Count; i++)
        {
            if (_keyPriorities[i]._keyCode == k) return i;
        }
        return -1;
    }
    private int GetIndexOfPriorityItem(NCGF_KeyArbiterListItem keyItem, byte priority)
    {
        for (int i = 0; i < keyItem._priorities.Count; i++)
        {
            if (keyItem._priorities[i]._priority == priority) return i;
        }
        return -1;
    }
}

public struct NCGF_KeyArbiterListItem
{
    public KeyCode _keyCode;
    public List<NCGF_KeyArbiterPriorityItem> _priorities;

    public NCGF_KeyArbiterListItem(KeyCode keyCode)
    {
        _keyCode = keyCode;
        _priorities = new List<NCGF_KeyArbiterPriorityItem>();
    }
}
public class NCGF_KeyArbiterPriorityItem
{
    public byte _priority;
    public ushort _subscribers;

    public NCGF_KeyArbiterPriorityItem(byte priority)
    {
        _priority = priority;
        _subscribers = 0;
    }
}
