using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] GameObject - Dialogue Box
//[][] Performs all tasks related to displaying and managing the dialogue box
public class NCGF_DIA_GO_DialogueBox : MonoBehaviour
{
    [SerializeField] private GameObject                     _textBoundingBox;   // Must have a BoxCollider2D component
    [SerializeField] private GO_Animation                   _boxBGAnim;         // Animated background
    [SerializeField] private GO_Animation                   _characterAnim;     // Animated character icon
    [SerializeField] private NCGF_DIA_GO_DiaText            _textBox;
    [SerializeField] private GameObject                     _controlDisplay;
    [SerializeField] private NCGF_DIA_GO_TextAgeIndicator   _txtAgeInd;
    [SerializeField] private NCGF_DIA_RG_CharacterInfo      _characterInfo;

    [SerializeField] private float              _openTime = 1f;
    [SerializeField] private float              _hardOpenTime = 0.15f;
    [SerializeField] private float              _moveOutOfWayYDist = 1f;
    [SerializeField] private float              _moveOutOfWaySpeed = 20f;
    [SerializeField] private float              _movementClampVal = 1;
    [SerializeField] private float              _closeEnoughY = 0.02f;
    [SerializeField] private Vector3            _upperDisplayLocalPos;
    [SerializeField] private Vector3            _lowerDisplayLocalPos;

    [SerializeField] private Vector3            _upperControlDisplayLocalPos;
    [SerializeField] private Vector3            _lowerControlDisplayLocalPos;
    [SerializeField] private bool               _doSlowHide = false;

    // Keeping
    private State                   _state;
    private Bounds                  _textBounds;
    private bool                    _isOnTop = true;
    private bool                    _isSetUp = false;
    private Vector3                 _thisLocalPos;
    private byte                    _keyPriority;
    private float                   _openClock = 0;
    private List<ushort>            _participants;

    // Dialogue State
    private List<DIA_O_Line>        _curDialogue = null;
    private int                     _curLineInd = 0;
    private int                     _furthestInd = 0;

    private ushort                  _prevChar   = ushort.MaxValue;
    private NCGF_Types.EmoteType    _prevEmote  = NCGF_Types.EmoteType.None;


