using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrolSeeker : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Patrol")]
    public Transform waypointRoot;     
    public float waitAtPointSeconds = 1.0f;
    public bool loop = true;
    public float patrolSpeed = 2.5f;

    [Header("Random Patrol")]
    public bool useRandomPatrol = true;     
    public Transform wanderCenter;         
    public float wanderRadius = 25f;        
    public float wanderStopDistance = 0.3f; 

    [Header("Game Start & Dialogue")]
    public bool waitForGameStart = true;     
    public bool alignWithPlayerAtStart = false; 
    public Transform playerTransform;       
    public bool followPlayerDuringIntro = true; 
    public float introFollowDistance = 0.2f;  
    public float introFollowSpeed = 3.0f;      

    public enum IntroPlacement { None, BesidePlayer, FaceToFace }
    [Header("Intro Placement")]
    public IntroPlacement introPlacement = IntroPlacement.BesidePlayer; 
    public bool preferRightSide = true;        
    public float introSpacing = 1.0f;          

    [Header("Movement & Rotation")]
    public bool agentHandlesRotation = false; 
    public float turnSpeedDegrees = 720f;    

    [Header("Perception & Chase")]
    public float detectionRange = 15f;  
    [Range(0f, 180f)] public float detectionAngle = 120f; 
    public LayerMask obstacleMask;         
    public float chaseSpeed = 3.5f;
    public float pickDistance = 1.2f;   

    [Header("Animator Parameters (Optional)")]
    public string speedParam = "Speed";     

    private readonly List<Vector3> _points = new List<Vector3>();
    private int _index = 0;
    private float _waitTimer = 0f;
    

    private enum State { Patrol, Chase }
    private State _state = State.Patrol;

    private Transform _tissueTarget;
    private bool _gameActive = false;       
    private float _visibleConfirmTimer = 0f;
    private float _gameStartTime = -1f;

    [Header("Perception Confirm")]
    public float sightConfirmSeconds = 1.25f;
    public float detectGraceSeconds = 2.5f;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        CollectWaypoints();
    }

    void Start()
    {
        var tissue = Tissue.Instance;
        if (tissue != null)
        {
            _tissueTarget = tissue.transform;
        }

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.updateRotation = agentHandlesRotation;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            if (alignWithPlayerAtStart && playerTransform != null)
            {
                transform.position = playerTransform.position;
                transform.rotation = playerTransform.rotation;
            }

            if (waitForGameStart)
            {
                agent.isStopped = true;
                _gameActive = false;
            }
            else
            {
                _gameActive = true;
                if (!useRandomPatrol && _points.Count > 0) GoToNextPoint();
                else GoToRandomPoint();
            }
        }
    }

    void Update()
    {
        if (animator != null && agent != null && !string.IsNullOrEmpty(speedParam))
        {
            animator.SetFloat(speedParam, agent.velocity.magnitude);
        }

        if (Tissue.Instance != null)
        {
            var f = typeof(Tissue).GetField("hasGameEnded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null && (bool)f.GetValue(Tissue.Instance)) return;
        }

        if (waitForGameStart)
        {
            if (!_gameActive)
            {
                if (playerTransform != null && agent != null)
                {
                    if (introPlacement != IntroPlacement.None)
                    {
                        IntroPlacementUpdate();
                    }
                    else if (followPlayerDuringIntro)
                    {
                        agent.isStopped = false;
                        agent.speed = introFollowSpeed;
                        agent.SetDestination(playerTransform.position);

                        if (!agentHandlesRotation)
                        {
                            Vector3 dir = agent.desiredVelocity; dir.y = 0f;
                            if (dir.sqrMagnitude > 0.0001f)
                            {
                                Quaternion targetRot = Quaternion.LookRotation(dir);
                                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
                            }
                        }
                    }
                }

                if (InputManager.CanControl)
                {
                    _gameActive = true;
                    _gameStartTime = Time.time;
                    agent.isStopped = false;
                    if (!useRandomPatrol && _points.Count > 0) GoToNextPoint();
                    else GoToRandomPoint();
                }
                return;
            }
        }

        if (_tissueTarget != null)
        {
            if (_gameStartTime > 0f && Time.time - _gameStartTime < detectGraceSeconds)
            {
                _visibleConfirmTimer = 0f;
            }
            else if (CanSeeTissue())
            {
                _visibleConfirmTimer += Time.deltaTime;
                if (_visibleConfirmTimer >= sightConfirmSeconds)
                {
                    _state = State.Chase;
                }
            }
            else
            {
                _visibleConfirmTimer = 0f;
            }
        }

        switch (_state)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
        }
    }

    private void IntroPlacementUpdate()
    {
        if (playerTransform == null || agent == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f; playerForward.Normalize();
        Vector3 playerRight = new Vector3(playerForward.z, 0f, -playerForward.x);

        Vector3 target = playerPos;

        if (introPlacement == IntroPlacement.BesidePlayer)
        {
            Vector3 side = (preferRightSide ? playerRight : -playerRight) * introSpacing;
            Vector3 candidate = playerPos + side;
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                candidate = playerPos - side;
                if (!NavMesh.SamplePosition(candidate, out hit, 1.5f, NavMesh.AllAreas))
                {
                    NavMesh.SamplePosition(playerPos + side * 0.5f, out hit, 2f, NavMesh.AllAreas);
                }
            }
            target = hit.position;

            if (!agentHandlesRotation)
            {
                Quaternion targetRot = Quaternion.LookRotation(playerForward);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
            }
        }
        else if (introPlacement == IntroPlacement.FaceToFace)
        {
            Vector3 back = -playerForward * introSpacing;
            Vector3 candidate = playerPos + back;
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                NavMesh.SamplePosition(playerPos + back * 0.5f, out hit, 2f, NavMesh.AllAreas);
            }
            target = hit.position;

            if (!agentHandlesRotation)
            {
                Vector3 lookDir = playerPos - transform.position; lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
                }
            }
        }

        agent.isStopped = false;
        agent.speed = introFollowSpeed;
        agent.SetDestination(target);

        Vector3 flatSelf = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(target.x, 0f, target.z);
        float dist = Vector3.Distance(flatSelf, flatTarget);
        if (dist <= Mathf.Max(introFollowDistance, agent.stoppingDistance))
        {
            agent.isStopped = true;
        }
    }

    private void PatrolUpdate()
    {
        if (agent == null) return;

        agent.speed = patrolSpeed;
        TryUnstuck();

        if (!agentHandlesRotation)
        {
            Vector3 dir = agent.desiredVelocity;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
            }
        }

        if (useRandomPatrol || _points.Count == 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= wanderStopDistance)
            {
                if (_waitTimer <= 0f)
                {
                    _waitTimer = waitAtPointSeconds;
                }
                else
                {
                    _waitTimer -= Time.deltaTime;
                    if (_waitTimer <= 0f)
                    {
                        GoToRandomPoint();
                    }
                }
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (_waitTimer <= 0f)
            {
                _waitTimer = waitAtPointSeconds;
            }
            else
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    GoToNextPoint();
                }
            }
        }
    }

    private void ChaseUpdate()
    {
        if (agent == null || _tissueTarget == null) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(_tissueTarget.position);
        TryUnstuck();

        if (!agentHandlesRotation)
        {
            Vector3 dir = agent.desiredVelocity;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
            }
        }

        var dist = Vector3.Distance(transform.position, _tissueTarget.position);
        if (dist <= pickDistance)
        {
            if (Tissue.Instance != null)
            {
                Tissue.Instance.TimeUp();
            }
            _state = State.Patrol;
        }
    }

    private float _stuckTime = 0f;
    private void TryUnstuck()
    {
        if (agent == null) return;
        bool shouldMove = !agent.pathPending && agent.remainingDistance > agent.stoppingDistance;
        float speed = agent.velocity.magnitude;
        if (shouldMove && speed < 0.05f)
        {
            _stuckTime += Time.deltaTime;
            if (_stuckTime > 1.0f)
            {
                agent.ResetPath();
                Vector3 jitter = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
                Vector3 target = ( _state == State.Chase && _tissueTarget != null ) ? _tissueTarget.position : agent.destination;
                Vector3 candidate = target + jitter;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                _stuckTime = 0f;
            }
        }
        else
        {
            _stuckTime = 0f;
        }
    }

    private bool CanSeeTissue()
    {
        var selfPos = transform.position;
        var targetPos = _tissueTarget.position;

        var dir = (targetPos - selfPos);
        var dist = dir.magnitude;
        if (dist > detectionRange) return false;
        dir.Normalize();

        var angle = Vector3.Angle(transform.forward, dir);
        if (angle > detectionAngle * 0.5f) return false;

        int mask = obstacleMask.value != 0 ? obstacleMask.value : ~0;
        if (Physics.Raycast(selfPos + Vector3.up * 1.6f, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
        {
            var tissueComp = hit.collider.GetComponentInParent<Tissue>();
            if (tissueComp == null)
            {
                return false;
            }
        }
        return true;
    }

    private void GoToNextPoint()
    {
        if (_points.Count == 0 || agent == null) return;
        agent.SetDestination(_points[_index]);
        _index = (_index + 1);
        if (loop) _index %= _points.Count; else _index = Mathf.Min(_index, _points.Count - 1);
    }

    private void GoToRandomPoint()
    {
        if (agent == null) return;

        Vector3 center = wanderCenter != null ? wanderCenter.position : transform.position;
        Vector3 target;
        if (TryGetRandomNavmeshPoint(center, wanderRadius, out target))
        {
            agent.SetDestination(target);
        }
        else
        {
            if (NavMesh.SamplePosition(center, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private bool TryGetRandomNavmeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector2 circle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(center.x + circle.x, center.y, center.z + circle.y);
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    private void CollectWaypoints()
    {
        _points.Clear();
        if (waypointRoot == null)
        {
            foreach (Transform child in transform)
            {
                _points.Add(child.position);
            }
        }
        else
        {
            foreach (Transform child in waypointRoot)
            {
                _points.Add(child.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _points.Count; i++)
        {
            Gizmos.DrawSphere(_points[i], 0.2f);
            var next = (i + 1) % _points.Count;
            if (_points.Count > 1) Gizmos.DrawLine(_points[i], _points[next]);
        }

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
        var left = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.up) * transform.forward;
        var right = Quaternion.AngleAxis(detectionAngle * 0.5f, Vector3.up) * transform.forward;
        Gizmos.DrawRay(transform.position, left * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);

        if (useRandomPatrol)
        {
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);
            Vector3 center = wanderCenter != null ? wanderCenter.position : transform.position;
            Gizmos.DrawWireSphere(center, wanderRadius);
        }
    }
}