using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float freezeDistance = 15f;
    public float killDistance = 1.5f;
    public float detectionRange = 15f; 
    public float patrolRadius = 20f;
    
    private NavMeshAgent agent;
    private Camera playerCam;
    private Animator anim;
    
    private enum State { Patrol, Chase }
    private State currentState = State.Patrol;

    private Vector3 patrolTarget;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerCam = Camera.main;
        anim = GetComponent<Animator>();
        
        // Начинаем патрулировать
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (player == null) return;

        // 1. Проверяем, смотрит ли игрок на врага (Плачущий ангел)
        Vector3 screenPoint = playerCam.WorldToViewportPoint(transform.position);
        bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 2. Логика состояний
        if (currentState == State.Patrol)
        {
            PatrolLogic(distanceToPlayer);
        }
        else if (currentState == State.Chase)
        {
            ChaseLogic(distanceToPlayer);
        }

        // --- ЛОГИКА ЗАМИРАНИЯ (Плачущий ангел) ---
        // Он замирает, ТОЛЬКО если игрок смотрит на него И он находится в радиусе freezeDistance
        if (isVisible && distanceToPlayer <= freezeDistance)
        {
            agent.isStopped = true; 
            if (anim != null) anim.speed = 0f; 
        }
        else
        {
            // Во всех остальных случаях (отвернулся или он далеко) он идет
            agent.isStopped = false; 
            if (anim != null) anim.speed = 1f; 
        }

        // 4. Включаем анимацию ходьбы, если он реально движется
        if (anim != null && anim.speed > 0f)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            anim.SetBool("isWalking", isMoving);
        }

        // 5. Смерть игрока
        if (distanceToPlayer < killDistance)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    // --- ПАТРУЛИРОВАНИЕ ---
    void PatrolLogic(float distanceToPlayer)
    {
        // Просто проверяем дистанцию без всяких лучей
        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        // Движение к точке патруля
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 3f) 
            {
                SetNewPatrolTarget();
                waitTimer = 0f;
            }
        }
    }

    // --- ПОГОНЯ ---
    void ChaseLogic(float distanceToPlayer)
    {
        // Если игрок слишком далеко — теряем его
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = State.Patrol;
            SetNewPatrolTarget();
            return;
        }

        // Двигаемся к игроку
        agent.SetDestination(player.position);

        // Если игрок залез в баг или на крышу (нет пути)
        if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            currentState = State.Patrol;
            SetNewPatrolTarget();
        }
    }

    // --- ВЫБОР СЛУЧАЙНОЙ ТОЧКИ ДЛЯ ПАТРУЛЯ ---
    void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        
        if (NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, -1))
        {
            patrolTarget = navHit.position;
            agent.SetDestination(patrolTarget);
        }
    }
}