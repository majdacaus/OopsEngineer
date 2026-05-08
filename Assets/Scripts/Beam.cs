using System;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private Node _startNode;
    [SerializeField] private Node _endNode;
    [SerializeField] private float _costPerUnit = 10f;
    [SerializeField] private float _maxLoad = 100f;
    private float _currentStress = 0f;
    
    public Node StartNode => _startNode;
    public Node EndNode => _endNode;
    
    public float MaxLoad => _maxLoad;
    public float CurrentStress => _currentStress;
    public float StressRatio => Mathf.Clamp01(_currentStress / _maxLoad);
    public float Length;
    public float Angle;
    
    public event Action<float> OnStressChanged;

    public void Initialize(Node start, Node end, float maxLoad = 100f)
    {
        _startNode = start;
        _endNode = end;
        _maxLoad = maxLoad;
        
        Vector3 dir = _endNode.transform.position - _startNode.transform.position;
        Length = dir.magnitude;
        Angle = Vector3.Angle(dir, new Vector3(dir.x, 0, dir.z));
        
        _startNode.AddConnectionBeam(this);
        _endNode.AddConnectionBeam(this);
    }

    public void UpdateStress(float newStress)
    {
        _currentStress = newStress;
        OnStressChanged?.Invoke(StressRatio);
    }

    public bool IsBroken() => _currentStress >= _maxLoad;
    
}
