using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    
    [SerializeField]
    private bool _isAnchor; 
    [SerializeField]
    private bool _isStartNode; 
    private readonly List<Beam> _connectedBeams = new();

    public float CurrentLoad{get; private set;}
    public float ExternalLoad{get; private set;}
    public bool IsAnchor=>_isAnchor;
    public bool IsStartNode => _isStartNode;
    public bool IsRevealed { get; private set; }
    public Vector3 Position => transform.position;
    
    public event Action OnNodeRevealed;
    public event Action<float> OnLoadChanged;
    public event Action OnNodeHidden;
    
    public void Hide()
    {
        if (_connectedBeams.Count > 0) return; 
    
        IsRevealed = false;
        OnNodeHidden?.Invoke();
    }
    
    public void Initialize(bool isAnchor)
    {
        _isAnchor = isAnchor;
    }
    
    public void AddConnectionBeam(Beam beam)
    {
        if (!_connectedBeams.Contains(beam))
        {
            _connectedBeams.Add(beam);
        }
    }
    
    public void Reveal()
    {
        if(IsRevealed) return;
        IsRevealed = true;
        OnNodeRevealed?.Invoke();
    }
    
    public void UpdateLoad(float newLoad)
    {
        CurrentLoad = newLoad;
        OnLoadChanged?.Invoke(CurrentLoad);
    }

    public void CopySettingsFrom(Node otherNode)
    {
        this._isAnchor = otherNode._isAnchor;
    }

}
