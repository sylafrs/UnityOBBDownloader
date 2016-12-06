using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SmoothScale : MonoBehaviour 
{
	public  float target;
	private float current;

	public float SpeedHigher;
	public float SpeedLower;

	public bool UsesDuration;
	public float DurationHigher;
	public float DurationLower;
	public float DurationRange;

	public bool ResetOnEnable = true;
	public float enableValue = 1;

	public bool x = true, y = true, z = true;
	private bool reached;
	public bool canInvoke;
	public UnityEvent OnTargetReached;

	void Awake () 
	{
		current = target;
	}

	void OnEnable()
	{
		if (ResetOnEnable)
		{
			SetTarget(enableValue);
			ForceScale(enableValue);
		}
	}

	public void ForceScale(float scale)
	{
		current = scale;

		Vector3 v = new Vector3();
		v.x = x ? this.current : 1;
		v.y = y ? this.current : 1;
		v.z = z ? this.current : 1;

		this.transform.localScale = v;
	}

	public void SetTarget(float scale)
	{
		target = scale;
	}

	public void SetInvokable(bool canInvoke)
	{
		this.canInvoke = canInvoke;
	}

	void Update()
	{
		if (UsesDuration)
		{
			SpeedHigher = DurationRange / DurationHigher;
			SpeedLower = DurationRange / DurationLower;
		}

		if (this.target > this.current)
		{
			this.current += SpeedHigher * Time.deltaTime;
			if (this.current > this.target)
			{
				this.current = this.target;
			}
		}

		else if (this.target < this.current)
		{
			this.current -= SpeedLower * Time.deltaTime;
			if (this.current < this.target)
			{
				this.current = this.target;
			}
		}

		if (this.current == this.target)
		{
			if (!reached && canInvoke)
				OnTargetReached.Invoke();
			reached = true;
		}
		else
		{
			reached = false;
		}

		Vector3 scale = new Vector3();
		scale.x = x ? this.current : 1;
		scale.y = y ? this.current : 1;
		scale.z = z ? this.current : 1;

		this.transform.localScale = scale;
	}
}