    //[][] On Enable / Disable
    private void OnEnable()
    {
        NCGF_UI_S_Events.KE_PrioritizedKeyPress += HandleKeyPress;
    }
    private void OnDisable()
    {
        NCGF_UI_S_Events.KE_PrioritizedKeyPress -= HandleKeyPress;
    }

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }
    private void Update()
    {
        UpdateStatuses();
        ApproachGoToPosition();
    }

    //[][] Public Functions

    // TEMP
    public void PerformDialogue(List<DIA_O_Line> dialogue, List<ushort> participants)
    {
        if (!_isSetUp) return;
        if (dialogue == null) return;
        if (dialogue.Count == 0) return;
        if (participants == null) return;
        if (participants.Count == 0) return;

        _curDialogue = dialogue;
        _state = State.Opening;
        _curLineInd = 0;
        _furthestInd = 0;
        _participants = participants;

        LoadCharSprites();
    }
    public void SetToTopOrBottom(bool isOnTop)
    {
        _isOnTop = isOnTop;
        _thisLocalPos = (_isOnTop) ? _upperDisplayLocalPos : _lowerDisplayLocalPos;
        _thisLocalPos.y += (_state != State.Hiding) ? 0 : (_isOnTop) ? _moveOutOfWayYDist : -_moveOutOfWayYDist;
        transform.localPosition = _thisLocalPos;
        _controlDisplay.transform.localPosition = (_isOnTop) ? _upperControlDisplayLocalPos : _lowerControlDisplayLocalPos;
    }

    //[][] Private Functions
    private void Setup()
    {
        if (_isSetUp) return;

        // Text bounds
        if (_textBoundingBox == null)                                       goto S_TBB_Fail;
        var collider = _textBoundingBox.GetComponent<BoxCollider2D>();
        if (collider != null) { _textBounds = collider.bounds;              goto S_TBB_Succeed;}

        S_TBB_Fail: 
            Debug.Log("NCGF_DIA_GO_DialogueBox.Setup: Dialogue Box Setup Fail! No text bounding box with collider used.");
            return;
    S_TBB_Succeed:
        // End Text Bounds

        var pos = transform.localPosition;
        transform.localPosition = new Vector3(pos.x, pos.y, NCGF_UI_R_Parameters._UIstandardZ + NCGF_UI_R_Parameters._dialogueBoxZOffset);
        _thisLocalPos = transform.localPosition;
        _textBox.Setup(_textBounds);

        _keyPriority = NCGF_UI_R_Parameters._dialogueBoxPriority;
        _state = State.Hiding;
        SetToTopOrBottom(_isOnTop);
        _characterAnim._allFrames = null;

        _isSetUp = true;
    }

    private void UpdateStatuses()
    {
        if (_state == State.Opening)
        {
            _openClock += Time.deltaTime;
            if (_openClock >= ((_doSlowHide) ? _openTime : _hardOpenTime)) SwitchToDisplaying();
        }
    }
    private float AGTP_upBaseY;
    private void ApproachGoToPosition()
    {
        AGTP_upBaseY = (_isOnTop) ? _upperDisplayLocalPos.y : _lowerDisplayLocalPos.y;
        AGTP_upBaseY += (_state == State.Hiding) ? ((_isOnTop) ? _moveOutOfWayYDist : -_moveOutOfWayYDist) : 0;
        _thisLocalPos.y = Mathf.Lerp(_thisLocalPos.y, AGTP_upBaseY,
            (_doSlowHide) ?
            Time.deltaTime * _moveOutOfWaySpeed * Mathf.Clamp(Mathf.Abs(_thisLocalPos.y - AGTP_upBaseY), 0, _movementClampVal)
            : 1);
        if (Mathf.Abs(_thisLocalPos.y - AGTP_upBaseY) < _closeEnoughY) _thisLocalPos.y = AGTP_upBaseY;

        transform.localPosition = _thisLocalPos;
    }

    // Keys
    private void OnSpace()
    {
        _textBox.ForceRevealPublic();
    }
    private void OnLeftArrow()
    {
        UnadvanceDialogue();
    }
    private void OnRightArrow()
    {
        AdvanceDialogue();
    }
    private void OnReturnKey()
    {
        AdvanceDialogue();
    }

    // Operations
    private void AdvanceDialogue()
    {
        if (_curDialogue == null) return;
        _curLineInd++;
        if (_curDialogue.Count == _curLineInd) { CompleteDialogue(); return; }

        if (_curLineInd == _furthestInd) _txtAgeInd.UpdateStatus(false);
        else if (_curLineInd > _furthestInd) _furthestInd = _curLineInd;


        PrintText();
    }
    private void UnadvanceDialogue()
    {
        if (_curDialogue == null) return;
        if (_curLineInd == 0) return;
        _curLineInd--;

        _txtAgeInd.UpdateStatus(true);

        PrintText();
    }
    private void PrintText()
    {
        // Write text
        var res = _textBox.WriteAndReport(_curDialogue[_curLineInd]);
        if (res != null)
        {
            AdjustDialogueToFitSplitLines(res, _curLineInd);
        }

        LoadCharSprites();
    }
    private void AdjustDialogueToFitSplitLines(List<DIA_O_Line> splitLine, int splitLineIndex)
    {
        if (_curDialogue == null) return;
        if (splitLine == null) return;
        if (splitLineIndex < 0 || splitLineIndex >= _curDialogue.Count) return;
        if (splitLine.Count != 2) { Debug.Log($"DIA_GO_DB_ADTFSL: Wrong split line list count! Count of {splitLine.Count} when 2 is required."); return; }

        List<DIA_O_Line> newLineList = new List<DIA_O_Line>();
        newLineList.AddRange(_curDialogue.GetRange(0, splitLineIndex));
        newLineList.AddRange(splitLine);
        if (splitLineIndex < _curDialogue.Count - 1) newLineList.AddRange(
            _curDialogue.GetRange(splitLineIndex + 1, _curDialogue.Count - splitLineIndex - 1));

        var old = _curDialogue;
        _curDialogue = newLineList;
        old.Clear();
    }
    private void LoadCharSprites()
    {
        int curIndex = _curDialogue[_curLineInd]._characterIndex;
        if (_participants.Count <= curIndex || curIndex < 0) curIndex = 0;

        var curChar = _participants[curIndex];
        var curEmote = _curDialogue[_curLineInd]._emotion;

        // Get character sprites
        List<Sprite> sprites = null;
        if (_characterInfo != null && _characterAnim != null)
        {
            sprites = _characterInfo.GetCharSprites(curChar, curEmote);
        }

        if (sprites != null) { _characterAnim._allFrames = sprites; }
        else
        {
            // TEMP
            _characterAnim._allFrames = null;
        }

        // Force quick update on change of character or emotion
        if (curChar != _prevChar || curEmote != _prevEmote) _characterAnim.ForceUpdateFrame();
        _prevChar = curChar;
        _prevEmote = curEmote;
    }
    private void CompleteDialogue()
    {
        _curLineInd = 0;
        _curDialogue = null;
        _state = State.Hiding;
    }
    private void SwitchToDisplaying()
    {
        SubscribeToKeys();
        _state = State.Displaying;
        _curLineInd = -1;
        AdvanceDialogue();
    }

    // Subscribes
    private void SubscribeToKeys()
    {
        NCGF_UI_S_Events.KE_Subscribe_Go(KeyCode.RightArrow, _keyPriority);
        NCGF_UI_S_Events.KE_Subscribe_Go(KeyCode.LeftArrow, _keyPriority);
        NCGF_UI_S_Events.KE_Subscribe_Go(KeyCode.Space, _keyPriority);
        NCGF_UI_S_Events.KE_Subscribe_Go(KeyCode.Return, _keyPriority);
    }
    private void UnsubscribeToKeys()
    {
        NCGF_UI_S_Events.KE_Unsubscribe_Go(KeyCode.RightArrow, _keyPriority);
        NCGF_UI_S_Events.KE_Unsubscribe_Go(KeyCode.LeftArrow, _keyPriority);
        NCGF_UI_S_Events.KE_Unsubscribe_Go(KeyCode.Space, _keyPriority);
        NCGF_UI_S_Events.KE_Unsubscribe_Go(KeyCode.Return, _keyPriority);
    }

    // Handle Events
    private void HandleKeyPress(byte priority, KeyCode key, List<uint> ID)
    {
        if (_curDialogue == null) { UnsubscribeToKeys(); return; }

        if (priority > _keyPriority) return;

        switch (key)
        {
            case KeyCode.RightArrow:
                OnRightArrow();
                break;

            case KeyCode.LeftArrow:
                OnLeftArrow();
                break;

            case KeyCode.Space:
                OnSpace();
                break;

            case KeyCode.Return:
                OnReturnKey();
                break;

            default:
                break;
        }
    }

    //[][] Enums
    private enum State : byte
    {
        None = 0,
        Opening = 1,
        Displaying = 2,
        Hiding = 3,
    }

}
