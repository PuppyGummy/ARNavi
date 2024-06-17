// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Niantic.Lightship.AR;
// // using Niantic.Lightship.AR.ARSessionEventArgs;
// using Niantic.Lightship.AR.Utilities;
// using UnityEngine.XR.ARFoundation;
// using Input = Niantic.Lightship.AR.Input;


// public class HelloARController : MonoBehaviour
// {
//     public Camera FirstPersonCamera;
//     public GameObject CameraTarget;
//     private Vector3 PrevARPosePosition;
//     private bool Tracking = false;

//     public void Start()
//     {  //set initial position 
//         PrevARPosePosition = Vector3.zero;
//     }
//     public void Update()
//     {
//         UpdateApplicationLifecycle();  //move the person indicator according to position
//         Vector3 currentARPosition = Frame.Pose.position;
//         if (!Tracking)
//         { Tracking = true; PrevARPosePosition = Frame.Pose.position; }  //Remember the previous position so we can apply deltas
//         Vector3 deltaPosition = currentARPosition - PrevARPosePosition; PrevARPosePosition = currentARPosition;
//         if (CameraTarget != null)
//         {    // The initial forward vector of the sphere must be aligned with the initial camera direction in the XZ plane.
//              // We apply translation only in the XZ plane.
//             CameraTarget.transform.Translate(deltaPosition.x, 0.0f, deltaPosition.z);    // Set the pose rotation to be used in the CameraFollow script   
//             FirstPersonCamera.GetComponent<ArrowDirection>().targetRot = Frame.Pose.rotation;
//         }
//     }
// }