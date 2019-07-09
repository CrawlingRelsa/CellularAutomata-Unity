using System.Collections.Generic;
using UnityEngine;
using System;

public class Segment
{
	public float min;
	public float max;
	public Segment(float max, float min)
	{
		this.min = min;
		this.max = max;
	}

	public float Length()
	{
		return (Mathf.Abs(max) + Mathf.Abs(min));
	}

}