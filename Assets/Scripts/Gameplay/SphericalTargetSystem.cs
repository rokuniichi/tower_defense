﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SphericalTargetSystem : MonoBehaviour, ITargetSystem
{
	[SerializeField] private float radius;
	[SerializeField] private LayerMask layerMask;
	[SerializeField] private OnTargetSetEvent onTargetSet;

	private SphereCollider _sphereCollider;
	private Transform _currentTarget;
	private HashSet<Transform> _allTargets;

	public void Init(float newRadius, LayerMask newLayerMask)
    {
		radius = newRadius;
		layerMask = newLayerMask;
		_sphereCollider = GetComponent<SphereCollider>();
		_sphereCollider.radius = newRadius;
		_allTargets = new HashSet<Transform>();
		Collider[] collidersInRadius = Physics.OverlapSphere(_sphereCollider.transform.position, _sphereCollider.radius);
		foreach (Collider collider in collidersInRadius)
			if (CheckTargetLayer(collider.gameObject.layer))
				_allTargets.Add(collider.transform);

		UpdateTarget();
	}

	private void UpdateTarget()
	{
		if (_currentTarget != null) return;
		float currentDistance = float.MaxValue;
		Transform result = null;
		foreach (Transform target in _allTargets)
		{
			float distance = Vector3.Distance(transform.position + _sphereCollider.center, target.position);
			if (distance <= currentDistance)
			{
				result = target;
				currentDistance = distance;
			}
		}

		SetTarget(result);
	}

	private void SetTarget(Transform target)
	{
		_currentTarget = target;
		onTargetSet?.Invoke(target);
	}

	private void AddTarget(Transform target)
	{
		_allTargets.Add(target);
		if (_currentTarget == null)
			UpdateTarget();

		target.GetComponent<IKillable>().OnKill += RemoveTarget;
	}

	private void RemoveTarget(Transform target)
	{
		_allTargets.Remove(target);
		if (_currentTarget == target)
		{
			_currentTarget = null;
			UpdateTarget();
		}

		target.GetComponent<IKillable>().OnKill -= RemoveTarget;
	}

	private bool CheckTargetLayer(int layer)
	{
		return layerMask.value == (layerMask.value | (1 << layer));
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!CheckTargetLayer(other.gameObject.layer)) return;
		AddTarget(other.transform);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!CheckTargetLayer(other.gameObject.layer)) return;
		RemoveTarget(other.transform);
	}
}
