using System;
using UnityEngine;

//This script is used by both movement and jump to detect when the character is touching the ground
// [RequireComponent(typeof(CharacterController))]
public class CharacterGround : MonoBehaviour {
    private CharacterMovement cMove;

    [Header("Collider Settings - Set in CharacterSettings_SO")]
    [XnTools.ReadOnly][SerializeField] [Tooltip("Length of the ground-checking collider")] public float groundLength = 0.95f;
    [XnTools.ReadOnly][SerializeField] public Vector3 raycastOffsetHeight;
    [XnTools.ReadOnly][SerializeField] public Vector3 raycastOffsetWidth;

    //[XnTools.ReadOnly] [Tooltip("Distance between the ground-checking colliders")] public Vector3 colliderOffset;
    [XnTools.ReadOnly] [Tooltip("Which layers are read as the ground")] public LayerMask groundLayers;

    private Vector3[] raycastOrigins = new Vector3[3];
    private bool[] onGrounds = new bool[3];

    private void Update() {
        CheckGrounded();
    }

    private void CheckGrounded() {
        raycastOrigins[0] = transform.position + raycastOffsetHeight;
        raycastOrigins[1] = raycastOrigins[0] + raycastOffsetWidth;
        raycastOrigins[2] = raycastOrigins[0] - raycastOffsetWidth;

        //Determine if the player is stood on objects on the ground layer, using a three raycasts: mid, left, and right
        onGrounds[0] = Physics2D.Raycast(raycastOrigins[0], Vector2.down, groundLength, groundLayers);
        onGrounds[1] = Physics2D.Raycast(raycastOrigins[1], Vector2.down, groundLength, groundLayers);
        onGrounds[2] = Physics2D.Raycast(raycastOrigins[2], Vector2.down, groundLength, groundLayers);
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying) {
            CheckGrounded();
        }
        //Draw the ground colliders on screen for debug purposes
        if (onGrounds[0]) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(raycastOrigins[0], raycastOrigins[0] + Vector3.down * groundLength);
        if (onGrounds[1]) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(raycastOrigins[1], raycastOrigins[1] + Vector3.down * groundLength);
        if (onGrounds[2]) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(raycastOrigins[2], raycastOrigins[2] + Vector3.down * groundLength);
    }

    private bool onGround { get { return onGrounds[0] || onGrounds[1] || onGrounds[2]; } }
    //Send ground detection to other scripts
    public bool GetOnGround() { return onGround || StairMaster.ON_STAIRS; }


}