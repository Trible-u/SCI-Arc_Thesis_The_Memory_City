//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{


    //public GameObject Camera;
    public Transform target;
    public GameObject targetFollow;
    public Vector3 targetOffset;
    public float distance = 5.0f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;
    private Vector3 position;

    public Vector3 startPosition = new Vector3(65f, 60f, 65f);
    public Quaternion startRotation = Quaternion.Euler(30f, 45f, 0f); // old one, the new one is flipped in space because of direction of models
                                                                      // public Quaternion startRotation = Quaternion.Euler(145f, 45f, 180f);

    public Vector3 topPosition = new Vector3(0, 120, 0);
    public Quaternion topRotation = Quaternion.Euler(90f, 0f, 0f);

    public Vector3 frontPosition = new Vector3(2, 2, 0);
    public Quaternion frontRotation = Quaternion.Euler(0f, 0f, 0f);

    private Vector3 targetOffsetStart = Vector3.zero;
    private float startDistance;

    private bool isRotating = false;

    //public Button Topbutton;
    // public Button Frontbutton;

    void Start()
    {
        Init();

        //  Button btn = button.GetComponent<Button>();
        // Topbutton.onClick.AddListener(TaskOnClick);
        // Frontbutton.onClick.AddListener(TaskOnClick);

    }
    void OnEnable() { Init(); }

    public void Init()
    {
        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        //if (!target)
        //{
        //    GameObject go = new GameObject("Cam Target");
        //    go.transform.position = transform.position + (transform.forward * distance);
        //    target = go.transform;
        //}

        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;



        //be sure to grab the current rotations as starting points.
        position = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;



        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);

        startDistance = distance;


    }

    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
    void LateUpdate()
    {

        Follow();

        if (Input.GetKeyDown(KeyCode.F) && Input.GetKeyDown(KeyCode.LeftControl))         // god knows how i did this Focus thing..
        {

            focus();

        }


        if (Input.GetKeyDown(KeyCode.T) && Input.GetKeyDown(KeyCode.LeftControl))        // god knows how i did this Focus thing..
        {

            top();

        }


        if (Input.GetKeyDown(KeyCode.Y) && Input.GetKeyDown(KeyCode.LeftControl))         // god knows how i did this Focus thing..
        {

            front();

        }


        // If Control and Alt and Middle button? ZOOM!
        if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        {
            desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
        }
        // If middle mouse and left alt are selected? ORBIT - right mouse only atm
        else if (Input.GetMouseButton(1)) // && Input.GetKey(KeyCode.LeftAlt))
        {
            isRotating = true;
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            ////////OrbitAngle

            //Clamp the vertical axis for the orbit
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            // set camera rotation 
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            transform.rotation = rotation;
        }
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
        else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt))
        {
            //grab the rotation of the camera so we can move in a psuedo local XY space
            target.rotation = transform.rotation;
            target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
            target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
        }

        ////////Orbit Position

        // affect the desired Zoom distance if we roll the scrollwheel
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        //clamp the zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        // For smoothing of the zoom, lerp distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

        // calculate position based on the new currentDistance 
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;





    }

    public void focus()
    {
        desiredDistance = startDistance;
        currentDistance = startDistance;
        desiredRotation = startRotation;
        currentRotation = startRotation;
        rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        transform.rotation = rotation;
        target.position = Vector3.zero;
        targetOffset = targetOffsetStart;


    }


    public void top()
    {

        desiredDistance = startDistance;
        currentDistance = startDistance;
        desiredRotation = topRotation;
        currentRotation = topRotation;
        rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        transform.rotation = rotation;
        target.position = Vector3.zero;
        targetOffset = targetOffsetStart;
    }

    public void front()
    {

        desiredDistance = startDistance;
        currentDistance = startDistance;
        desiredRotation = frontRotation;
        currentRotation = frontRotation;
        rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        transform.rotation = rotation;
        target.position = Vector3.zero;
        targetOffset = targetOffsetStart;


    }

    public void Follow()
    {
        target.transform.position = targetFollow.transform.position;


    }


    //void TaskOnClick(){

    //   top();
    //  front();


    //}


    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}