using UnityEngine;

public class BridgeFinishTrigger : MonoBehaviour
{
    private bool _rewarded = false;

    private void OnEnable()
    {
        StressSimulator.OnTestStarted += ResetTrigger;
        StressSimulator.OnResetToBuild += ResetTrigger;
        StressSimulator.OnSimulationFailed += ResetTrigger;
    }

    private void OnDisable()
    {
        StressSimulator.OnTestStarted -= ResetTrigger;
        StressSimulator.OnResetToBuild -= ResetTrigger;
        StressSimulator.OnSimulationFailed -= ResetTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rewarded) return;
        if (other.GetComponent<VehicleWeightSource>() == null) return;

        _rewarded = true;

        StressSimulator sim = FindFirstObjectByType<StressSimulator>();
        AnalysisResult result = sim?.LastAnalysis;

        BridgeRewardSystem.Instance?.EvaluateAndReward(result);
        sim?.NotifySimulationPassed(result);    }

    private void ResetTrigger() => _rewarded = false;
}