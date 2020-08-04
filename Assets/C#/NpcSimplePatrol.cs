using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcSimplePatrol : MonoBehaviour
{
    [SerializeField]
    bool _patrolWaiting;

    [SerializeField]
    float _totalWaitTime = 3f;

    [SerializeField]
    float _switchProbability = 0.2f;

    [SerializeField]
    List<Waypoint> _patrolPoints;

    NavMeshAgent _navMeshAgent;
    int _currentPatrolIndex;
    bool _travelling;
    bool _waiting;
    bool _patrolForward;
    float _waitTimer;

    // Use this for initialization
    void Start()
    {
        _navMeshAgent = this.GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogError("The nav mesh agent component is not attached to" + gameObject.name);
        }
        else
        {
            if(_patrolPoints != null && _patrolPoints.Count >= 2)
            {
                _currentPatrolIndex = 0;
                SetDestination();
            }
            else
            {
                Debug.Log("Insufficient patrol points for basic patrolling behaviour.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check if we are close to the destination.
        if(_travelling && _navMeshAgent.remainingDistance <= 1.0f)
        {
            _travelling = false;

            if(_patrolWaiting)
            {
                _waiting = true;
                _waitTimer = 0f;
            }
            else
            {
                ChangePatrolPoint();
                SetDestination();
            }
        }

        //Instead if we are waiting.
        if(_waiting)
        {
            _waitTimer += Time.deltaTime;
            if(_waitTimer >= _totalWaitTime)
            {
                _waiting = false;

                ChangePatrolPoint();
                SetDestination();
                
            }
        }
    }

    

    private void SetDestination()
    {
        if (_patrolPoints != null)
        {
            Vector3 targetVector = _patrolPoints[_currentPatrolIndex].transform.position;
            _navMeshAgent.SetDestination(targetVector);
            _travelling = true;
        }
    }

    /// <summary>
    /// Selects a new patrol point in the available list, but
    /// also with a small probability allows for us to move forward ot backwards.
    /// </summary>
    private void ChangePatrolPoint()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= _switchProbability)
        {
            _patrolForward = !_patrolForward;
        }

        if (_patrolForward)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        }
        else
        {
            if (--_currentPatrolIndex < 0)
            {
                _currentPatrolIndex = _patrolPoints.Count - 1;
            }
        }
    }
}
