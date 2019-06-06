using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClickToMoveAON : MonoBehaviour {

	// public float clickMaxDist = 1000.0f;
	NavMeshAgentCallback m_Agent;
    RaycastHit m_HitInfo = new RaycastHit();
    public Collider coll;
    
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgentCallback>();
    }
    float waitBlock = 0;
    bool skipTouch = false;
    // public Plane plane = new Plane(Vector3.up, 0f);
    // public Plane plane;

    void Update()
    {
        if(TriggerGame.Instance.IsScriptMainRunning){
            return;
        }
        if( m_Agent == null || m_Agent.IsCanMove() == false){
            return;
        }
        if (Input.GetMouseButtonDown(0)){
            if( Input.GetKey(KeyCode.W )){
                //Blink
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (coll.Raycast(ray, out m_HitInfo, 1000)){
                    var r = m_HitInfo.point;
                    m_Agent.NavMeshAgent.Warp(r);
                }
            }
        }
    }

    public void MoveTo( Vector3 mousePosition)
    {
        if( m_Agent == null || m_Agent.IsCanMove() == false){
            return;
        }
        if(GameGui.IsIgnoreMouse(mousePosition)){
            return;
        }
        if( Camera.main == null || !Camera.main.isActiveAndEnabled){
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (coll.Raycast(ray, out m_HitInfo, 1000)){
            var r = m_HitInfo.point;
            m_Agent.NavMeshAgent.SetDestination(r);
        }
    }

    void UpdateLastVer()
    {
        #if true //Using Coll
        if(TriggerGame.Instance.IsScriptMainRunning){
            return;
        }
        if( m_Agent.IsCanMove() == false){
            return;
        }
        if (Input.GetMouseButtonDown(0)){
            if( Input.GetKey(KeyCode.W )){
                //Blink
                skipTouch = true;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (coll.Raycast(ray, out m_HitInfo, 1000)){
                    var r = m_HitInfo.point;
                    m_Agent.NavMeshAgent.Warp(r);
                }
            }else{
                skipTouch = false;
                waitBlock = 0;
            }
        }
        if(skipTouch)
            return;
		if(Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            {
                if(waitBlock > 0){
                    waitBlock -= Time.deltaTime;
                }
                if (coll.Raycast(ray, out m_HitInfo, 1000)){
                    var r = m_HitInfo.point;
                    // var s = transform.position;
                    // s.y = r.y;
                    bool blocked = false;
                    {
                        bool isCanMove  = true;
                        if (blocked){
                            if(waitBlock > 0){
                                isCanMove = false;
                            }else{
                                waitBlock = 0.5f;
                            }
                        }else{
                            waitBlock = 0;
                        }
                        if(isCanMove){
                            m_Agent.NavMeshAgent.SetDestination(r);    
                        }
                    }
                }
            }
		}
        #else //Using Plane
        if (Input.GetMouseButtonDown(0)){
            if( Input.GetKey(KeyCode.W )){
                //Blink
                skipTouch = true;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distanceToPlane;
                if(plane.Raycast( ray, out distanceToPlane)){
                    var r = ray.GetPoint( distanceToPlane);
                    // transform.position = r;
                    m_Agent.Warp(ray.GetPoint( distanceToPlane));
                }
            }else{
                skipTouch = false;
                waitBlock = 0;
            }
        }
        if(skipTouch)
            return;
		if(Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Method 1:
            // if (coll.Raycast(ray, out m_HitInfo, 1000)){
            //     m_Agent.destination = m_HitInfo.point;
            // }

            //Method 2:
            {
                if(waitBlock > 0){
                    waitBlock -= Time.deltaTime;
                }
                float distanceToPlane;
                if(plane.Raycast( ray, out distanceToPlane)){
                    m_Agent.isStopped = false;
                    NavMeshHit navmeshHit;
                    var r = ray.GetPoint( distanceToPlane);
                    bool blocked = NavMesh.Raycast(transform.position, r, out navmeshHit, NavMesh.AllAreas);
                    Debug.Log("transform.position:" + transform.position.ToString());
                    Debug.Log("distanceToPlane:" + distanceToPlane.ToString());
                    Debug.Log("r:" + r.ToString());
                    bool isCanMove  = true;
                    if (blocked){
                        if(waitBlock > 0){
                            isCanMove = false;
                        }else{
                            waitBlock = 0.5f;
                        }
                    }else{
                        waitBlock = 0;
                    }
                    if(isCanMove)
                        // m_Agent.SetDestination(navmeshHit.position);
                        m_Agent.SetDestination(r);

                    /*
                    if(NavMesh.SamplePosition( r, out navmeshHit, 1000.0f, NavMesh.AllAreas)) {
                        m_Agent.SetDestination(navmeshHit.position);
                        // m_Agent.Resume();
                        Debug.Log("navmeshHit.position: " + navmeshHit.position.ToString());
                        isHasWarp = false;
                    }else{
                        isWaitBtbegin = true;
                    }
                    */
                    
                }
            }
		}
        #endif
    }
}
