using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform CameraArm;
    private float cameraRot = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        RotCamera();

        CameraArm.position = transform.position;
    }

    void RotCamera()
    {
        cameraRot = 0;

        if(Input.GetKey(KeyCode.Q))
        {
            cameraRot -= 90;
        }
        if(Input.GetKey(KeyCode.E))
        {
            cameraRot += 90;
        }

        CameraArm.rotation = Quaternion.Lerp
        (
            CameraArm.rotation, 
            Quaternion.Euler(CameraArm.eulerAngles.x, cameraRot, CameraArm.eulerAngles.z), 
            0.1f
        );
    }
}
