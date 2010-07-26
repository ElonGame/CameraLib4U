using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (ChaseCamera))]
public class ChaseCameraGUI : Editor {
	
	private bool springFoldout = false;
	private bool lookAtDamping = false;
	private bool lookHorizontal = false;
	private bool lookVertical = false;
	private bool debug = false;

	public override void OnInspectorGUI ()
	{
		ChaseCamera chaseCamera;

		chaseCamera = target as ChaseCamera;

		if (chaseCamera == null)
		{
			return;
		}
		EditorGUILayout.BeginHorizontal ();
    	EditorGUILayout.PrefixLabel ("Target");
    	chaseCamera.target = (Transform)EditorGUILayout.ObjectField(chaseCamera.target, typeof(Transform));
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	chaseCamera.targetHeight = EditorGUILayout.FloatField("Target height",chaseCamera.targetHeight);
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	chaseCamera.targetMinimumRenderingDistance = EditorGUILayout.FloatField("Minimum render distance", chaseCamera.targetMinimumRenderingDistance);
    	EditorGUILayout.EndHorizontal ();
		
		
		EditorGUILayout.BeginHorizontal ();
		chaseCamera.cameraType = (ChaseCamera.ChaseCameraType)EditorGUILayout.EnumPopup("Camera type", chaseCamera.cameraType);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	EditorGUILayout.PrefixLabel ("Distance");
    	chaseCamera.distance = EditorGUILayout.FloatField(chaseCamera.distance);
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	EditorGUILayout.PrefixLabel ("Camera height");
    	chaseCamera.cameraHeight = EditorGUILayout.FloatField(chaseCamera.cameraHeight);
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		string cameraPitch = ""+chaseCamera.GetCameraPitch()+'\u00B0';
		EditorGUILayout.LabelField("Camera pitch", cameraPitch);
    	EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Separator();
		EditorGUILayout.BeginHorizontal ();
        chaseCamera.virtualCameraCollisionTest = 
					EditorGUILayout.Toggle("Collision Test",chaseCamera.virtualCameraCollisionTest);
    		EditorGUILayout.EndHorizontal ();
		if (chaseCamera.virtualCameraCollisionTest){
			EditorGUILayout.BeginHorizontal ();
	    	EditorGUILayout.PrefixLabel ("CollisionRadius");
	    	chaseCamera.virtualCameraCollisionRadius = EditorGUILayout.FloatField(chaseCamera.virtualCameraCollisionRadius);
	    	EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			chaseCamera.virtualCameraCollisionLayerMask.value = EditorGUILayout.LayerField("Collision layer", chaseCamera.virtualCameraCollisionLayerMask.value);
			EditorGUILayout.EndHorizontal ();
		}
		
		springFoldout =EditorGUILayout.Foldout(springFoldout, "Spring damping");
		if (springFoldout){
			
			EditorGUILayout.BeginHorizontal ();
        		chaseCamera.springSmoothingEnabled = 
					EditorGUILayout.Toggle("Enabled",chaseCamera.springSmoothingEnabled);
    		EditorGUILayout.EndHorizontal ();
		
			
			if (chaseCamera.springSmoothingEnabled){
				EditorGUISpringDampingStat(ref chaseCamera.springStiffness, 
			                                                  ref chaseCamera.springDamping);
			}
		}
		if (chaseCamera.cameraType==ChaseCamera.ChaseCameraType.StayBehind){
			lookVertical = EditorGUILayout.Foldout(lookVertical, "Look horizontal damping");
			if (lookVertical){
				EditorGUILayout.BeginHorizontal ();
	        	chaseCamera.lookHorizontalSpringDamped = 
					EditorGUILayout.Toggle("Enabled",chaseCamera.lookHorizontalSpringDamped);
	        	EditorGUILayout.EndHorizontal ();
					
				if (chaseCamera.springSmoothingEnabled){
					EditorGUISpringDampingStat(ref chaseCamera.lookHorizontalSpringStiffness, 
					                                                  ref chaseCamera.lookHorizontalSpringDamping);
				}
			}
			
			lookHorizontal = EditorGUILayout.Foldout(lookHorizontal, "Look vertical damping");
			if (lookHorizontal){
				EditorGUILayout.BeginHorizontal ();
	        	chaseCamera.lookVerticalSpringDamped = 
					EditorGUILayout.Toggle("Enabled",chaseCamera.lookVerticalSpringDamped);
	        	EditorGUILayout.EndHorizontal ();
					
				if (chaseCamera.lookVerticalSpringDamped){
					EditorGUISpringDampingStat(ref chaseCamera.lookHorizontalSpringStiffness, 
					                                                  ref chaseCamera.lookHorizontalSpringDamping);
				}
			}
		}
		
		lookAtDamping = EditorGUILayout.Foldout(lookAtDamping, "LookAt damping");
		if (lookAtDamping){
			ChaseCameraGUI.EditorGUISmoothLookAt(chaseCamera);
		}
		debug =EditorGUILayout.Foldout(debug, "Debug");
		if (debug){
			this.DrawDefaultInspector();
		}	
	}
	
	public static void EditorGUISpringDampingStat(ref float springStiffness, ref float springDamping){
		
		EditorGUILayout.BeginHorizontal ();
    	springStiffness = Mathf.Abs(EditorGUILayout.FloatField("Stiffness",springStiffness));
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	springDamping = Mathf.Abs(EditorGUILayout.FloatField("Damping",springDamping));
    	EditorGUILayout.EndHorizontal ();
		
		float dampingRatio =Damping.GetSpringDampingRatio(springStiffness, springDamping);
		EditorGUILayout.BeginHorizontal ();
    	EditorGUILayout.LabelField("Damping ratio", ""+dampingRatio);
    	EditorGUILayout.EndHorizontal ();	
		EditorGUILayout.BeginHorizontal ();
		string dampingType = "";
		if (dampingRatio<0.99f){
			 dampingType ="Underdamped";
		} else if (dampingRatio>1.00f){
			dampingType ="Overdamped";
		} else {
			dampingType ="Critically damped";
		}
    	EditorGUILayout.LabelField("Damping type", ""+dampingType);
    	EditorGUILayout.EndHorizontal ();	
		EditorGUILayout.BeginHorizontal ();
	    bool res = GUILayout.Button("Compute critically damped from stiffness");
	    if (res){
			springDamping = 2*Mathf.Sqrt(springStiffness);	
		}
		EditorGUILayout.EndHorizontal ();
	}
	
	public static void EditorGUISmoothLookAt(AbstractCamera camera){
		EditorGUILayout.BeginHorizontal ();
    	camera.smoothLookAtEnabled = 
			EditorGUILayout.Toggle("Enabled",camera.smoothLookAtEnabled);
    	EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
    	camera.smoothLookAtDamping = 
			EditorGUILayout.FloatField("Damping",camera.smoothLookAtDamping);
    	EditorGUILayout.EndHorizontal ();
	}
}
	