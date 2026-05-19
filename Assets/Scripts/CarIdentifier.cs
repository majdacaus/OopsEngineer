using UnityEngine;

public class CarIdentifier : MonoBehaviour
{
    public static Transform ActiveCar; 

    void OnEnable() { ActiveCar = this.transform; }
}