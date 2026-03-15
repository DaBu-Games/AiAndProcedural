using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private FieldOfView fov;
    [SerializeField] private Transform player;
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject weapon;

    private readonly BlackBoard _blackBoard = new BlackBoard();
    private BlackBoardKey _canSeePlayerKey;
    private BlackBoardKey _lastSeePlayerPosKey;
    private BlackBoardKey _hasWeaponKey;
    
    private BehaviourTree _tree;

    private void Awake()
    {
        fov.Initialize(_blackBoard);
        _canSeePlayerKey = _blackBoard.GetOrRegisterKey("canSeePlayer");
        _lastSeePlayerPosKey = _blackBoard.GetOrRegisterKey("lastSeePlayerPos");
        _hasWeaponKey = _blackBoard.GetOrRegisterKey("hasWeapon");
        _blackBoard.SetValue(_hasWeaponKey, false);
    }

    private void Start()
    {
        _tree = new BehaviourTree("guardTree");
        
        //Patrol sequence
        // check if the lastseeplayerpos is null if it is patrol
        SequenceNode patrolSequence = new SequenceNode("PatrolSequence");
        bool HasNoPlayerPos()
        {
            if (_blackBoard.TryGetValue(_lastSeePlayerPosKey, out Vector3 playerPos) && playerPos != Vector3.zero) {
                //Debug.Log(playerPos);
                return false;
            }
            
            //Debug.Log("No player pos");
            return true;
        }
        patrolSequence.AddChild(new LeafNode("hasLastPlayerPos", new ConditionStrategy(HasNoPlayerPos)));
        foreach (var patrolPoint in patrolPoints)
        {
            patrolSequence.AddChild(new LeafNode(
                $"WalkToPoint_x{patrolPoint.position.x:F1}_z{patrolPoint.position.z:F1}", 
                new MoveStrategy(patrolPoint.position, this.transform, agent)
            ));
        }
        
        // follow player sequence
        SequenceNode followPlayer = new SequenceNode("followPlayerSequence");
        bool HasWeapon()
        {
            if (_blackBoard.TryGetValue(_hasWeaponKey, out bool hasWeapon) && hasWeapon) {
                //Debug.Log("has weapon");
                return true;
            }
            
            //Debug.Log("no weapon");
            return false;
        }
        followPlayer.AddChild(new LeafNode("hasWeaponCondition", new ConditionStrategy(HasWeapon)));
        followPlayer.AddChild(new LeafNode("followPlayerMove", new FollowStrategy(_blackBoard, transform, agent)));
        
        SequenceNode getWeaponSequence = new SequenceNode("getWeaponSequence");
        getWeaponSequence.AddChild(new LeafNode("moveToWeapon", new MoveStrategy(weapon.transform.position, this.transform, agent)));
        getWeaponSequence.AddChild(new LeafNode("pickUpWeapon", new ActionStrategy( () =>
            {
                _blackBoard.SetValue(_hasWeaponKey, true);
                weapon.SetActive(false);
            }
        )));
        
        // root selector
        SelectorNode rootSelector = new SelectorNode("rootSelector");
        rootSelector.AddChild(patrolSequence);
        rootSelector.AddChild(followPlayer);
        rootSelector.AddChild(getWeaponSequence);
        
        _blackBoard.SubScribe(_canSeePlayerKey, () =>
        {
            if (patrolSequence.Process() == NodeStatus.Running)
            {
                Debug.Log("Reset patrol");
                patrolSequence.Reset();
            }
        });
        
        _tree.AddChild(rootSelector);
    }

    private void Update()
    {
        _tree.Process();
        
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
            _tree.PrintTree();
        }*/
    }
}
