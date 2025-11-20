using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ZombieAI : MonoBehaviour
{
    [Header("AI Behavior")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float screamRange = 10f;
    public float walkSpeed = 1f;    // Slower walk speed to match animation
    public float runSpeed = 4f;     // Moderate run speed
    public float roamRadius = 20f;
    public Transform eyePosition; // Assign the camera/head position here
    public float fieldOfView = 120f; // Zombie's vision cone
    public float loseTargetTime = 3f; // How long before losing player
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public float attackAnimationDuration = 1.5f; // How long attack animation takes
    public float screamDuration = 2f; // How long scream lasts
    
    [Header("Roaming")]
    public float roamWaitTime = 3f;
    public bool shouldRoam = true;
    
    [Header("Debug")]
    public bool showDebug = false;
    
    // Components
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    
    // State
    private enum ZombieState { Idle, Roaming, Chasing, Attacking, Screaming }
    private ZombieState currentState = ZombieState.Idle;
    
    // Timers
    private float lastAttackTime;
    private float roamTimer;
    private bool hasScreamed;
    private Vector3 roamStartPosition;
    private float lastSeenPlayerTime;
    private Vector3 lastKnownPlayerPosition;
    private float screamStartTime;
    private float attackStartTime;
    private bool isAttacking = false;
    private bool isScreaming = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Check if NavMesh exists
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            Debug.LogError($"❌ No NavMesh found! Please bake NavMesh first. Disabling {gameObject.name}");
            enabled = false;
            return;
        }
        
        // Check animator
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"⚠ No Animator Controller assigned to {gameObject.name}. Animations won't work!");
        }
        
        // Configure NavMeshAgent to work with animations
        agent.updateRotation = false;  // We'll handle rotation manually
        agent.updatePosition = true;   // Let NavMesh handle position
        agent.speed = walkSpeed;
        agent.acceleration = 8f;       // Smooth acceleration
        agent.angularSpeed = 120f;     // Rotation speed
        agent.stoppingDistance = 0.1f; // Stop closer to destination
        
        // Auto-find eye position if not assigned
        if (eyePosition == null)
        {
            // Look for a camera in children
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                eyePosition = cam.transform;
                Debug.Log($"✓ Auto-assigned eye position to camera: {cam.name}");
            }
            else
            {
                // Use a position slightly above the zombie
                GameObject eyeObj = new GameObject("EyePosition");
                eyeObj.transform.parent = transform;
                eyeObj.transform.localPosition = new Vector3(0, 1.6f, 0); // Head height
                eyePosition = eyeObj.transform;
                Debug.Log($"✓ Created eye position at head height");
            }
        }
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure player has 'Player' tag.");
        }
        
        roamStartPosition = transform.position;
        
        if (shouldRoam)
        {
            currentState = ZombieState.Roaming;
            SetNewRoamDestination();
        }
        else
        {
            currentState = ZombieState.Idle;
        }
        
        Debug.Log($"✓ Zombie AI initialized: {gameObject.name}");
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Update state based on distance
        UpdateState(distanceToPlayer);
        
        // Execute behavior based on state
        ExecuteState();
        
        // Handle rotation towards movement direction
        RotateTowardsMovement();
        
        // Update animator
        UpdateAnimator();
    }
    
    void UpdateState(float distanceToPlayer)
    {
        // Priority: Attacking and Screaming lock other states
        if (isAttacking)
        {
            // Stay in attacking state until animation finishes
            if (Time.time - attackStartTime >= attackAnimationDuration)
            {
                isAttacking = false;
                
                // Check if player is still in attack range - attack again
                if (CanSeePlayer() && distanceToPlayer <= attackRange)
                {
                    currentState = ZombieState.Attacking;
                    isAttacking = true;
                    attackStartTime = Time.time;
                    lastAttackTime = Time.time;
                    
                    if (showDebug) Debug.Log("Player still near - ATTACKING AGAIN!");
                    return;
                }
                // Return to chasing if player is still visible but out of attack range
                else if (CanSeePlayer() && distanceToPlayer <= detectionRange)
                {
                    currentState = ZombieState.Chasing;
                }
                else
                {
                    hasScreamed = false; // Reset scream for next encounter
                    currentState = ZombieState.Roaming;
                    SetNewRoamDestination();
                }
            }
            return; // Don't process other state changes while attacking
        }
        
        if (isScreaming)
        {
            // Stay in screaming state for full duration
            if (Time.time - screamStartTime >= screamDuration)
            {
                isScreaming = false;
                // After scream, start chasing
                currentState = ZombieState.Chasing;
            }
            return; // Don't process other state changes while screaming
        }
        
        // Check if player is in sight with proper vision detection
        bool playerInSight = CanSeePlayer();
        
        // Update last seen time
        if (playerInSight)
        {
            lastSeenPlayerTime = Time.time;
            lastKnownPlayerPosition = player.position;
        }
        
        // Check if we've lost the player for too long
        bool hasLostPlayer = (Time.time - lastSeenPlayerTime) > loseTargetTime;
        
        // State transitions (priority order)
        
        // 1. Attack if player is very close
        if (playerInSight && distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            currentState = ZombieState.Attacking;
            isAttacking = true;
            attackStartTime = Time.time;
            lastAttackTime = Time.time;
            
            if (showDebug) Debug.Log("ATTACKING!");
        }
        // 2. Scream when first detecting player (only once per encounter)
        else if (playerInSight && distanceToPlayer <= screamRange && !hasScreamed && currentState != ZombieState.Chasing)
        {
            currentState = ZombieState.Screaming;
            isScreaming = true;
            screamStartTime = Time.time;
            hasScreamed = true;
            
            if (showDebug) Debug.Log("SCREAMING!");
        }
        // 3. Chase if player is in detection range (after scream or if already chasing)
        else if (playerInSight && distanceToPlayer <= detectionRange && !isScreaming)
        {
            currentState = ZombieState.Chasing;
        }
        // 4. Lost player - return to roaming
        else if (currentState == ZombieState.Chasing)
        {
            if (hasLostPlayer || distanceToPlayer > detectionRange * 1.5f)
            {
                hasScreamed = false; // Reset for next encounter
                
                if (shouldRoam)
                {
                    currentState = ZombieState.Roaming;
                    SetNewRoamDestination();
                }
                else
                {
                    currentState = ZombieState.Idle;
                }
                
                if (showDebug) Debug.Log("Lost player - returning to roam/idle");
            }
        }
        // 5. Continue roaming if that's the current state
        else if (currentState == ZombieState.Roaming)
        {
            // Roaming logic handled in ExecuteState
        }
        // 6. Idle state
        else if (currentState == ZombieState.Idle)
        {
            // Just stay idle
        }
    }
    
    void ExecuteState()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                // COMPLETELY stop all movement
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                
                // Occasionally look around
                if (Random.value < 0.01f && !isAttacking && !isScreaming)
                {
                    transform.Rotate(0, Random.Range(-30f, 30f), 0);
                }
                break;
                
            case ZombieState.Roaming:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                
                // Check if we have a valid path
                if (agent.hasPath)
                {
                    // Check if reached destination
                    if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                    {
                        // Wait at destination
                        roamTimer -= Time.deltaTime;
                        
                        if (roamTimer <= 0)
                        {
                            // Pick new destination
                            SetNewRoamDestination();
                        }
                    }
                }
                else
                {
                    // No path, set new destination immediately
                    SetNewRoamDestination();
                }
                break;
                
            case ZombieState.Chasing:
                agent.isStopped = false;
                agent.speed = runSpeed;
                
                // Only chase if we can see the player
                if (CanSeePlayer())
                {
                    agent.SetDestination(player.position);
                }
                else
                {
                    // Go to last known position
                    agent.SetDestination(lastKnownPlayerPosition);
                }
                break;
                
            case ZombieState.Attacking:
                // COMPLETELY stop - no movement during attack
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.speed = 0f; // Force zero speed
                
                LookAtPlayer();
                PerformAttack();
                break;
                
            case ZombieState.Screaming:
                // COMPLETELY stop - no movement during scream
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.speed = 0f; // Force zero speed
                
                LookAtPlayer();
                PerformScream();
                break;
        }
    }
    
    void UpdateAnimator()
    {
        // Calculate actual movement speed from the agent's velocity
        float currentSpeed = agent.velocity.magnitude;
        
        // Determine target speed based on state and actual movement
        float targetSpeed = 0f;
        
        // Force animation to match state precisely
        if (isAttacking || currentState == ZombieState.Attacking)
        {
            targetSpeed = 0f; // Idle animation during attack
        }
        else if (isScreaming || currentState == ZombieState.Screaming)
        {
            targetSpeed = 0f; // Idle animation during scream
        }
        else if (agent.isStopped || currentState == ZombieState.Idle || currentSpeed < 0.05f)
        {
            targetSpeed = 0f; // Idle
        }
        else if (currentState == ZombieState.Roaming || (currentState == ZombieState.Chasing && currentSpeed < 2f))
        {
            // Walking - scale speed to match actual velocity
            targetSpeed = Mathf.Clamp01(currentSpeed / walkSpeed) * 0.5f; // 0 to 0.5 for walk
        }
        else if (currentState == ZombieState.Chasing && currentSpeed >= 2f)
        {
            // Running - scale speed for run animation
            targetSpeed = 0.5f + Mathf.Clamp01((currentSpeed - walkSpeed) / (runSpeed - walkSpeed)) * 1.5f; // 0.5 to 2.0 for run
        }
        
        // Smoothly transition to target speed
        float currentAnimSpeed = animator.GetFloat("Speed");
        float smoothSpeed = Mathf.Lerp(currentAnimSpeed, targetSpeed, Time.deltaTime * 15f); // Faster lerp for responsiveness
        
        animator.SetFloat("Speed", smoothSpeed);
        
        if (showDebug)
        {
            Debug.Log($"State: {currentState} | Velocity: {currentSpeed:F2} | AnimSpeed: {smoothSpeed:F2} | Attacking: {isAttacking} | Screaming: {isScreaming}");
        }
        
        // Trigger animations only once when entering state
        if (currentState == ZombieState.Attacking && isAttacking && Time.time - attackStartTime < 0.1f)
        {
            animator.SetTrigger("AttackTrigger");
            if (showDebug) Debug.Log("Triggered Attack Animation");
        }
        
        if (currentState == ZombieState.Screaming && isScreaming && Time.time - screamStartTime < 0.1f)
        {
            animator.SetTrigger("ScreamTrigger");
            if (showDebug) Debug.Log("Triggered Scream Animation");
        }
    }
    
    void RotateTowardsMovement()
    {
        // Don't rotate if in attack/scream state
        if (isAttacking || isScreaming || currentState == ZombieState.Attacking || currentState == ZombieState.Screaming)
        {
            return; // LookAtPlayer() handles rotation for these states
        }
        
        if (currentState == ZombieState.Idle)
        {
            return; // Don't rotate when idle
        }
        
        // Get the movement direction from NavMeshAgent
        Vector3 velocity = agent.velocity;
        
        if (velocity.magnitude > 0.05f)
        {
            // Rotate towards movement direction
            Vector3 direction = velocity.normalized;
            direction.y = 0; // Keep rotation on horizontal plane only
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f); // Faster rotation
            }
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        // Use eye position for raycast
        Vector3 rayStart = eyePosition != null ? eyePosition.position : transform.position + Vector3.up * 1.6f;
        Vector3 directionToPlayer = (player.position - rayStart).normalized;
        float distanceToPlayer = Vector3.Distance(rayStart, player.position);
        
        // Check if player is within detection range
        if (distanceToPlayer > detectionRange) return false;
        
        // Check if player is within field of view
        Vector3 forward = eyePosition != null ? eyePosition.forward : transform.forward;
        float angle = Vector3.Angle(forward, directionToPlayer);
        
        if (angle > fieldOfView / 2f)
        {
            if (showDebug)
            {
                Debug.DrawLine(rayStart, player.position, Color.yellow);
            }
            return false; // Player is outside vision cone
        }
        
        // Raycast to check line of sight
        RaycastHit hit;
        if (Physics.Raycast(rayStart, directionToPlayer, out hit, detectionRange))
        {
            if (showDebug)
            {
                Debug.DrawLine(rayStart, hit.point, hit.transform.CompareTag("Player") ? Color.green : Color.red);
            }
            
            if (hit.transform == player || hit.transform.CompareTag("Player"))
            {
                return true; // Can see player
            }
        }
        
        return false; // Something is blocking view
    }
    
    bool IsPlayerInSight(float distance)
    {
        // Deprecated - use CanSeePlayer() instead
        return CanSeePlayer();
    }
    
    void SetNewRoamDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += roamStartPosition;
        randomDirection.y = roamStartPosition.y; // Keep on same Y level
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            roamTimer = roamWaitTime;
            currentState = ZombieState.Roaming;
            agent.speed = walkSpeed;
            
            if (showDebug)
            {
                Debug.Log($"Zombie roaming to: {hit.position} (Distance: {Vector3.Distance(transform.position, hit.position):F2})");
            }
        }
        else
        {
            // If can't find valid position, wait and try again
            roamTimer = 1f;
            if (showDebug)
            {
                Debug.LogWarning("Could not find valid roam position, retrying...");
            }
        }
    }
    
    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    
    void PerformAttack()
    {
        // Attack logic only executes once at the start
        if (Time.time - attackStartTime < 0.1f)
        {
            // Deal damage to player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // You can add player health script here
                if (showDebug)
                {
                    Debug.Log($"Zombie attacked player for {attackDamage} damage!");
                }
            }
        }
    }
    
    void PerformScream()
    {
        // Scream logic only executes once at the start
        if (Time.time - screamStartTime < 0.1f)
        {
            if (showDebug)
            {
                Debug.Log($"Zombie screamed at player! Will chase after {screamDuration} seconds.");
            }
        }
    }
    
    void ReturnToChasing()
    {
        // This method is no longer needed - handled in UpdateState
    }
    
    // Visualize detection ranges
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Scream range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, screamRange);
        
        // Roam radius
        if (shouldRoam)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(roamStartPosition, roamRadius);
        }
    }
}
