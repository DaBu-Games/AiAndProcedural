using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestBlackBoard : MonoBehaviour
{
    [SerializeField] private GameObject testObject;

    private readonly BlackBoard _blackBoard = new BlackBoard();

    private BlackBoardKey _isSafeKey;
    private BehaviourTree _tree;

    private void Awake()
    {
        _isSafeKey = _blackBoard.GetOrRegisterKey("IsTested");
        _blackBoard.SetValue(_isSafeKey, true);

        if (_blackBoard.TryGetValue(_isSafeKey, out bool isSafe))
        {
            Debug.Log($"Is Safe: {isSafe}");
        }
    }

    private void Start()
    {
        _tree = new BehaviourTree("guardTree");
        
        SequenceNode isSafeSequence = new SequenceNode("IsSafeSequence");
        bool IsSafe()
        {
            if (_blackBoard.TryGetValue(_isSafeKey, out bool isSafe)) {
                if (isSafe) {
                    return true;
                }
            }
            
            isSafeSequence.Reset();
            return false;
        }
        isSafeSequence.AddChild(new LeafNode("isSafeCondition", new ConditionStrategy(IsSafe)));
        isSafeSequence.AddChild(new LeafNode("IsSafeMove", new ActionStrategy(() => {
                Vector3 pos = testObject.transform.position;
                testObject.transform.position = new Vector3(
                    pos.x + 0.005f, 
                    pos.y, 
                    pos.z
                );
                testObject.SetActive(true);
                Debug.Log($"Moved object to: {testObject.transform.position}");
            })
        ));
        
        SelectorNode isSafeSelector = new SelectorNode("IsSafeSelector");
        isSafeSelector.AddChild(isSafeSequence);
        isSafeSelector.AddChild(new LeafNode("setactivefalse", new ActionStrategy( () => testObject.SetActive(false) )));
        
        _tree.AddChild(isSafeSelector);
    }

    private void Update()
    {
        _tree.Process();
        
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
            if (_blackBoard.TryGetValue(_isSafeKey, out bool isSafe)) {
                _blackBoard.SetValue(_isSafeKey, !isSafe);
                Debug.Log($"IsSafe: {isSafe}");
            }
        }
    }
}
