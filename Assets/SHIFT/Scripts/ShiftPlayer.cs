using System.Collections.Generic;
using UnityEngine;

namespace ShiftPrototype
{
    // Movement Controller
    // Physics stay Unity2D and are values are converted to Unity units
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class ShiftPlayer : MonoBehaviour
    {
        public ShiftWorld world;
        [Header("Laufen")]
        [Min(0.1f)] public float moveSpeed = 7.5f;
        [Min(0.1f)] public float acceleration = 83.3333f;
        [Range(0.1f, 1f)] public float airControl = 0.65f;
        [Min(0.1f)] public float overspeedDeceleration = 33.3333f;

        [Header("Sprung")]
        [Min(0.1f)] public float jumpSpeed = 8.75f;
        [Min(0)] public float jumpHorizontalBoost = 3.3333f;
        [Min(0)] public float variableJumpTime = 0.2f;
        [Min(0.1f)] public float gravityAcceleration = 75f;
        [Min(0.1f)] public float maxFallSpeed = 13.3333f;
        [Min(0)] public float apexSpeedThreshold = 3.3333f;
        [Range(0.1f, 1f)] public float apexGravityMultiplier = 0.5f;

        [Header("Sprunghilfen")]
        [Min(0)] public float coyoteTime = 0.1f;
        [Min(0.01f)] public float jumpBuffer = 0.12f;
        public bool cornerCorrection = true;
        [Range(0, 0.3f)] public float maxCornerCorrection = 0.16f;
        public LayerMask solidLayers = ~0;

        public BoxCollider2D BodyCollider { get; private set; }
        public bool Grounded { get; private set; }
        Rigidbody2D body;
        ContactFilter2D solidFilter;
        readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
        float moveInput, jumpQueued, grace, holdRemaining;
        bool jumpHeld, releasedSinceStep;
        const float Skin = 0.01f;

        public Vector2 Velocity
        {
            get
            {
                return body.linearVelocity;
            }
            private set
            {
                body.linearVelocity = value;
            }
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            BodyCollider = GetComponent<BoxCollider2D>();
            body.freezeRotation = true;
            body.gravityScale = 0;
            body.linearDamping = 0;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            solidFilter = new ContactFilter2D { useTriggers = false };
        }

        void Update()
        {
            if (world != null && ShiftInput.ResetDown)
            {
                if (world.Completed) world.Restart(); else world.Respawn();
                return;
            }
            if (world != null && world.Completed) return;
            moveInput = ShiftInput.Move.x;
            jumpHeld = ShiftInput.JumpHeld;
            // Save input in Update
            if (ShiftInput.JumpDown) jumpQueued = jumpBuffer;
            if (ShiftInput.JumpUp) releasedSinceStep = true;
            if (world != null && ShiftInput.SwapDown) world.TrySwap();
        }

        void FixedUpdate()
        {
            if (world != null && world.Completed) return;
            if (world != null && body.position.y < -8f) { world.Respawn(); return; }
            float dt = Time.fixedDeltaTime;
            solidFilter.SetLayerMask(solidLayers);
            Vector2 v = Velocity;
            Grounded = v.y <= 0.01f && HasSurface(Vector2.down, 0.04f, Vector2.up);
            grace = Grounded ? coyoteTime : Mathf.Max(0, grace - dt);

            // Releasing stops hold, even when pressed again
            if (releasedSinceStep || !jumpHeld) holdRemaining = 0;
            releasedSinceStep = false;

            // Full control on ground, a bit less in the air
            float rate = acceleration;
            if (Mathf.Abs(v.x) > moveSpeed && Mathf.Sign(v.x) == moveInput)
                rate = overspeedDeceleration;
            v.x = Mathf.MoveTowards(v.x, moveInput * moveSpeed, rate * (Grounded ? 1 : airControl) * dt);

            if (Grounded)
            {
                v.y = 0;
                holdRemaining = 0;
            }
            else
            {
                // Less Gravity around the apex, as long as jump is held
                float multiplier = jumpHeld && Mathf.Abs(v.y) < apexSpeedThreshold ? apexGravityMultiplier : 1;
                v.y = Mathf.MoveTowards(v.y, -maxFallSpeed, gravityAcceleration * multiplier * dt);
                if (holdRemaining > 0.0001f && jumpHeld)
                {
                    // keep upwards momentum when jump is held for a short time
                    v.y = Mathf.Max(v.y, jumpSpeed);
                    holdRemaining = Mathf.Max(0, holdRemaining - dt);
                }
            }

            if (jumpQueued > 0 && (Grounded || grace > 0))
            {
                v.y = jumpSpeed;
                v.x += moveInput * jumpHorizontalBoost;
                holdRemaining = jumpHeld ? Mathf.Max(0, variableJumpTime - dt) : 0;
                jumpQueued = grace = 0;
                Grounded = false;
            }
            else jumpQueued = Mathf.Max(0, jumpQueued - dt);

            if (v.y > 0 && cornerCorrection) CorrectCorner(v, dt);
            Velocity = v;
        }

        // Small Box Inner Corner Collision Detection
        Vector2 QuerySize => new Vector2(
            Mathf.Max(0.01f, BodyCollider.bounds.size.x - 2 * Skin),
            Mathf.Max(0.01f, BodyCollider.bounds.size.y - 2 * Skin));

        bool IsSolid(Collider2D other)
        {
            return other != null && !other.isTrigger && other.attachedRigidbody != body
                && !Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)
                && !Physics2D.GetIgnoreCollision(BodyCollider, other);
        }

