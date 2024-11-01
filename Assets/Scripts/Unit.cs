using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour {


    private const int ACTION_POINTS_MAX = 9;



    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDeath;


    [SerializeField] private bool isEnemy;


    private GridPosition gridPosition;
    private HealthSystem healthSystem;
    private BaseAction[] baseActionArray;
    private int actionPoints = ACTION_POINTS_MAX;


    private void Awake() {
        healthSystem = GetComponent<HealthSystem>();
        baseActionArray = GetComponents<BaseAction>();
    }

    private void Start() {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        healthSystem.OnDeath += HealthSystem_OnDeath;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition) {
            // Unit changed Grid Position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;

            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }

    public T GetAction<T>() where T : BaseAction {
        foreach (BaseAction baseAction in baseActionArray) {
            if (baseAction is T) {
                return baseAction as T;
            }
        }
        return null;
    }

    public GridPosition GetGridPosition() {
        return gridPosition;
    }

    public Vector3 GetWorldPosition() {
        return transform.position;
    }

    public BaseAction[] GetBaseActionArray() {
        return baseActionArray;
    }

    public bool TrySpendActionPointToTakeAction(BaseAction baseAction) {
        if (CanSpendActionPointsToTakeAction(baseAction)) {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        } else {
            return false;
        }
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction) {
        return actionPoints >= baseAction.GetActionPointsCost();
    }

    private void SpendActionPoints(int amount) {
        actionPoints -= amount;

        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints() {
        return actionPoints;
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e) {
        if (IsEnemy() && !TurnSystem.Instance.IsPlayerTurn() ||
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn())) {
            actionPoints = ACTION_POINTS_MAX;

            OnAnyActionPointsChanged.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsEnemy() {
        return isEnemy;
    }

    public void Damage(int damageAmount) {
        healthSystem.Damage(damageAmount);
    }

    private void HealthSystem_OnDeath(object sender, EventArgs e) {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        
        Destroy(gameObject);

        OnAnyUnitDeath?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized() => healthSystem.GetHealthNormalized();

}
