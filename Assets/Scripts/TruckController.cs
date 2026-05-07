using UnityEngine;
using UnityEngine.InputSystem;

public class TruckController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftC; public WheelCollider frontRightC;
    public WheelCollider backLeftC; public WheelCollider backRightC;

    [Header("Wheel Visuals")]
    public Transform frontLeftT; public Transform frontRightT;
    public Transform backLeftT; public Transform backRightT;
    
    [Header("Brake Lights")]
    public Light[] brakeLights;
    public float brakeEmissionIntensity = 2f;
    public Material brakeLightMaterial;
    
    [Header("Reverse Lights")]
    public Light[] reverseLights;

    public float motorForce = 2000f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 35f;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.mass = 1500f;
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
            rb.interpolation = RigidbodyInterpolation.Interpolate; 
        }

        foreach (WheelCollider wc in new[] { frontLeftC, frontRightC, backLeftC, backRightC })
        {
            JointSpring sus = wc.suspensionSpring;
            sus.spring = 35000f;
            sus.damper = 4500f;
            sus.targetPosition = 0.5f;
            wc.suspensionSpring = sus;

            wc.forceAppPointDistance = 0f; 
        }
    }bool isBraking = false;

    void FixedUpdate()
    {
        float v = 0;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v = 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v = -1;

        float h = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1;

        float speed = Vector3.Dot(GetComponent<Rigidbody>().linearVelocity, transform.forward);

        bool isBraking = ((Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) && speed > 0.5f)
                         || Keyboard.current.spaceKey.isPressed;

        float brake = isBraking ? brakeForce : 0f;
        frontLeftC.brakeTorque = brake;
        frontRightC.brakeTorque = brake;
        backLeftC.brakeTorque = brake;
        backRightC.brakeTorque = brake;

        frontLeftC.motorTorque = isBraking ? 0f : v * motorForce;
        frontRightC.motorTorque = isBraking ? 0f : v * motorForce;

        frontLeftC.steerAngle = h * maxSteerAngle;
        frontRightC.steerAngle = h * maxSteerAngle;

        bool isReversing = (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) 
                           && speed < 0.5f;

        SetBrakeLights(isBraking);
        SetReverseLights(isReversing);

        /*UpdateWheel(frontLeftC, frontLeftT);
        UpdateWheel(frontRightC, frontRightT);
        UpdateWheel(backLeftC, backLeftT);
        UpdateWheel(backRightC, backRightT);*/
    }
    
    void SetBrakeLights(bool active)
    {
        foreach (Light l in brakeLights)
        {
            if (l != null) l.enabled = active;
        }

        if (brakeLightMaterial != null)
        {
            if (active)
                brakeLightMaterial.EnableKeyword("_EMISSION");
            else
                brakeLightMaterial.DisableKeyword("_EMISSION");
        }
    }

    void SetReverseLights(bool active)
    {
        foreach (Light l in reverseLights)
        {
            if (l != null) l.enabled = active;
        }
    }
    
    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 pos; Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}