        bool HasSurface(Vector2 direction, float distance, Vector2 normal)
        {
            Physics2D.BoxCast(BodyCollider.bounds.center, QuerySize, 0, direction, solidFilter, hits, distance);
            foreach (RaycastHit2D hit in hits)
                if (IsSolid(hit.collider) && Vector2.Dot(hit.normal, normal) > 0.7f) return true;
            return false;
        }

        bool PathClear(Vector2 origin, Vector2 movement)
        {
            if (movement.sqrMagnitude < 0.000001f) return true;
            Physics2D.BoxCast(origin, QuerySize, 0, movement.normalized, solidFilter, hits, movement.magnitude + Skin);
            foreach (RaycastHit2D hit in hits)
                if (IsSolid(hit.collider)) return false;
            return true;
        }

        void CorrectCorner(Vector2 velocity, float dt)
        {
            if (maxCornerCorrection <= 0 || !HasSurface(Vector2.up, velocity.y * dt + Skin, Vector2.down)) return;
            Vector2 center = BodyCollider.bounds.center;
            // just correct in direction of movement, test both sides if no sidewards movement
            int first = velocity.x < 0 ? -1 : 1;
            for (int i = 1; i <= 4; i++)
            {
                float distance = maxCornerCorrection * i / 4;
                for (int side = 0; side < 2; side++)
                {
                    if (side == 1 && Mathf.Abs(velocity.x) > 0.01f) break;
                    Vector2 offset = Vector2.right * distance * (side == 0 ? first : -first);
                    // the sidewards way and the upcoming physics step must be free
                    if (!PathClear(center, offset) || !PathClear(center + offset, velocity * dt)) continue;
                    body.position += offset;
                    Physics2D.SyncTransforms();
                    return;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision) { CancelHoldAtCeiling(collision); }
        void OnCollisionStay2D(Collision2D collision) { CancelHoldAtCeiling(collision); }
        void CancelHoldAtCeiling(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y < -0.7f) { holdRemaining = 0; return; }
        }

        public void Teleport(Vector2 position)
        {
            body.simulated = true;
            body.position = position;
            transform.position = new Vector3(position.x, position.y, 0);
            Velocity = Vector2.zero;
            body.gravityScale = 0;
            jumpQueued = grace = holdRemaining = moveInput = 0;
            jumpHeld = releasedSinceStep = Grounded = false;
            Physics2D.SyncTransforms();
        }
        public void Stop()
        {
            Velocity = Vector2.zero;
            body.simulated = false;
        }

        public void ApplyCelesteJumpPreset()
        {
            // the values from Player.cs scaled with 12 pixels per unity unit
            // World-Reference and Collision-Layer are kept
            moveSpeed = 7.5f; acceleration = 83.3333f; airControl = 0.65f;
            overspeedDeceleration = 33.3333f; jumpSpeed = 8.75f;
            jumpHorizontalBoost = 3.3333f; variableJumpTime = 0.2f;
            gravityAcceleration = 75f; maxFallSpeed = 13.3333f;
            apexSpeedThreshold = 3.3333f; apexGravityMultiplier = 0.5f;
            coyoteTime = 0.1f; jumpBuffer = 0.12f;
            cornerCorrection = true; maxCornerCorrection = 0.16f;
        }
    }
}
