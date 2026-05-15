using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftC;
    public WheelCollider frontRightC;
    public WheelCollider backLeftC;
    public WheelCollider backRightC;

    [Header("Wheel Visuals")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform backLeftT;
    public Transform backRightT;

    [Header("Lights")]
    public Light[] brakeLights;
    public Light[] reverseLights;
    public Material brakeLightMaterial;

    [Header("Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 2500f;
    public float maxSteerAngle = 30f;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.mass = 1000f;
            rb.centerOfMass = new Vector3(0, -0.4f, 0);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        foreach (WheelCollider wc in new[] { frontLeftC, frontRightC, backLeftC, backRightC })
        {
            JointSpring sus = wc.suspensionSpring;
            sus.spring = 30000f;
            sus.damper = 4000f;
            sus.targetPosition = 0.5f;
            wc.suspensionSpring = sus;

            wc.forceAppPointDistance = 0f;
        }
    }

    void FixedUpdate()
    {
        float v = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v = -1f;

        float h = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1f;

        Rigidbody rb = GetComponent<Rigidbody>();
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);

        bool braking = (v < 0 && speed > 1f) || Keyboard.current.spaceKey.isPressed;
        bool reversing = (v < 0 && speed < 1f);

        float brake = braking ? brakeForce : 0f;

        frontLeftC.brakeTorque = brake;
        frontRightC.brakeTorque = brake;
        backLeftC.brakeTorque = brake;
        backRightC.brakeTorque = brake;

        frontLeftC.motorTorque = 0f;
        frontRightC.motorTorque = 0f;
        backLeftC.motorTorque = !braking ? v * motorForce : 0f;
        backRightC.motorTorque = !braking ? v * motorForce : 0f;

        frontLeftC.steerAngle = h * maxSteerAngle;
        frontRightC.steerAngle = h * maxSteerAngle;

        SetBrakeLights(braking);
        SetReverseLights(reversing);

        UpdateWheel(frontLeftC, frontLeftT);
        UpdateWheel(frontRightC, frontRightT);
        UpdateWheel(backLeftC, backLeftT);
        UpdateWheel(backRightC, backRightT);
    }

    void SetBrakeLights(bool active)
    {
        foreach (Light l in brakeLights)
            if (l) l.enabled = active;

        if (brakeLightMaterial)
        {
            if (active) brakeLightMaterial.EnableKeyword("_EMISSION");
            else brakeLightMaterial.DisableKeyword("_EMISSION");
        }
    }

    void SetReverseLights(bool active)
    {
        foreach (Light l in reverseLights)
            if (l) l.enabled = active;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}