using UnityEngine;
using System.Collections;

/// <summary>
/// The camera will track the object, but it's movement is bound to the spline curve.
///  
/// Assumptions:
/// I assume that the camera can see the player from the spline given the maxDistance-parameter. This means that I 
/// do not do any visibility check.
/// I also assumes that the spline does not collide with the environment (or other parts of the game world).
/// Note that neither of these assumption may hold in a game with moving objects or dynamic environment.
/// </summary>
[AddComponentMenu("CameraLib/Path Bound Camera")]
[RequireComponent (typeof (Camera))]
public class PathBoundCamera : ICamera {
	public SplineComponent cameraSpline;
	// public SplineComponent targetSpline;
	private SplineCurve cameraSplineObject;
	
	/// <summary>
	/// If the preferred distance between camera and is exceeded, then the camera needs to move
	/// </summary>
	public float maxDistanceToTarget = 2;
	/// <summary>
	/// if the distance between camera and target is 'too large' the camera will search the full path for the 
	/// closest control-point and position itself there
	/// </summary>
	public float maxDistanceToJumpCut =4;
	private float currentPositionOnPath = 0;
	 
	public float minTimeBetweenDistancebasedJumpcut =3;
	private float distanceBasedJumpcutTimer = 0;
	
	
	public bool dampingEnabled = true;
	public float dampingConstant = 1f;
	private Vector3 desiredPosition;
	
	// Use this for initialization
	void Start () {
		this.cameraSplineObject = cameraSpline.GetSplineObject();
		InitCamera();	
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary> 
	void Update () {
		distanceBasedJumpcutTimer += Time.deltaTime;
		
		desiredPosition = GetCameraTargetPosition();
		if (dampingEnabled){
			transform.position = Vector3.Lerp(transform.position,desiredPosition,dampingConstant*Time.deltaTime);
		} else {
			transform.position = desiredPosition;
		}
	}
	
	/// <summary>
	/// Set the camera on on the controlpoint closest to the player
	/// </summary>
	public override void InitCamera(){
		JumpCutToClosestControlPoint();
	}
	
	/// <summary>
	/// Search for the control point closest to the player and jump cut to that position 
	/// </summary>
	private void JumpCutToClosestControlPoint(){
		Vector3[] controlpoints =cameraSplineObject.controlPoints;
		float minDistance = float.MaxValue;
		
		int closestControlPoint= 0;
		for (int i=0;i<controlpoints.Length;i++){
			float controlpointLength =(controlpoints[i]-target.position).sqrMagnitude;
			if (minDistance > controlpointLength){
				minDistance =controlpointLength;
				closestControlPoint = i;
			}
		}
		transform.position = controlpoints[closestControlPoint];
		desiredPosition = transform.position;
		currentPositionOnPath =cameraSplineObject.time[closestControlPoint];
		
		distanceBasedJumpcutTimer = 0;
	}
	
	public void SetPreferredDistanceToCamera(float d){
		this.maxDistanceToTarget = d;
	}
	
	public override Vector3 GetCameraTargetPosition(){
		Vector3 splineVelocity = cameraSplineObject.GetVelocity(currentPositionOnPath);
		Vector3 distance = desiredPosition-target.position;
		
		if (distance.sqrMagnitude <= maxDistanceToTarget*maxDistanceToTarget){
			// don't move camera, since target is closer than preferredDistanceToCamera
			return desiredPosition;
		} 	
		
		if (distance.sqrMagnitude > maxDistanceToJumpCut*maxDistanceToJumpCut && 
		    distanceBasedJumpcutTimer > minTimeBetweenDistancebasedJumpcut){
			// jump cut
			JumpCutToClosestControlPoint();
			return desiredPosition;
		}
		
		
		// determine search direction
		distance = distance-(distance.normalized*maxDistanceToTarget);
		
		float estimatedDelta = distance.magnitude;
		float dotProduct = Vector3.Dot(distance,splineVelocity);
		
		if (dotProduct>0){
			estimatedDelta =-estimatedDelta;
		}
		
		float desiredPositionOnSpline = currentPositionOnPath+estimatedDelta;
		
		// make sure if the desired position will not pull the camera backwards
		Vector3 newSplineVelocity = cameraSplineObject.GetVelocity(desiredPositionOnSpline);
		Vector3 newDistance = cameraSplineObject.GetPosition(desiredPositionOnSpline)-target.position;
		float newDotProduct = Vector3.Dot(newSplineVelocity, newDistance);
		if (newDotProduct>0==dotProduct>0){
			currentPositionOnPath = Mathf.Clamp(desiredPositionOnSpline,0,cameraSplineObject.totalLength);;
		}
		return cameraSplineObject.GetPosition(currentPositionOnPath);
	}
	
	public override void SetTarget(Transform target){
		this.target = target;
	}
	
	public override Transform GetTarget(){
		return transform;
	}
	
	public void OnDrawGizmosSelected(){
		Gizmos.color = Color.white;
    	Gizmos.DrawWireSphere (desiredPosition, 0.2f);
	}
}
