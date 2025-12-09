using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject chasingObject;
    [SerializeField] private GameObject idleObject;

    [SerializeField] private GameObject hittingObject;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCooldown;
    [SerializeField] private float forwardShotForce = 10f;
    [SerializeField] private float verticalShotForce = 5f;

    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    [Header("Hitting Settings")]
    [SerializeField] private float hittingDamageInterval = 1f;
    private float hittingDamageTimer;

    private bool isPlayerVisible;
    private bool isPlayerInRange;
    private bool isInEnemyCircle;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        hittingDamageTimer = hittingDamageInterval;
    }

    private void Update()
    {
        DetectPlayer();
        UpdateBehaviourState();
        UpdateHittingDamage();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyCircle"))
        {
            isInEnemyCircle = true;
            
            if (hittingObject != null)
            {
                hittingObject.SetActive(true);
            }

            hittingDamageTimer = hittingDamageInterval;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyCircle"))
        {
            isInEnemyCircle = false;
            
            if (hittingObject != null)
            {
                hittingObject.SetActive(false);
            }
        }
    }

    private void UpdateHittingDamage()
    {
        if (!isInEnemyCircle) return;

        hittingDamageTimer -= Time.deltaTime;

        if (hittingDamageTimer <= 0f)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);

            hittingDamageTimer = hittingDamageInterval;
        }
    }

    private void DetectPlayer()
    {
        isPlayerVisible = Physics.CheckSphere(transform.position, visionRange, playerLayerMask);
        isPlayerInRange = Physics.CheckSphere(transform.position, engagementRange, playerLayerMask);
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Rigidbody projectileRb = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        projectileRb.AddForce(transform.forward * forwardShotForce, ForceMode.Impulse);
        projectileRb.AddForce(transform.up * verticalShotForce, ForceMode.Impulse);

        Destroy(projectileRb.gameObject, 3f);
    }

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 potentialPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(potentialPoint, -transform.up, 2f, terrainLayer))
        {
            currentPatrolPoint = potentialPoint;
            hasPatrolPoint = true;
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;
    }

    private void PerformChase()
    {
        if (playerTransform != null && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(playerTransform.position);
        }

        SetAnimationState(true);
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
            FindPatrolPoint();

        if (hasPatrolPoint && navAgent != null && navAgent.isOnNavMesh)
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            hasPatrolPoint = false;

        SetAnimationState(false);
    }

    private void PerformAttack()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
            navAgent.SetDestination(transform.position);

        if (playerTransform != null)
        {
            transform.LookAt(playerTransform);
        }

        SetAnimationState(false);

        if (!isOnAttackCooldown)
        {
            FireProjectile();
            
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);
            
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private void PerformIdle()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
            navAgent.SetDestination(transform.position);
        
        SetAnimationState(false);
    }

    private void SetAnimationState(bool isChasing)
    {
        if (chasingObject != null)
            chasingObject.SetActive(isChasing);

        if (idleObject != null)
            idleObject.SetActive(!isChasing);
    }

    private void UpdateBehaviourState()
    {
        if (isInEnemyCircle)
        {
            if (chasingObject != null)
                chasingObject.SetActive(false);
            if (idleObject != null)
                idleObject.SetActive(false);
            
            if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                navAgent.SetDestination(transform.position);
            
            return;
        }

        if (!isPlayerVisible && !isPlayerInRange)
        {
            PerformPatrol();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if (isPlayerVisible && isPlayerInRange)
        {
            PerformAttack();
        }
    }
}