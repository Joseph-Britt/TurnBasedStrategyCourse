using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour {


    public static event EventHandler OnAnyGrenadeExploded;


    [SerializeField] private Transform grenadeExplodeVFXPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;


    private Vector3 targetPosition;
    private Action onGrenadeBehaviorComplete;
    private float totalDistance;
    private Vector3 positionXZ;


    private void Update() {
        Vector3 moveDir = (targetPosition - positionXZ).normalized;

        float moveSpeed = 15f;
        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);
        
        float reachedTargetDistance = .2f;
        if (Vector3.Distance(positionXZ, targetPosition) < reachedTargetDistance) {
            float damageRadius = 4f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach (Collider collider in colliderArray) {
                if (collider.TryGetComponent(out Unit targetUnit)) {
                    targetUnit.Damage(30);
                }
                if (collider.TryGetComponent(out DestructibleCrate destructibleCrate)) {
                    destructibleCrate.Damage();
                }
            }

            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

            trailRenderer.transform.parent = null;

            Instantiate(grenadeExplodeVFXPrefab, targetPosition + Vector3.up * 1f, Quaternion.identity);
            
            Destroy(gameObject);

            onGrenadeBehaviorComplete();
        }
    }

    public void Setup(GridPosition targetGridPosition, Action onGrenadeBehaviorComplete) {
        this.onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }

}
