using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
/// <summary>
/// ARPG camera controller.
/// Created By: Juandre Swart 
/// Email: info@3dforge.co.za
/// 
/// A Script for a ARPG (Diablo, Path of Exile, Torchlight) style camera that also allows rotation on the y and x axis.
/// It contains code that makes objects transparent if they are between the camera and target.
/// The objects material needs to be a transparent shader so we can change the alpha value.
/// </summary>
public class ARPGCameraController : MonoBehaviour {

	public Transform target;
	public Vector3 targetPos;
	public float distance = 30f; // Distance the camera starts from target object.
	public float maxDistance = 30f; // Max distance the camera can be from target object.
	public float minDistance = 20f; // Min distance the camera can be from target object.
	public float zoomSpeed = 50f; // The speed the camera zooms in.
	public float targetHeight = 0.0f; // The amount from the target object pivot the camera should look at.
	public float camRotationSpeed = 100;// The speed at which the camera rotates.
	public float camXAngle = 45.0f; // The camera x euler angle.
	public float camXAngleMin = 20.0f;
	public float camXAngleMax = 67.0f;
	// public bool fadeObjects = false; // Enable objects of a certain layer to be faded.
	// public List<int> layersToTransparent = new List<int>();	// The layers where we will allow transparency.
	// public float alpha = 0.3f; // The alpha value of the material when player behind object.

	public float fileldOfView = 15.0f;
	public float minFileldOfView = 7.0f;
	public float maxFileldOfView = 15.0f;
	
	public float yCam = 0.0f; // The camera y euler angle.
	private Transform myTransform;
	// private Transform prevHit;
	private float minCameraAngle = 10.0f; // The min angle on the camera's x axis.
	private float maxCameraAngle = 90.0f; // The max angle on the camera's x axis.
	
	private Camera mCamera;
	
	public bool canControl = true;

	void Start () {
		mCamera = GetComponent<Camera>();

		myTransform = transform;
		// myTransform.position = target.position;	
		targetPos = target.position;
		// Vector3 angles = myTransform.eulerAngles ; 
		// Set default y angle.
	    // y = angles.y;
		
		if(target == null)
		{			
			// Debug.LogWarning("No taget added, please add target Game object ");
		}
		if (Application.platform == RuntimePlatform.WindowsPlayer){
			zoomSpeed = 200;
			camRotationSpeed = 700;
		}
	}
	
	void LateUpdate  () {
		
		if(target == null)
		{			
			return;
		}
		targetPos = target.position;
		// Zoom Camera and keep the distance between [minDistance, maxDistance].
		// if(!ShopGame.Instance.IsShow && !PropertysGame.Instance.IsShow)
		{
			float mw = Input.GetAxis("Mouse ScrollWheel");
			if(mw>0){			
				distance-=Time.deltaTime*zoomSpeed;
				if(distance<minDistance)
					distance=minDistance;
			}else if(mw<0){
				distance+=Time.deltaTime*zoomSpeed;
				if(canControl == false){
					if(distance>maxDistance)
						distance=maxDistance;
				}else
				{
					if(distance>70)
						distance=70;
				}
			}
		}
		if(canControl == false){
			if(distance >= maxDistance){
				camXAngle = camXAngleMax;
			}else
			{
				float mu = ( distance - minDistance);
				mu = mu / ( maxDistance - minDistance);
				camXAngle = camXAngleMin + (camXAngleMax - camXAngleMin) * mu;
			}
		}

		if(canControl){
			// Rotate Camera around character.
			if(Input.GetButton("Fire3")){ // 0 is left, 1 is right, 2 is middle mouse button.
				float h = Input.GetAxis("Mouse X"); // The horizontal movement of the mouse.						
				float v = Input.GetAxis("Mouse Y"); // The vertical movement of the mouse.
				if(h>0 && h > Math.Abs(v)){
					myTransform.RotateAround(targetPos,new Vector3(0,1,0),camRotationSpeed*Time.deltaTime);	
					yCam = myTransform.eulerAngles.y;
				}
				else if(h<0 && h<-Math.Abs(v))
				{
					myTransform.RotateAround(targetPos,new Vector3(0,1,0),-camRotationSpeed*Time.deltaTime);
					yCam = myTransform.eulerAngles.y;
				}
				else if(v > 0 && v > Math.Abs(h))
				{
					camXAngle += camRotationSpeed * Time.deltaTime;
					if(camXAngle>maxCameraAngle)
					{
						camXAngle = maxCameraAngle;
					}
				}
				else if(v < 0 && v < -Math.Abs(h))
				{
					camXAngle += -camRotationSpeed * Time.deltaTime;
					if(camXAngle<minCameraAngle)
					{
						camXAngle = minCameraAngle;
					}
				}
			}
		}
		
		// Set camera angles.
		Quaternion rotation = Quaternion.Euler (camXAngle, yCam, 0); 	
	   	myTransform.rotation  = rotation ;
		
		// Position Camera.
		Vector3 trm = rotation * Vector3.forward * distance + new Vector3(0, -1 * targetHeight, 0);
		Vector3 position = targetPos  - trm;
		myTransform.position = position;

		//
		if(maxDistance != minDistance){
			float mu = (distance - minDistance) / ( maxDistance - minDistance);
			fileldOfView = minFileldOfView + (maxFileldOfView - minFileldOfView) * mu;
			mCamera.fieldOfView = fileldOfView;
		}
		/*
		//Start checking if object between camera and target.
		if(fadeObjects)
		{
			// Cast ray from camera.position to target.position and check if the specified layers are between them.
			Ray ray = new Ray(myTransform.position, (targetPos - myTransform.position).normalized);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, maxDistance)) {
			    Transform objectHit = hit.transform;
				if(layersToTransparent.Contains(objectHit.gameObject.layer))
				{
					if(prevHit!=null)
					{
						prevHit.GetComponent<Renderer>().material.color = new Color(1,1,1,1);
					}
					if(objectHit.GetComponent<Renderer>() != null)
					{
						prevHit = objectHit;
						// Can only apply alpha if this material shader is transparent.
						prevHit.GetComponent<Renderer>().material.color = new Color(1,1,1,alpha);
					}
				}		    
				else if(prevHit != null)
				{
					prevHit.GetComponent<Renderer>().material.color = new Color(1,1,1,1);
					prevHit = null;
				}
			}
		}
		*/
	}

