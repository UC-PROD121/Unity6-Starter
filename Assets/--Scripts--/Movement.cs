using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce    = 50;
    public float slideSpeed   = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed    = 20;
    public float mantleTime   = 0.05f;

    [Space]
    [Header("Booleans")]
    [XnTools.ReadOnly] public bool canMove;
    [XnTools.ReadOnly] public bool wallGrab;
    [XnTools.ReadOnly] public bool wallJumped;
    [XnTools.ReadOnly] public bool wallSlide;
    [XnTools.ReadOnly] public bool isDashing;
    [XnTools.ReadOnly] public bool isMantling; // JGB 2025-08-03

    private Vector2 _mantlingVel;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    [XnTools.ReadOnly] public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        isMantling = false;
    }

    
    /// <summary>
    /// Update is called once per frame
    /// The original code had everything in Update, but that doesn't work well with the Physics system
    ///  because physics runs on FixedUpdate(). 
    /// However, Input functions like GetButtonUp() and GetButtonDown() ONLY work in Update() and not FixedUpdate().
    /// So I am caching their values and then consuming those values in FixedUpdate(), which solves the problem.
    ///   - JGB 2025-08-03
    /// </summary>
    void Update() {
        if ( Input.GetButtonDown( "Fire1" ) ) _fire1Down = true;
        if ( Input.GetButtonUp( "Fire3" ) ) _fire3Up = true;
        if ( Input.GetButtonDown( "Jump" ) ) _jumpDown = true;
    }

    private bool _fire3Up, _jumpDown, _fire1Down;

    
    /// <summary>
    /// FixedUpdate is called every time the Physics system updates (50x/second by default).
    /// Input.GetButton and GetAxis work fine in FixedUpdate, but GetButtonDown, GetButtonUp, and other Input.…Up
    ///  and Input.…Down calls do NOT work in FixedUpdate(), so I'm caching them in Update() (see above).
    ///    - JGB 2025-08-03
    /// </summary>
    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.linearVelocity.y);

        bool wasWallGrabbing = wallGrab; // Used for mantling over walls we've climbed. - JGB

        if (coll.onWall && Input.GetButton("Fire3") && canMove)
        {
            if(side != coll.wallSide) anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
        }

        // if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        if (_fire3Up || !coll.onWall || !canMove)
        {
            _fire3Up = false; // We used the value, so we need to set it back to false - JGB
            wallGrab = false;
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        
        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f) {
                rb.linearVelocity = new Vector2( rb.linearVelocity.x, 0 );
            }

            float speedModifier = y > 0 ? .5f : 1;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        // if (Input.GetButtonDown("Jump"))
        if (_jumpDown)
        {
            _jumpDown = false; // We used the value, so we need to set it back to false - JGB
            anim.SetTrigger("jump");

            if (coll.onGround)
                Jump(Vector2.up, false);
            if (coll.onWall && !coll.onGround)
                WallJump();
        }

        // if (Input.GetButtonDown("Fire1") && !hasDashed)
        if (_fire1Down && !hasDashed)
        {
            _fire1Down = false; // We used the value, so we need to set it back to false - JGB
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);
        
        if ( isMantling ) {
            rb.linearVelocity = _mantlingVel;
            return; // While mantling, don't do anything else.
        }

        if (wallGrab || wallSlide || !canMove)
            return;

        // Mantle walls that we've climbed to the top - JGB 2025-08-03
        if ( wasWallGrabbing && !wallGrab && !coll.onGround ) {
            // We were climbing a wall, but now we're at the top (hopefully)
            // side should be facing toward where the wall was
            Vector2 capsuleCenter = coll.capsule.bounds.center;
            float capsuleHalfWidth = coll.capsule.size.x * 0.5f;
            RaycastHit2D hit2D = Physics2D.Raycast( capsuleCenter, Vector2.right * side, capsuleHalfWidth * 1.2f, coll.groundLayer ); 
            if ( hit2D.collider == null ) {
                // Nothing was hit, so give a boost in that direction
                _mantlingVel = new Vector2( speed * side, speed );
                rb.linearVelocity = _mantlingVel;
                isMantling = true;
                anim.Flip(side);
                StartCoroutine( MantleDelay() );
                Debug.Log( "Tried to mantle" );
            }
        }
        
        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }

    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }


    static private RippleEffect _RIPPLE_EFFECT;
    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        
        // The two lines that follow replace the much less efficient commented-out line below them - JGB 2025-07-29
        // This is more efficient because it will only call FindObjectOfType if _RIPPLE_EFFECT is null (i.e., has never been set)
        // Once _RIPPLE_EFFECT is not null, FindObjectOfType will never be called again. - Jeremy
        // Also replaced the deprecated (de-pre-cated, not "depreciated") FindObjectOfType calls. - JGB 2025-07-29
        _RIPPLE_EFFECT ??= FindFirstObjectByType<RippleEffect>(); // FindObjectOfType<RippleEffect>(); 
        _RIPPLE_EFFECT?.Emit(Camera.main.WorldToViewportPoint(transform.position));
        // FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.linearVelocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.linearVelocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
        
        SoundAndMusic.Play( eAudioTrigger.dash );
    }

    static private GhostTrail _GHOST_TRAIL;
    IEnumerator DashWait()
    {
        // The two lines that follow replace the much less efficient commented-out line below them - JGB 2025-07-29
        _GHOST_TRAIL ??= FindFirstObjectByType<GhostTrail>(); // FindObjectOfType<RippleEffect>(); 
        _GHOST_TRAIL?.ShowGhost();
        // FindObjectOfType<GhostTrail>().ShowGhost();
        
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    IEnumerator MantleDelay() {
        yield return new WaitForSeconds( mantleTime );
        isMantling = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
            anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = (rb.linearVelocity.x > 0 && coll.onRightWall) || (rb.linearVelocity.x < 0 && coll.onLeftWall);
        float push = pushingWall ? 0 : rb.linearVelocity.x;

        rb.linearVelocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, (new Vector2(dir.x * speed, rb.linearVelocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    
    
    private void Jump(Vector2 dir, bool wall)
    {
        // NOTE: Much better jump code can be derived from watching Math for Game Programmers: Building a Better Jump
        // https://www.youtube.com/watch?v=hG9SzQxaCm8&t=9m35s & https://www.youtube.com/watch?v=hG9SzQxaCm8&t=784s
        // Th = Xh/Vx     V0 = 2H / Th     G = -2H / (Th * Th)     V0 = 2HVx / Xh     G = -2H(Vx*Vx) / (Xh*Xh) 
        // This allows you to specifically set the max jump height and distance. I haven't had a chance to implement
        // it in this project yet, but if you have questions, I'm happy to share my implementation for a different project.
        //   – Jeremy @GameProfBond
        
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity += dir * jumpForce;

        particle.Play();
        
        SoundAndMusic.Play( eAudioTrigger.jump );
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.linearDamping = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
