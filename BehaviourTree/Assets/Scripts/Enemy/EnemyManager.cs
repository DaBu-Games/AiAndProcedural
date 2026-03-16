using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private FieldOfView fov;
    [SerializeField] private float attackDmg;
    [SerializeField] private float attackCooldown;
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject weapon;
    [SerializeField] private TextMeshProUGUI textBox;

    private readonly BlackBoard _blackBoard = new BlackBoard();
    private BlackBoardKey _canSeePlayerKey;
    private BlackBoardKey _lastSeePlayerPosKey;
    private BlackBoardKey _hasWeaponKey;
    private BlackBoardKey _isPlayerAliveKey;
    
    private BehaviourTree _tree;

    private void Awake()
    {
        fov.Initialize(_blackBoard);
        _canSeePlayerKey = _blackBoard.GetOrRegisterKey("canSeePlayer");
        _lastSeePlayerPosKey = _blackBoard.GetOrRegisterKey("lastSeePlayerPos");
        _hasWeaponKey = _blackBoard.GetOrRegisterKey("hasWeapon");
        _isPlayerAliveKey = _blackBoard.GetOrRegisterKey("isPlayerAlive");
        _blackBoard.SetValue(_hasWeaponKey, false);
        _blackBoard.SetValue(_isPlayerAliveKey, true);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDeath += () => _blackBoard.SetValue(_isPlayerAliveKey, false);
        GameEvents.OnPlayerDeath += () => fov.ResetValues();
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDeath -= () => _blackBoard.SetValue(_isPlayerAliveKey, false);
        GameEvents.OnPlayerDeath -= () => fov.ResetValues();
    }

    private void Start()
    {
        _tree = new BehaviourTree("guardTree");
        
        // root selector
        SelectorNode rootSelector = new SelectorNode("rootSelector");
        SequenceNode patrolSequence = PatrolSequence();
        rootSelector.AddChild(patrolSequence);
        rootSelector.AddChild(GetWeaponSequence());
        rootSelector.AddChild(FollowPlayerSequence());
        
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
    }

    private SequenceNode PatrolSequence()
    {
        SequenceNode patrolSequence = new SequenceNode("patrolSequence");
        patrolSequence.AddChild(new LeafNode("hasNoPlayerPos", new ConditionStrategy( 
            () => _blackBoard.IsValueEqualTo(_lastSeePlayerPosKey, Vector3.zero) 
        )));
        foreach (var patrolPoint in patrolPoints)
        {
            patrolSequence.AddChild(new LeafNode(
                "walkToPoint_X" + patrolPoint.position.x + "Z" + patrolPoint.position.z, 
                new MoveStrategy(patrolPoint.position, this.transform, agent)
            ));
        }
        
        return patrolSequence;
    }
    
    private SequenceNode GetWeaponSequence()
    {
        SequenceNode getWeaponSequence = new SequenceNode("getWeaponSequence");

        getWeaponSequence.AddChild(new LeafNode("hasNoWeaponCondition", new ConditionStrategy(
            () => _blackBoard.IsValueEqualTo(_hasWeaponKey, false) 
        )));
        getWeaponSequence.AddChild(new LeafNode("moveToWeapon", new MoveStrategy(weapon.transform.position, this.transform, agent)));
        getWeaponSequence.AddChild(new LeafNode("pickUpWeapon", new ActionStrategy( () =>
            {
                _blackBoard.SetValue(_hasWeaponKey, true);
                weapon.SetActive(false);
            }
        )));

        return getWeaponSequence;
    }

    private SequenceNode FollowPlayerSequence()
    {
        SequenceNode followPlayer = new SequenceNode("followPlayerSequence");
        
        followPlayer.AddChild(new LeafNode("followLastSeenPlayerPos", new FollowStrategy(_blackBoard, transform, agent)));
        followPlayer.AddChild(AttackPlayerSequence());
        
        return followPlayer;
    }

    private SequenceNode AttackPlayerSequence()
    {
        SequenceNode attackPlayer = new SequenceNode("attackPlayerSequence");

        attackPlayer.AddChild(new LeafNode("canSeePlayer", new ConditionStrategy(
            () => _blackBoard.IsValueEqualTo(_canSeePlayerKey, true) 
        )));
        attackPlayer.AddChild(new LeafNode("isPlayerAlive", new ConditionStrategy(
            () => _blackBoard.IsValueEqualTo(_isPlayerAliveKey, true) 
        )));
        attackPlayer.AddChild(new LeafNode("damagePlayer", new ActionStrategy( () => GameEvents.OnPlayerDamaged?.Invoke(attackDmg))));
        attackPlayer.AddChild(new LeafNode("attackCooldown", new WaitStrategy(attackCooldown)));

        return attackPlayer;
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
