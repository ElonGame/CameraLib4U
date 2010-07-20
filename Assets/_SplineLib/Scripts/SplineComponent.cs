using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplineComponent : MonoBehaviour {
	
	public enum SplineType { LinearSpline,BezierSpline,BezierSmoothSpline, HermiteSpline, KochanekBartel };
	
	public SplineType splineType = SplineType.LinearSpline;
	public SplineCurve spline;
	public bool alwaysDrawGizmo = true;
	public Color splineColor = Color.white;
	public Color controlPointColor = Color.yellow;
	public Color controlPointColorSelected = Color.green;
	
	public float sphereRadius = 0.1f;
	public float lengthPrecision =0.001f;
	
	private bool updated = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	
	public void OnDrawGizmosSelected () {
		if (!alwaysDrawGizmo){
			DrawGizmo();
		}
	}
	
	public SplineCurve GetSplineObject(){
		if (spline==null){
			 DoUpdateSpline();
		}
		return spline;
	}
	
	public void UpdateSpline(){
		updated =false;
	}
	
	public void DoUpdateSpline(){
		List<Vector3> controlPoints = new List<Vector3>();
		List<Vector3> tangents = new List<Vector3>();
		List<float> time = new List<float>();
		float lastTime = -1f;
		for (int i=0;i<transform.childCount;i++){
			ControlPointComponent controlPointComp = transform.GetChild(i).gameObject.GetComponent<ControlPointComponent>();
			if (controlPointComp!=null){
				controlPoints.Add(controlPointComp.position);
				tangents.Add(controlPointComp.tangent);
				// if illegal time detected, then autoadjust
				if (lastTime>=controlPointComp.time){
					controlPointComp.time += lastTime; 
				}
				lastTime = controlPointComp.time;
				time.Add(controlPointComp.time);
			}
		}
		
		switch (splineType){
		case SplineType.LinearSpline:{
			LinearSplineCurve lSpline = new LinearSplineCurve();
			spline = lSpline;
			spline.lengthPrecision =lengthPrecision;
			lSpline.Init(controlPoints.ToArray());
			}
			break;
		case SplineType.HermiteSpline:{
			HermiteSplineCurve hSpline = new HermiteSplineCurve();
			spline = hSpline;
			spline.lengthPrecision =lengthPrecision;
			hSpline.InitNatural(controlPoints.ToArray());
			}
			break;
		case SplineType.KochanekBartel:
		{
			HermiteSplineCurve hSpline = new HermiteSplineCurve();
			spline = hSpline;
			spline.lengthPrecision =lengthPrecision;
			hSpline.InitKochanekBartel(controlPoints.ToArray(),0f,0f,0f);	
		}
		break;
		case SplineType.BezierSpline:
		case SplineType.BezierSmoothSpline:
			BezierSplineCurve bSpline = new BezierSplineCurve();
			spline = bSpline;
			spline.lengthPrecision =lengthPrecision;
			switch (splineType){
			case SplineType.BezierSpline:
				bSpline.Init(controlPoints.ToArray());
				break;
			case SplineType.BezierSmoothSpline:
				bSpline.InitSmoothTangents(controlPoints.ToArray());
				break;
			}
			break;
		}	
		updated = true;
	}
	
	private void DrawGizmo(){
		Gizmos.color = controlPointColor;
		string debug = "";
		for (int i=0;i<transform.childCount;i++){
			ControlPointComponent splineComponent = transform.GetChild(i).gameObject.GetComponent<ControlPointComponent>();
			if (splineComponent!=null){
				Gizmos.DrawSphere(splineComponent.position, sphereRadius);
				debug+=splineComponent.name+" "; 
			}
		}
		Debug.Log(debug);
		
		// todo only call when updates 
		if (!updated || spline ==null){
			DoUpdateSpline();
		}
		
		float[] fs = spline.GetRenderPoints();
		Vector3[] vs = new Vector3[fs.Length];
		string renderpoints = "";
		for (int i=0;i<fs.Length;i++){
			vs[i] = spline.GetPosition(fs[i]);
			renderpoints += fs[i]+" ";
		}
		for (int i=1;i<vs.Length;i++){
			Gizmos.color = splineColor;
			Gizmos.DrawLine(vs[i-1],vs[i]);
		}	
	}
	
	/// <summary>
	/// Returns the number of controlpoints for each segment.
	/// </summary>
	public int GetControlPointsPerSegment(){
		switch (splineType){
			case SplineType.LinearSpline:
			case SplineType.HermiteSpline:
			return 2;
		case SplineType.BezierSpline:
			return 4;
		case SplineType.BezierSmoothSpline:
		default:
			return 3;	
		}
	}
	
	public void OnDrawGizmos(){
		if (alwaysDrawGizmo){
			DrawGizmo();
		}
	}
}
