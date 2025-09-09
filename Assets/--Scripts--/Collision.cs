using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I made some improvements to this to have it work better with the physics system.
// The original version was using Update() instead of FixedUpdate(), which doesn't
//  really make any sense (because the physics only update once per FixedUpdate()).
//  -- JGB 2025-08-03

[DefaultExecutionOrder(-100)] // This makes Collision.FixedUpdate happen before Movement.FixedUpdate();
public class Collision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int  wallSide;
    public bool jumpedThisFrame;
    public bool landedThisFrame;

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset, rightOffset, leftOffset;
    private Color debugCollisionColor = Color.red;

    internal CapsuleCollider2D capsule;
    // Start is called before the first frame update
    void Start() {
        capsule = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    // void Update()
    void FixedUpdate() { // FixedUpdate is called every time the physics updates, which is 50x/second - JGB 2025-08-03
        bool wasOnGround = onGround;
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        
        landedThisFrame = !wasOnGround && onGround;
        if ( landedThisFrame ) SoundAndMusic.Play( eAudioTrigger.land );
        
        jumpedThisFrame = wasOnGround && !onGround;
        // The following line is NOT called here, because it would play a jump every time you walked off a ledge. - JGB
        // if ( jumpedThisFrame ) SoundAndMusic.Play( SoundAndMusic.eAudioTrigger.jump );
        
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer) 
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        wallSide = onRightWall ? -1 : 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position  + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}
