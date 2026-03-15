using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private FieldOfView fov;
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject weapon;
    [SerializeField] private TextMeshProUGUI textBox;

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
        SequenceNode patrolSequence = new SequenceNode("patrolSequence");
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
                "walkToPoint_X" + patrolPoint.position.x + "Z" + patrolPoint.position.z, 
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
        followPlayer.AddChild(new LeafNode("followPlayer", new FollowStrategy(_blackBoard, transform, agent)));
        
        //Get weapon sequence
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
        UpdateUI();
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
            Debug.Log(_tree.GetCurrentNode().Name);
        }
        */
    }

    private void UpdateUI()
    {
        string currentNodeName = _tree.GetCurrentNode().Name;
        if (textBox.text != currentNodeName)
        {
            textBox.text = AddSpacesToSentence(currentNodeName);
        }
            
    }
    
    private string AddSpacesToSentence(string text)
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder();
    
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            if (i > 0 && char.IsUpper(c))
            {
                result.Append(' ');
            }
            
            if (c == '_')
            {
                result.Append('\n');
            }
            else
            {
                result.Append(c);
            }
        }
    
        return result.ToString();
    }
}
