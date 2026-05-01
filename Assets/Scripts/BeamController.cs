using System.Net;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    public Transform endPoint;
    [SerializeField]
    public float distanceBetweenPoints = 0.0f;
    public Vector3 middlePoint;
    
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        middlePoint = (startPoint.position+endPoint.position)/2;
     
        transform.position = middlePoint;
        transform.LookAt(endPoint);
     
        distanceBetweenPoints = Vector3.Distance(startPoint.position, endPoint.position);
     
        transform.localScale = new Vector3(0.2f, 0.2f, distanceBetweenPoints);
    }
}
