using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
public class DJ_CameraControl : MonoBehaviour
{
    private GameObject MainCamera;
    private Camera Camera;
    private bool Zoom = false;

    [SerializeField]
    private float ZoomOffset = 0.5f;
    [SerializeField]
    private float CameraSpeed = 5;
    [SerializeField]
    private Vector3 CameraOffset = new Vector3(0, -3, -10);
    [SerializeField]
    private Vector2 XBounds = new Vector2(-10000f, 10000f);
    [SerializeField]
    private Vector2 YBounds = new Vector2(-10000f, 10000f);
    private Vector3 Center = new Vector3(0, 0, 0);
    private float ZoomTarget = 5f;
    private List<Vector2> InterestPoints = new List<Vector2>();

    void Start()
    {
        MainCamera = gameObject;
        Camera = MainCamera.GetComponent<Camera>();
    }

    void Update()
    {
        UpdateCenter();
        //Interpolation to center
        transform.position = Vector3.Lerp(transform.position, Center, CameraSpeed * Time.deltaTime);
        //Interpolation to zoom
        Camera.orthographicSize = Mathf.Lerp(Camera.orthographicSize, ZoomTarget, CameraSpeed * Time.deltaTime);
        // Replace with hitstop call later on
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        if (Zoom == false)
        {
            ZoomTarget -= ZoomOffset;
            Zoom = true;
        }
        else
        {
            ZoomTarget += ZoomOffset;
            Zoom = false;
        }
    }
    // Recalculates the center of interestPoints
    void UpdateCenter()
    {
        //Calculating center of interestPoints
        if (InterestPoints.Count == 0)
        {
            return;
        }
        Vector3 NewCenter = new Vector3(0, 0, 0);
        foreach (Vector3 point in InterestPoints)
        {
            NewCenter += new Vector3(point.x, point.y, 0);
        }
        NewCenter /= InterestPoints.Count;
        //Adding CameraOffset
        NewCenter += CameraOffset;
        //Binding center
        if (NewCenter.x < XBounds.x)
        {
            NewCenter.x = XBounds.x;
        } else if (NewCenter.x > XBounds.y)
        {
            NewCenter.x = XBounds.y;
        } else if (NewCenter.y < YBounds.x)
        {
            NewCenter.y = YBounds.x;
        } else if (NewCenter.y > YBounds.y)
        {
            NewCenter.y = YBounds.y;
        }
        Center = NewCenter;
    }
    public void UpdateInterestPoints(List<Vector2> Points)
    {
        InterestPoints = Points;
    }
}
