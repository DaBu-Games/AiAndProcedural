using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private FieldOfView fov;
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer objectRenderer;

    private readonly BlackBoard _blackBoard = new BlackBoard();
    private BlackBoardKey _canSeePlayerKey;
    
    private BehaviourTree _tree;

    private void Awake()
    {
        fov.Initialize(_blackBoard);
        _canSeePlayerKey = _blackBoard.GetOrRegisterKey("canSeePlayer");
    }

    private void Start()
    {
        _tree = new BehaviourTree("guardTree");
        
        /*
        SequenceNode isSafeSequence = new SequenceNode("IsSafeSequence");
        
        bool IsSafe()
        {
            if (_blackBoard.TryGetValue(_canSeePlayerKey, out bool canSeePlayer)) {
                return !canSeePlayer;
            }
            
            return true;
        }
        
        isSafeSequence.AddChild(new LeafNode("isSafeCondition", new ConditionStrategy(IsSafe)));
        isSafeSequence.AddChild(new LeafNode("IsSafeToComeOut", new ActionStrategy(() => objectRenderer.enabled = true)));
        
        SelectorNode isSafeSelector = new SelectorNode("IsSafeSelector");
        isSafeSelector.AddChild(isSafeSequence);
        isSafeSelector.AddChild(new LeafNode("Setactivefalse", new ActionStrategy( () => objectRenderer.enabled = false)));
        */
        
        SequenceNode patrolSequence = new SequenceNode("PatrolSequence");

        foreach (var patrolPoint in patrolPoints)
        {
            patrolSequence.AddChild(new LeafNode(
                $"WalkToPoint_x{patrolPoint.position.x:F1}_z{patrolPoint.position.z:F1}", 
                    new MoveStrategy(patrolPoint, this.transform, agent)
            ));
        }
        
        _tree.AddChild(patrolSequence);
    }

    private void Update()
    {
        _tree.Process();
        
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
            if (_blackBoard.TryGetValue(_isSafeKey, out bool isSafe)) {
                _blackBoard.SetValue(_isSafeKey, !isSafe);
                Debug.Log($"IsSafe: {isSafe}");
            }
        }
        */
    }
}
