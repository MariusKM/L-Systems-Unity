using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragMove : MonoBehaviour
{
    public Vector3 ResetCamera;
    public Vector3 Origin;
    public Vector3 Diference;
    public Vector3 mousePos, mousePosStart;
    public float speed = 1.5f;
    public bool Drag = false;
    public Camera cam;
    void Start()
    {
        ResetCamera = Camera.main.transform.position;
        cam = GetComponent<Camera>();
    }
    void LateUpdate()
    {
       
        if (Input.GetMouseButton(0))
        {
            if(Input.GetMouseButtonDown(0))
                mousePosStart = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));

            //Debug.Log(mousePos);
            mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            Diference = (mousePos - transform.position);
            Diference = new Vector3(Diference.x*speed, Diference.y*speed, Diference.z);
            if (Drag == false)
            {
                Drag = true;
                Origin = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Camera.main.nearClipPlane));
            }
        }
        else
        {
            Drag = false;
        }
        if (Drag == true)
        {
            transform.position = Origin - Diference;
        }
        //RESET CAMERA TO STARTING POSITION WITH RIGHT CLICK
        if (Input.GetMouseButton(1))
        {
            transform.position = ResetCamera;
        }
    }
}