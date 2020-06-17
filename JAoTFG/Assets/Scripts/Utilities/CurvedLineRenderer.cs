using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof(LineRenderer) )]
public class CurvedLineRenderer : MonoBehaviour 
{
	//PUBLIC
	public float lineSegmentSize = 0.15f;
	public float lineWidth = 0.1f;
	//PRIVATE
	private Vector3[] linePoints = new Vector3[0];
	private Vector3[] linePointsOld = new Vector3[0];

	// locals
	private LineRenderer lineRenderer;

	private void Awake()
	{
		this.lineRenderer = GetComponent<LineRenderer>();
	}

	public void SetPoints(Vector3[] linePoints)
	{
		//get smoothed values
		Vector3[] smoothedPoints = LineSmoother.SmoothLine(linePoints, lineSegmentSize);

		//set line settings
		lineRenderer.positionCount = smoothedPoints.Length;
		lineRenderer.SetPositions(smoothedPoints);
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
	}
}
