using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private BridgeStructure _bridgeStructure;
    [SerializeField] private float _hintDuration = 3f;

    public void ShowHints()
    {
        StartCoroutine(HintRoutine());
    }

    private IEnumerator HintRoutine()
    {
        foreach (Node node in _bridgeStructure.Nodes)
        {
            if (!node.IsAnchor)
            {
                node.Reveal();
            }
        }

        yield return new WaitForSeconds(_hintDuration);
    }
}