using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    patrol,
    alert,
    followPlayer,
    attacking,
    searching
}

public class EnemyMotion : MonoBehaviour
{
    public EnemyState state;
    public Transform pointOfView;
    public GameObject target;
    public Transform player;
    public Transform[] waypoints;
    public LayerMask playerMask, visibleMask;
    public float viewDistance, speedNormal, speedCombat, angularNormal, angularCombat, stoppingDistance, timeToSearching, radius;
    [Range(0, 360)]
    public float angle;
    public int waypointN;
    public bool playerDetected, stop, run;
    EnemyCombat enemyCombat;
    NavMeshAgent agent;
    Animator anim;
    Sequence sequence;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        enemyCombat = GetComponent<EnemyCombat>();
    }
    private void Start()
    {
        agent.stoppingDistance = 1;
        agent.SetDestination(waypoints[waypointN].position);
    }
    private void Update()
    {
        if (stop) return;
        MachineState();
        anim.SetBool("Move", run);
    }

    private void MachineState()
    {
        switch (state)
        {
            case EnemyState.patrol:
                playerDetected = OnPlayerDetect();
                if (!playerDetected)
                {
                    run = (Vector3.Distance(waypoints[waypointN].position, transform.position) > agent.stoppingDistance);
                    if (!run)
                    {
                        Stopping();
                        Vector3 target = waypoints[waypointN].rotation.eulerAngles;
                        Vector3 lookPos = target - transform.position;
                        lookPos.y = 0;

                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        float dirAnim = (rotation.x > 0) ? 1 : -1;
                        anim.SetFloat("Direction", dirAnim);
                        StartCoroutine("NextWaypoint");
                        return;
                    }
                }
                else
                {
                    Stopping();
                    anim.SetBool("Enconter", true);
                    sequence = DOTween.Sequence();
                    sequence.AppendInterval(1).OnComplete(() =>
                    {
                        anim.SetBool("Enconter", false);
                        Enconter();
                    });
                }
                break;
            case EnemyState.alert:
                run = false;
                playerDetected = OnPlayerDetect();
                if (playerDetected)
                {
                    ResetEnemy();
                    Stopping();
                    anim.Rebind();
                    anim.SetBool("Enconter", true);
                    sequence = DOTween.Sequence();
                    sequence.AppendInterval(1).OnComplete(() =>
                    {
                        anim.SetBool("Enconter", false);
                        Enconter();
                    });
                }
                break;
            case EnemyState.followPlayer:
                if (agent.pathPending)
                {
                    timeToSearching = 0;
                    bool playerView = PlayerDirect();
                    if (!playerView)
                    {
                        agent.ResetPath();
                        player = null;
                        Stopping();
                        StartCoroutine("NextWaypoint");
                        return;
                    }
                }
                if (timeToSearching > 30)
                {
                    timeToSearching = 0;
                    bool playerView = PlayerDirect();
                    if (!playerView)
                    {
                        agent.ResetPath();
                        player = null;
                        Stopping();
                        StartCoroutine("NextWaypoint");
                        return;
                    }
                    else
                    {
                        timeToSearching += Time.deltaTime;
                    }

                    if (agent.isStopped) agent.isStopped = false;
                    if (player == null) OnPlayerDetect();

                    agent.SetDestination(player.position);
                    run = (Vector3.Distance(player.position, transform.position) > agent.stoppingDistance);

                    if (Vector3.Distance(player.position, transform.position) <= agent.stoppingDistance)
                    {
                        Stopping();
                        state = EnemyState.attacking;

                        Vector3 target = player.transform.position;
                        Vector3 lookPos = target - transform.position;
                        lookPos.y = 0;

                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        transform.rotation = rotation;
                        enemyCombat.Attack();
                    }
                }
                break;
        }
    }

    private bool OnPlayerDetect()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, playerMask);
        if (rangeChecks.Length != 0)
        {
            RaycastHit hit;
            Transform playerT = rangeChecks[0].transform;
            Vector3 directionToTarget = ((playerT.position + (Vector3.up * 1.5f)) - pointOfView.position);
            if (Vector3.Angle(pointOfView.forward, directionToTarget) < angle / 2)
            {
                if (Physics.Raycast(pointOfView.position, directionToTarget, out hit, viewDistance, visibleMask))
                {
                    if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Shield"))
                    {
                        return false;
                    }

                    player = (hit.collider.CompareTag("Shield")) ? hit.collider.GetComponentInParent<PlayerMotion>().transform : hit.collider.transform;
                    return true;
                }
            }
        }
        player = null;
        return false;
    }

    private bool PlayerDirect()
    {
        if (player == null) return false;

        RaycastHit hit;
        Vector3 directionToTarget = ((player.position + (Vector3.up * 1.5f)) - pointOfView.position);
        if (Physics.Raycast(pointOfView.position, directionToTarget, out hit, viewDistance, visibleMask))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Shield"))
            {
                return true;
            }
        }
        return false;
    }

    public void Enconter()
    {
        ResetEnemy();
        Stopping();
        state = EnemyState.searching;

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, playerMask);
        if (rangeChecks.Length != 0)
        {
            Vector3 target = rangeChecks[0].transform.position;
            Vector3 lookPos = target - transform.position;
            lookPos.y = 0;

            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = rotation;
            playerDetected = OnPlayerDetect();

            agent.speed = speedCombat;
            agent.angularSpeed = angularCombat;

            state = EnemyState.followPlayer;

            agent.stoppingDistance = stoppingDistance;
            StopEnd();
        }
        else
        {
            state = EnemyState.alert;
            StopEnd();
            anim.SetBool("Alert", true);
            sequence = DOTween.Sequence();
            sequence.AppendInterval(1).OnComplete(() =>
            {
                Stopping();
                anim.SetBool("Alert", false);
                state = EnemyState.patrol;

                agent.speed = speedNormal;
                agent.angularSpeed = angularNormal;
                agent.stoppingDistance = 1;

                agent.SetDestination(waypoints[waypointN].position);
                StopEnd();
            });
        }
    }

    public void Stopping()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        stop = true;
        run = false;

        anim.SetBool("Move", run);
    }

    public void StopEnd()
    {
        if (state == EnemyState.attacking)
        {
            agent.speed = speedCombat;
            agent.angularSpeed = angularCombat;

            state = EnemyState.followPlayer;
            agent.stoppingDistance = stoppingDistance;
        }
        stop = false;
        agent.isStopped = false;
    }

    public void ResetEnemy()
    {
        StopCoroutine("NextWaypoint");
        transform.DOKill();
        sequence.Kill();
    }

    IEnumerator NextWaypoint()
    {
        transform.DOLookAt(waypoints[waypointN].forward, 1, AxisConstraint.Y).OnComplete(() =>
        {
            anim.SetFloat("Direction", 0);
            state = EnemyState.alert;
            StopEnd();
            anim.SetBool("Alert", true);
        });
        yield return new WaitForSeconds(3.5f);
        Stopping();
        anim.SetBool("Alert", false);
        yield return new WaitForSeconds(1);
        state = EnemyState.patrol;
        agent.stoppingDistance = 1;
        agent.speed = speedNormal;
        agent.angularSpeed = angularNormal;

        if (waypointN == waypoints.Length - 1) waypointN = 0;
        else waypointN++;

        agent.SetDestination(waypoints[waypointN].position);
        StopEnd();
    }
}