	public void MoveCamToLock(Transform listener, Transform talker, GTweenCallback _onComplete){
		float _camXAngle_to = (camXAngleMin + camXAngleMax) / 2;
		float _y_to = 75;
		// float _y_to = 0;
		float _distance_to = 20;
		// Vector3 _target_to = (listener.position + talker.position) * 0.5f;
		Vector3 _target_to = talker.position;
		MoveCamTo(_camXAngle_to, _y_to, _distance_to, _target_to, _onComplete);
	}

	public void MoveCamTo(float _camXAngle_to, float _y_to, float _distance_to, Vector3 _target_to, GTweenCallback _onComplete){
		float _camXAngle_from = camXAngle;
		float _camXAngle_a = _camXAngle_to - _camXAngle_from;
		float _y_from = yCam;
		float _y_a = _y_to - _y_from;
		float _distance_from = distance;
		float _distance_a = _distance_to - _distance_from;
		float _targetHeight = targetHeight;
		Vector3 _target_from = targetPos;
		target = null;
		GTween.Kill(this);
		// Debug.Log("_target_from: " + _target_from.ToString());
		// Debug.Log("_target_to: " + _target_to.ToString());
		GTween.To(0.0f, 1.0f, 1.0f).SetTarget(this).SetEase(EaseType.Linear).OnUpdate(
			(GTweener tweener) => {
				float mu = tweener.value.x;
				if(mu >= 1){
					mu = 1;
				}
				float _camXAngle = _camXAngle_from + _camXAngle_a * mu;
				float _y = _y_from + _y_a * mu;
				float _distance = _distance_from + _distance_a * mu;
				Vector3 _target = Vector3.Lerp(_target_from, _target_to, mu);
				ForceSetCam(_camXAngle, _y, _distance, _target, _targetHeight);
			}
		).OnComplete(()=>{
			ForceSetCam(_camXAngle_to, _y_to, _distance_to, _target_to, _targetHeight);
			if(_onComplete != null)
				_onComplete();
		});
	}

	private void ForceSetCam(float _camXAngle, float _y, float _distance, Vector3 _target, float _targetHeight){
		// Set camera angles.
		Quaternion rotation = Quaternion.Euler (_camXAngle, _y, 0);
	   	myTransform.rotation  = rotation ;
		
		// Position Camera.
		Vector3 trm = rotation * Vector3.forward * _distance + new Vector3(0, -1 * _targetHeight, 0);
		Vector3 position = _target  - trm;
		myTransform.position = position ;

		// Update data
		camXAngle = _camXAngle; 
		yCam = _y;
		distance = _distance;
		targetPos = _target;		
	}
}
