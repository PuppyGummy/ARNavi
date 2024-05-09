using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.Lightship.AR;
// using Niantic.Lightship.AR.ARSessionEventArgs;
using Niantic.Lightship.AR.Utilities;
using UnityEngine.XR.ARFoundation;
using Input = Niantic.Lightship.AR.Input;
public class BallController : MonoBehaviour
{
    public GameObject ball;
    ARSession ArSession;
    // Start is called before the first frame update
    void Start()
    {
        // ArSession
    }

    // Update is called once per frame
    void Update()
    {
        // If there is no touch, do nothing
        if (Input.touchCount == 0)
        {
            return;
        }
        // If we detect a touch, we will instantiate a ball at the touch position
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            TouchBegan(touch);
        }
    }
    private void TouchBegan(Touch touch)
    {
        // Get the touch position
        // Vector3 touchPosition = Input.GetTouch(0).position;
        // Convert the touch position to a ray
        // Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        // Create a new ball at the touch position
        ball = Instantiate(ball, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);
        // Add a force to the ball to shoot it forward
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 300);
    }
}
