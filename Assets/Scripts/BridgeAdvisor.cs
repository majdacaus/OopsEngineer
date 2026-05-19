using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BridgeAdvisor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConstructionAnalyzer analyzer;
    [SerializeField] private StressSimulator       stressSimulator;

    [Header("UI")]
    [SerializeField] private GameObject     bubbleRoot;
    [SerializeField] private TextMeshProUGUI messageText;
 //   [SerializeField] private Animator       bubbleAnimator;

    [Header("Timing")]
    [SerializeField] private float passiveCheckInterval = 6f;
    [SerializeField] private float minTimeBetweenTips   = 8f;
    [SerializeField] private float displayDuration      = 4f;
    [SerializeField] private float stressCheckInterval  = 3f;

    private float _lastTipTime      = -999f;
    private string _lastShownMessage = "";
    private bool _inStressMode      = false;
    private Coroutine _hideCoroutine;

    private List<Node> _nodes = new();
    private List<Beam> _beams = new();

    private static readonly int ShowHash = Animator.StringToHash("Show");

    private void OnEnable()
    {
        StressSimulator.OnSimulationFailed += OnSimulationFailed;
        StressSimulator.OnSimulationBlocked += OnSimulationBlocked;
    }

    private void OnDisable()
    {
        StressSimulator.OnSimulationFailed -= OnSimulationFailed;
        StressSimulator.OnSimulationBlocked -= OnSimulationBlocked;
    }

    private void Start()
    {
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
        StartCoroutine(PassiveCheckLoop());
    }

    private IEnumerator PassiveCheckLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(passiveCheckInterval);

            if (_inStressMode) continue;
            if (Time.time - _lastTipTime < minTimeBetweenTips) continue;

            RefreshNodeBeamLists();
            if (_nodes.Count < 2 || _beams.Count == 0) continue;

            AnalysisResult result = analyzer.PerformFullAnalysis(_nodes, _beams);
            string tip = PickPassiveTip(result);
            if (!string.IsNullOrEmpty(tip))
                ShowTip(tip);
        }
    }

    public void StartStressMonitoring()
    {
        _inStressMode = true;
        StartCoroutine(StressCheckLoop());
    }

    private IEnumerator StressCheckLoop()
    {
        while (_inStressMode)
        {
            yield return new WaitForSeconds(stressCheckInterval);

            RefreshNodeBeamLists();
            if (_nodes.Count == 0 || _beams.Count == 0) continue;

            AnalysisResult result = analyzer.PerformFullAnalysis(_nodes, _beams);
            string tip = PickStressTip(result);
            if (!string.IsNullOrEmpty(tip))
                ShowTip(tip);
        }
    }

    public void StopStressMonitoring()
    {
        _inStressMode = false;
    }

    private string PickPassiveTip(AnalysisResult res)
    {
        List<string> candidates = new();

        if (!res.PathExists)
            candidates.Add("We have a problem. The bridge doesn't actually reach the other side!");

        if (res.IsFloating)
            candidates.Add("Are you building a bridge or a kite? Some parts are literally floating in the air!");

        if (res.IsLackingTriangles)
            candidates.Add("Square shapes are for boxes, not bridges. Add some triangles or watch it fold like an omelet!");

        if (res.HasWeakMaterials)
            candidates.Add("Size matters! Some beams are too long and will snap like a toothpick.");

        // if (res.IsTopHeavy)
        //     candidates.Add("Your bridge is top-heavy. It has more ego than brains! It's going to tip over.");

        if (res.CurrentCost > analyzer.maxBudget)
            candidates.Add($"Our pockets are empty! You're ${res.CurrentCost - analyzer.maxBudget:F0} over budget. Chill with the steel!");

        if (candidates.Count == 0)
        {
            if (res.StructuralHealth >= 90f)
                candidates.Add("Look at that stability! You're a natural-born engineer.");
            else if (res.CurrentCost < analyzer.maxBudget * 0.5f)
                candidates.Add("Budget King! You're building this for pennies.");
        }

        return PickNonRepeat(candidates);
    }

    private string PickStressTip(AnalysisResult res)
    {
        List<string> candidates = new();

        // if (res.IsTopHeavy)
        //     candidates.Add("The steel on top is destabilizing us! It's wobbling like a jelly!");

        if (res.StructuralHealth < 70f && res.StructuralHealth >= 50f)
            candidates.Add("I hear creaking... that's never a good sign in this business.");

        if (res.StructuralHealth < 50f)
            candidates.Add("Mayday! Mayday! The bridge is holding on by a prayer!");

        if (res.IsFloating)
            candidates.Add("Unconnected parts are flying off! Physics doesn't like your design.");

        return PickNonRepeat(candidates);
    }

    private string PickNonRepeat(List<string> candidates)
    {
        if (candidates.Count == 0) return "";
        candidates.RemoveAll(m => m == _lastShownMessage);
        if (candidates.Count == 0) return "";
        return candidates[Random.Range(0, candidates.Count)];
    }

    public void ShowTip(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        _lastTipTime     = Time.time;
        _lastShownMessage = message;

        if (messageText != null) messageText.text = message;
        if (bubbleRoot  != null) bubbleRoot.SetActive(true);

      //  if (bubbleAnimator != null)
     //       bubbleAnimator.SetTrigger(ShowHash);

        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
    }

    private void OnSimulationFailed()
    {
        StopStressMonitoring();
        ShowTip("Gravity wins again! Maybe try more triangles and less... hope?");
    }

    private void OnSimulationBlocked(string message)
    {
        ShowTip(message);
    }

    private void RefreshNodeBeamLists()
    {
        _nodes = new List<Node>(FindObjectsByType<Node>(FindObjectsSortMode.None));
        _beams = new List<Beam>(FindObjectsByType<Beam>(FindObjectsSortMode.None));
    }
}