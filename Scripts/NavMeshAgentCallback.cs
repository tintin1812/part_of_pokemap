using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentCallback : MonoBehaviour
{
    public delegate void OnFinishPath(NavMeshAgent nav);

    private OnFinishPath mOnFinishPath = null;
    private Vector3 mDestination = Vector3.zero;
    private int mCountRepath = 0;
    private static float MIN_D = 2 * 2;

    private NavMeshAgent mNavMeshAgent;
    public NavMeshAgent NavMeshAgent
    {
        get
        {
            if (mNavMeshAgent == null)
            {
                mNavMeshAgent = GetComponent<NavMeshAgent>();
            }
            return mNavMeshAgent;
        }
    }
    public bool IsCanMove()
    {
        if (!gameObject.activeSelf || mNavMeshAgent == null || !mNavMeshAgent.isActiveAndEnabled || !mNavMeshAgent.isOnNavMesh || mNavMeshAgent.isStopped)
        {
            return false;
        }
        if (mWaitAction != Action.Non || mCurrentAction == Action.Pause || mCurrentAction == Action.Stop || mOnFinishPath != null)
        {
            return false;
        }
        return true;
    }
    private float mDelayCheck = 0;
    private Action mWaitAction = Action.Non;
    private Action mCurrentAction = Action.Non;

    private enum Action
    {
        Non,
        Pause,
        Resume,
        Stop,
    }

    public void WalkTo(float delay, Vector3 to, OnFinishPath onFinishPath)
    {
        float lastSpeed = NavMeshAgent.speed;
        NavMeshAgent.speed = 2;
        float lastStoppingDistance = NavMeshAgent.stoppingDistance;
        NavMeshAgent.stoppingDistance = 0.2f;
        this.GetComponent<BasicMecanimControl>().IsRun = false;
        this.DestinationTo(delay, to, (NavMeshAgent nav) =>
        {
            NavMeshAgent.speed = lastSpeed;
            NavMeshAgent.stoppingDistance = lastStoppingDistance;
            this.GetComponent<BasicMecanimControl>().IsRun = true;
            if (onFinishPath != null)
            {
                onFinishPath(nav);
            }
        });
    }

    public void DestinationTo(float delay, Vector3 to, OnFinishPath onFinishPath)
    {
        if(delay <= 0){
            mDelayCheck = -1;
            mOnFinishPath = onFinishPath;
            mDestination = to;
            mCountRepath = 0;
            SetDestination(to);
        }else
        {
            StopAllCoroutines();
            StartCoroutine(DestinationDelay( delay, to, onFinishPath));
        }
    }

    private void SetDestination(Vector3 to){
        StopAllCoroutines();
        NavMeshAgent.SetDestination(to);
    }

    private IEnumerator DestinationDelay(float delay, Vector3 to, OnFinishPath onFinishPath){
        yield return new WaitForSeconds(delay);
        mOnFinishPath = onFinishPath;
        mDestination = to;
        mCountRepath = 0;
        NavMeshAgent.SetDestination(to);
        yield break;
    }

    public void ClearDestination()
    {
        StopAllCoroutines();
        NavMeshAgent.ResetPath();
        mOnFinishPath = null;
        mDestination = Vector3.zero;
        mCountRepath = 0;
        // Debug.Log(gameObject.name + " ClearDestination");
    }

    public void ResetPath()
    {
        if(mOnFinishPath != null){
            Debug.LogWarning("Can't ResetPath when Wait Action");
            return;
        }
        StopAllCoroutines();
        NavMeshAgent.ResetPath();
    }

    public void PauseMove(float delay = -1)
    {
        if(!isHavePath()){
            return;
        }
        mDelayCheck = delay;
        mWaitAction = Action.Pause;
        // Debug.Log(gameObject.name + " Need_Pause");
    }

    public void ResumeMove(float delay = -1)
    {
        if(!isHavePath()){
            return;
        }
        mDelayCheck = delay;
        mWaitAction = Action.Resume;
        // Debug.Log(gameObject.name + " Need_Resume");
    }

    // public void StopMove(float delay = -1)
    // {
    //     if(mOnFinishPath != null){
    //         Debug.LogWarning("Can't stop when Wait Action");
    //         return;
    //     }
    //     mDelayUpdate = delay;
    //     mWaitAction = Action.Stop;
    //     Debug.Log(gameObject.name + " Need_Stop");
    // }

    public void Update()
    {
        if (mDelayCheck > 0)
        {
            mDelayCheck -= Time.deltaTime;
            return;
        }
        if (mNavMeshAgent == null)
        {
            mNavMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (mNavMeshAgent == null)
        {
            return;
        }
        if (mWaitAction != Action.Non)
        {
            CurrentAction = mWaitAction;
            mWaitAction = Action.Non;
        }
        else if (mOnFinishPath != null)
        {
            if (!mNavMeshAgent.isStopped
            && !mNavMeshAgent.pathPending
            && mNavMeshAgent.remainingDistance <= mNavMeshAgent.stoppingDistance
            && (!mNavMeshAgent.hasPath || mNavMeshAgent.velocity.sqrMagnitude == 0f))
            {
                Vector2 d = new Vector2(transform.position.x - mDestination.x, transform.position.z - mDestination.z);
                if (d.sqrMagnitude < MIN_D)
                {
                    // Debug.Log(gameObject.name + " OnFinishPath");
                    var callback = mOnFinishPath;
                    mOnFinishPath = null;
                    callback(mNavMeshAgent);
                    CurrentAction = mCurrentAction;
                }
                else
                {
                    if (mCountRepath <= 1)
                    {
                        SetDestination(mDestination);
                        mCountRepath++;
                    }
                    else
                    {
                        // Debug.Log(gameObject.name + " OnFinishPath");
                        var callback = mOnFinishPath;
                        mOnFinishPath = null;
                        callback(mNavMeshAgent);
                        CurrentAction = mCurrentAction;
                    }
                }
            }
        }
    }

    private bool isHavePath()
    {
        if (mNavMeshAgent != null 
        && mNavMeshAgent.enabled
        && mNavMeshAgent.hasPath)
        {
            return true;
        }
        return false;
    }

    private Action CurrentAction
    {
        set
        {
            mCurrentAction = value;
            if (mCurrentAction == Action.Resume)
            {
                if (mNavMeshAgent.isStopped)
                {
                    mNavMeshAgent.isStopped = false;
                    Debug.Log(gameObject.name + " Resume");
                }
            }
            else if (mCurrentAction == Action.Pause)
            {
                if (!mNavMeshAgent.isStopped)
                {
                    mNavMeshAgent.isStopped = true;
                    Debug.Log(gameObject.name + " Pause");
                }
            }
            else if (mCurrentAction == Action.Stop)
            {
                if (!mNavMeshAgent.isStopped)
                {
                    ResetPath();
                    mNavMeshAgent.isStopped = true;
                    Debug.Log(gameObject.name + " Stop");
                }
            }
        }
    }
}
