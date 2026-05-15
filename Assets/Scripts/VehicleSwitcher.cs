using UnityEngine;

public class VehicleSwitcher : MonoBehaviour
{
    [Header("Vehicles")]
    public GameObject[] vehicles;
    public Camera followCamera;
    private int currentIndex = 0;

    void Start()
    {
        for (int i = 0; i < vehicles.Length; i++)
            vehicles[i].SetActive(i == 0);

        SetCameraTarget(vehicles[0]);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool reverse = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            SwitchVehicle(reverse ? -1 : 1);
        }
    }

    void SwitchVehicle(int direction)
    {
        vehicles[currentIndex].SetActive(false);

        currentIndex = (currentIndex + direction + vehicles.Length) % vehicles.Length;

        vehicles[currentIndex].SetActive(true);
        SetCameraTarget(vehicles[currentIndex]);

        Debug.Log("Switched to: " + vehicles[currentIndex].name);
    }

    void SetCameraTarget(GameObject vehicle)
    {
        CameraFollow cam = followCamera?.GetComponent<CameraFollow>();
        if (cam != null)
            cam.SetTarget(vehicle.transform);
    }
}