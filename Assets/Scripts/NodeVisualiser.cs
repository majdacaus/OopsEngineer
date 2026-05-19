using System;
using UnityEngine;

public class NodeVisualiser : MonoBehaviour
{
    private Node _node;
    private Renderer _nodeRenderer;

    [SerializeField] private Color _anchorColor = Color.blue;
    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _overloadColor = Color.red;
    
    void Awake ()
    {
        _node = GetComponent<Node>();
        _nodeRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        _node.OnNodeRevealed += HandleReveal;
        _node.OnLoadChanged += HandleLoadVisuals;
        _node.OnNodeHidden += HandleHide;
    }
    
    private void HandleHide()
    {
        _nodeRenderer.enabled = false;
    }

    private void InitalizeVisuals()
    {
        if (_node.IsAnchor)
        {
            _nodeRenderer.enabled = true;
            _nodeRenderer.material.color = _anchorColor;
        }
    }
        
    private void HandleReveal()
    {
        _nodeRenderer.enabled = true;
    }

    private void HandleLoadVisuals(float currentLoad)
    {
        if (currentLoad > 50f)
        {
            _nodeRenderer.material.color = Color.Lerp(_defaultColor, _overloadColor, currentLoad / 100f);
        }
    }

    private void Start()
    {
        InitalizeVisuals();
    }
}
