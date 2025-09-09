using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsCamera : MonoBehaviour {
    static BoundsCamera S;
    public InfoProperty info = new InfoProperty( "Using the BoundsCamera",
        "1. <b>target</b> should be set to your main player character (or whatever you want the camera to follow).\n\n" +
        "2. <b>camBoundsParent</b> is an empty GameObject with several BoxCollider2Ds on it or in children beneath it.\n\n" +
        "3. Whenever the target is within one of the BoxCollider2Ds, the camera will move only within that Collider.\n\n" +
        "4. <b>deadZoneSize</b> determines how far from the middle of the screen the target must move for the " +
        "camera to track it. [0.5f,0.5f] means that the camera will not move until the player exits the middle 50% " +
        "of the screen.");

    [Header("Inscribed")]
    public Transform    target;
    public GameObject   camBoundsParent;
    [Tooltip("Must be between 0 & 1. Determines size of area in middle of screen that will not force scroll of camera.")]
    public Vector2      deadZoneSize = new Vector2(0.5f, 0.5f); // What border area must the target enter to force a scroll of the camera - JB
    
    [Header("Dynamic")]
    [Tooltip("The distance from the center of the camera to the side of the frame.")]
    public float        camHalfW = 8;
    [Tooltip("The distance from the center of the camera to the top or bottom of the frame.")]
    public float        camHalfH = 7.5f;
    public float        camOffsetZ = -10;
    
    [Header("Set Dynamically")]
    
    public Collider2D   currColld;

    private Collider2D[] camBounds;
    private Camera       cam;

	// Use this for initialization
	void Awake () {
        cam = GetComponent<Camera>();
        camOffsetZ = transform.position.z;
        S = this;
        RefreshCamBounds();
        
        // Determine camHalfW and camHalfH from Camera
        if ( cam.orthographic ) {
            camHalfH = cam.orthographicSize;
            camHalfW = cam.aspect * camHalfH;
        } else {
            Debug.LogError( "The Camera that BoundsCamera is attached to must be Orthographic to work properly." );
        }
    }

    void RefreshCamBounds() {
        // Get all of the camBounds colliders - JB
        camBounds = camBoundsParent.GetComponentsInChildren<Collider2D>();
        currColld = null;
	}
    public static void REFRESH_CAM_BOUNDS() {
        S.RefreshCamBounds();
    }    
	
    
	void FixedUpdate () {
        // Get the position of the target
        Vector3 tPos = target.position;

        // Determine whether the target is no longer still within the same Collider - JB
        if (currColld == null || !currColld.bounds.Contains(tPos)) {
            // Find the Collider that the target IS within - JB
            currColld = null;

            foreach (Collider2D colld in camBounds) {
                if (colld.bounds.Contains(tPos)) {
                    // If we find a collider, then set currColld and break out of the foreach loop – JB
                    currColld = colld;
                    break;
                }
            }

            //If changing bounds, reset all enemies
            if (currColld != null) {
                // EnemyManager.EM.ResetSpawners();
            }

        }

        // First, align the camera with the target (with a Z offset, of course) – JB
        tPos.z += camOffsetZ;
       
        // Make sure that the Camera only moves if the target is outside the middle of the screen
        Vector3 cPos = transform.position;
        float deadZoneX = camHalfW * deadZoneSize.x;
        float deadZoneY = camHalfH * deadZoneSize.y;
        if ( Mathf.Abs(tPos.x - cPos.x) > deadZoneX ) {
            if (tPos.x > cPos.x) {
                cPos.x = tPos.x - deadZoneX;
            } else {
                cPos.x = tPos.x + deadZoneX;
            }
        }
        if ( Mathf.Abs(tPos.y - cPos.y) > deadZoneY ) {
            if (tPos.y > cPos.y) {
                cPos.y = tPos.y - deadZoneY;
            } else {
                cPos.y = tPos.y + deadZoneY;
            }
        }
		
        // if the currColld is not null, limit movement to within the Collider – JB
        if (currColld != null) {
            // Account for camW and camH when judging the size of the bounds - JB
            Vector3 min = currColld.bounds.min;
            Vector3 max = currColld.bounds.max;

            min.x += camHalfW;
            min.y += camHalfH;
            max.x -= camHalfW;
            max.y -= camHalfH;

            cPos.x = Mathf.Clamp(cPos.x, min.x, max.x);
            cPos.y = Mathf.Clamp(cPos.y, min.y, max.y);
        }

        transform.position = cPos;
	}



}
