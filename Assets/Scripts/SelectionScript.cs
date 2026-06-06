using UnityEngine.InputSystem;
﻿using UnityEngine;

public class SelectionScript : MonoBehaviour
{
    public Camera tCamera;
    public GameObject editStoneMenu;

    private Transform selection = null;
    private Transform rotation = null;
    private int stoneMask = 1 << 9;
    private int groundMask = 1 << 10;
    private Vector3 panPosition;
    private Vector3 delta  =  Vector3.zero;
    private Vector3 prevPos = Vector3.zero;
    private bool    panning = false;
    private float   cameraY = 0.0f;

    private Vector3 deltaHitdef;

    // Use this for initialization
    void Start () {
        panPosition = new Vector3(0.0f, 0.0f);
        editStoneMenu.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate() {
        if (tCamera != null) {
            float scroll =  Mouse.current.scroll.ReadValue().y;

            if(scroll != 0.0f)
            {
                // print(scroll);
                cameraY = tCamera.transform.position.y + (-scroll * 20.0f);
                panPosition.x = tCamera.transform.position.x;
                panPosition.y = Mathf.Max(Mathf.Min(cameraY,100),30);
                panPosition.z = tCamera.transform.position.z;
                tCamera.transform.position = panPosition;
            }

            // Moving camera
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                prevPos = Mouse.current.position.ReadValue();
                deltaHitdef.y = Mathf.Infinity;
            }

            if (Mouse.current.rightButton.isPressed)
            {

                RaycastHit hit;
                Ray ray = tCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (panning == false && selection == null && Physics.Raycast(ray, out hit, Mathf.Infinity, stoneMask))
                {
                    editStoneMenu.SetActive(true);

                    selection = hit.transform;
                    rotation = hit.transform;
                }
                else if (selection == null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {   // Clicking on terrain or anything else (move camera).
                    delta = (Vector3)Mouse.current.position.ReadValue() - prevPos;
                    //print(delta);
                    panPosition.x = delta.x * 0.1f;
                    panPosition.y = 0;
                    panPosition.z = delta.y * 0.1f;
                    tCamera.transform.position += panPosition;
                    panning = true;
                    prevPos = Mouse.current.position.ReadValue();
                    rotation = null;
                    editStoneMenu.SetActive(false);
                }
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                selection = null;
                panning = false;
            }

            // Moving stone
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                prevPos = Mouse.current.position.ReadValue();
                deltaHitdef.y = Mathf.Infinity;
            }

            if (Mouse.current.leftButton.isPressed)
            {

                RaycastHit hit;
                RaycastHit terrainHit;
                Ray ray = tCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                
                if (panning == false && selection == null && Physics.Raycast(ray, out hit, Mathf.Infinity, stoneMask))
                {
                    editStoneMenu.SetActive(true);

                    selection = hit.transform;
                    rotation = hit.transform;
                }

                //selection is the object who collides with the cursor
                if (selection != null) {
                   if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                   {
                        Vector3 GroundhitPoint = hit.point;
                        if (deltaHitdef.y == Mathf.Infinity)
                        {
                            deltaHitdef = GroundhitPoint;
                        }
                        GroundhitPoint += selection.position - deltaHitdef;
                        deltaHitdef = hit.point;
                        selection.position = GroundhitPoint;
                    }
                    else if (Terrain.activeTerrains.Length > 0 && Terrain.activeTerrain.GetComponent<Collider>().Raycast(ray, out terrainHit, Mathf.Infinity))
                    {
                        Vector3 hitPoint = terrainHit.point;
                        if (deltaHitdef.y == Mathf.Infinity)
                        {
                            deltaHitdef = hitPoint;
                        }
                        deltaHitdef = hitPoint - deltaHitdef;
                        hitPoint += selection.position - hitPoint + deltaHitdef;
                        deltaHitdef = terrainHit.point;
                        selection.position = hitPoint;
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                selection = null;
                panning = false;
            }
        }
    }
    
    public void rotateUP(){
        if (rotation != null)
        {
            rotation.transform.Rotate(Vector3.forward, 20.0f);
        }
    }

    public void rotateDown()
    {
        if (rotation != null)
        {
            rotation.transform.Rotate(Vector3.forward, -20.0f);
        }
    }

    public void deleteStone()
    {
        if (rotation != null)
        {
            Destroy(rotation.gameObject);
        }
    }
}
