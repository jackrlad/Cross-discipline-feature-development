using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform CameraArm;

    private InputReader ir;
    private float cameraRot = 0;

    void Start()
    {
        ir = GetComponent<InputReader>();
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

        if(ir.cLeft)
        {
            cameraRot -= 90;
        }
        if(ir.cRight)
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
