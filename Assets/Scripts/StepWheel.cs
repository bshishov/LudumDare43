using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StepWheel : MonoBehaviour
{
    public float StepSize;
    public Sound StepSound;

    private NavMeshAgent _agent;
    private float _t;

	void Start ()
	{
	    _agent = GetComponent<NavMeshAgent>();
	}
	
	
	void Update ()
	{
	    var velocity = _agent.velocity.magnitude;
	    if (velocity < 0.01f)
	    {
	        _t = 0f;
	    }
	    else
	    {
	        _t += Time.deltaTime * _agent.velocity.magnitude;
        }
	    
	    if (_t > StepSize)
	    {
            OnStep();
	        _t -= StepSize;
	    }
	}

    void OnStep()
    {
        var s = SoundManager.Instance.Play(StepSound);
        if (s != null)
        {
            s.AttachToObject(transform);
        }
    }
}
