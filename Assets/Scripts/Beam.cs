using System;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private Node  _startNode;
    [SerializeField] private Node  _endNode;
    [SerializeField] private float _maxLoad = 2000f;
    
    public List<Beam> linkedBeams = new List<Beam>();
    public GameObject linkedRoad;

    private float _currentStress = 0f;

    public Node  StartNode    => _startNode;
    public Node  EndNode      => _endNode;
    public float MaxLoad      => _maxLoad;
    public float CurrentStress => _currentStress;
    public float StressRatio => _maxLoad > 0f ? _currentStress / _maxLoad : 0f;
    
    public float           Length;
    public float           Angle;
    public BeamMaterialType MaterialType;

    public event Action<float> OnStressChanged;

    void Start()
    {
        if (_startNode != null) _startNode.AddConnectionBeam(this);
        if (_endNode != null) _endNode.AddConnectionBeam(this);
        if (Length <= 0 && _startNode != null && _endNode != null)
        {
            Length = Vector3.Distance(_startNode.Position, _endNode.Position);
        }
    }
    
    public void Initialize(Node start, Node end, BeamMaterialType material, float maxLoad = 100f)
    {
        this._startNode   = start;
        this._endNode     = end;
        MaterialType = material;

        Vector3 dir = _endNode.transform.position - _startNode.transform.position;
        Length = dir.magnitude;
        Angle  = Vector3.Angle(dir, new Vector3(dir.x, 0, dir.z));

        if (maxLoad <= 0f)
        {
            _maxLoad = material switch
            {
                BeamMaterialType.Steel => 8000f,
                BeamMaterialType.Cable => 12000f,
                _                      => 3000f  
            };
        }
        else
        {
            _maxLoad = maxLoad;
        }
        
        _startNode.AddConnectionBeam(this);
        _endNode.AddConnectionBeam(this);
        
        StressSimulator sim = FindFirstObjectByType<StressSimulator>();
        if (sim != null) sim.RegisterBeam(this);
        
        Debug.Log($"Greda spojena između {start.name} i {end.name}");
    }

    public Node GetOtherNode(Node from) => from == _startNode ? _endNode : _startNode;

    public void UpdateStress(float newStress)
    {
        _currentStress = newStress;
        OnStressChanged?.Invoke(StressRatio);
    }

    public bool IsBroken() => _currentStress >= _maxLoad;
}