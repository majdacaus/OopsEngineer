using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private bool _isAnchor;
    [SerializeField] private bool _isStartNode;

    private readonly List<Beam> _connectedBeams = new();
    private SphereCollider _collider;
    public float   CurrentLoad  { get; private set; }
    public float   ExternalLoad { get; private set; }
    public bool    IsAnchor     => _isAnchor;
    public bool    IsStartNode  => _isStartNode;
    public bool    IsRevealed   { get; private set; }
    public Vector3 Position     => transform.position;

    public event Action        OnNodeRevealed;
    public event Action<float> OnLoadChanged;
    public event Action        OnNodeHidden;
    void Awake()
    {
        _collider = GetComponent<SphereCollider>();
    }
    public void Initialize(bool isAnchor)
    {
        _isAnchor = isAnchor;
    }

    public void Reveal()
    {
        //if (IsRevealed) return;
        IsRevealed = true;
        //if (_collider != null) _collider.enabled = true;
        
        if (TryGetComponent(out MeshRenderer mr)) mr.enabled = true;
        if (TryGetComponent(out Collider col)) col.enabled = true;
        OnNodeRevealed?.Invoke();
    }

    public void Hide()
    {
        IsRevealed = false;
        if (TryGetComponent(out MeshRenderer mr)) mr.enabled = false;
        if (TryGetComponent(out Collider col)) col.enabled = false;
        //if (_collider != null) _collider.enabled = false;
        OnNodeHidden?.Invoke();
    }

    public void AddConnectionBeam(Beam beam)
    {
        if (!_connectedBeams.Contains(beam))
            _connectedBeams.Add(beam);
    }

    public void UpdateLoad(float newLoad)
    {
        CurrentLoad = newLoad;
        OnLoadChanged?.Invoke(CurrentLoad);
    }

    public void CopySettingsFrom(Node other)
    {
        _isAnchor = other._isAnchor;
    }

    //public IReadOnlyList<Beam> GetConnectedBeams() => _connectedBeams;
    public List<Beam> GetConnectedBeams() => _connectedBeams;
}