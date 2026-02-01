//using Assets.Scripts.Character;
using System.Collections;
using UnityEngine;



public class Unit : MonoBehaviour
{
    public int luchadorID;
    public enum Team
    {
        Jugador1,
        Jugador2,
        NPC
    }

    [Header("Animation")]
    [SerializeField] private Animator animator;
    static readonly int CaminarTrigger = Animator.StringToHash("caminar");
    static readonly int CaidaBombaTrigger = Animator.StringToHash("CaidaBomba");

    [Header("Movement")]
    public float moveSpeed = 5f;
    public int movementRange = 5;

    [Header("Push")]
    public int pushBonus = 0;

    [Header("Team")]
    public Team team = Team.Jugador1;

    // Grid position
    public Vector2Int GridPosition { get; private set; }

    // Planning
    public bool HasPlannedMovement { get; private set; }
    public Vector2Int PlannedDestination { get; private set; }

    private bool isMoving;
    private bool wasPushedThisTurn;
    private bool stunnedThisTurn;

    private void Start()
    {
        GridPosition = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(GridPosition);
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // ================= PLANNING =================

    public void SetPlannedDestination(Vector2Int destination)
    {
        PlannedDestination = destination;
        HasPlannedMovement = true;
    }

    public void ClearPlannedMovement()
    {
        HasPlannedMovement = false;
    }

    // ================= MOVEMENT =================

    public bool IsMoving()
    {
        return isMoving;
    }

    // Called by MovementExecution
    public void MoveOneStep(Vector2Int targetGridPos, System.Action onFinished)
    {
        if (isMoving)
            return;

        // If stunned, do not continue original movement
        if (stunnedThisTurn)
        {
            onFinished?.Invoke();
            return;
        }

        Unit other = GetUnitAt(targetGridPos);

        if (other != null && other != this)
        {
            ResolveCollision(other, targetGridPos, onFinished);
            return;
        }

        Vector3 targetWorldPos =
            GridManager.Instance.GridToWorld(targetGridPos);

        StartCoroutine(
            MoveCoroutine(targetGridPos, targetWorldPos, onFinished)
        );
    }

    // Forced movement (push)
    private void ForceMove(Vector2Int targetGridPos, System.Action onFinished)
    {
        if (isMoving)
            return;

        Vector3 targetWorldPos =
            GridManager.Instance.GridToWorld(targetGridPos);

        StartCoroutine(
            MoveCoroutine(targetGridPos, targetWorldPos, onFinished)
        );
    }

    // ================= COLLISION =================

    private void ResolveCollision(
        Unit other,
        Vector2Int collisionPos,
        System.Action onFinished)
    {
        // If this unit was pushed before, it always loses
        if (wasPushedThisTurn)
        {
            ResolvePush(this, other, collisionPos, onFinished);
            return;
        }

        // If the other unit was pushed before, it always loses
        if (other.wasPushedThisTurn)
        {
            ResolvePush(other, this, collisionPos, onFinished);
            return;
        }

        // ---- PROBABILIDAD CON BONUS ----
        int chanceThisWins = 50 + (this.pushBonus - other.pushBonus);
        chanceThisWins = Mathf.Clamp(chanceThisWins, 0, 100);

        int roll = Random.Range(0, 100);
        bool thisWins = roll < chanceThisWins;

        Unit winner = thisWins ? this : other;
        Unit loser = thisWins ? other : this;

        ResolvePush(loser, winner, collisionPos, onFinished);
    }
    void PlayCaidaBomba()
    {
        if (animator == null) return;

        
        animator.ResetTrigger("Caminar");

        // Lanzar el ataque
        animator.SetTrigger("CaidaBomba");
    }
    IEnumerator CaidaBombaRoutine()
    {
        // cortar caminar sí o sí
        animator.ResetTrigger("Caminar");
        animator.ResetTrigger("Idle");

        // lanzar ataque
        animator.SetTrigger(CaidaBombaTrigger);

        // esperar 1 frame para que entre
        yield return null;

        // Esperar a que termine el estado de ataque
        // PON AQUÍ el nombre EXACTO del ESTADO en el Animator
        const string attackStateName = "CaidaBomba";

        // espera a estar en el estado
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(attackStateName))
            yield return null;

        // espera a que termine
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        // volver a idle
        animator.ResetTrigger(CaidaBombaTrigger);
        animator.SetTrigger("Idle");
    }

    private void ResolvePush(
        Unit loser,
        Unit winner,
        Vector2Int collisionPos,
        System.Action onFinished)
    {
        winner.PlayCaidaBomba();
        Vector2Int pushDir =
            loser.GridPosition - winner.GridPosition;

        pushDir = new Vector2Int(
            Mathf.Clamp(pushDir.x, -1, 1),
            Mathf.Clamp(pushDir.y, -1, 1)
        );

        Vector2Int pushTarget = loser.GridPosition + pushDir;

        // Ring out -> eliminado
        if (!GridManager.Instance.IsInsideGrid(pushTarget))
        {
            Debug.Log(
                "'" + winner.name + "' ha tirado del ring a '" + loser.name + "'"
            );

            loser.gameObject.SetActive(false);
            onFinished?.Invoke();
            return;
        }

        // Casilla ocupada -> no se puede empujar
        if (GetUnitAt(pushTarget) != null)
        {
            onFinished?.Invoke();
            return;
        }

        // Mark loser as pushed and stunned
        loser.wasPushedThisTurn = true;
        loser.stunnedThisTurn = true;
        loser.ClearPlannedMovement();

        loser.ForceMove(pushTarget, () =>
        {
            if (winner == this)
            {
                Vector3 worldPos =
                    GridManager.Instance.GridToWorld(collisionPos);

                StartCoroutine(
                    MoveCoroutine(collisionPos, worldPos, onFinished)
                );
            }
            else
            {
                onFinished?.Invoke();
            }
        });
    }

    // ================= UTIL =================

    private Unit GetUnitAt(Vector2Int gridPos)
    {
        Unit[] units = FindObjectsOfType<Unit>();

        foreach (Unit u in units)
        {
            if (u != this && u.GridPosition == gridPos && !u.isMoving)
                return u;
        }

        return null;
    }

    public void ResetPushState()
    {
        wasPushedThisTurn = false;
        stunnedThisTurn = false;
    }

    // ================= COROUTINE =================

    private IEnumerator MoveCoroutine(
        Vector2Int targetGridPos,
        Vector3 targetWorldPos,
        System.Action onFinished)
    {
        isMoving = true;
        if (animator != null)
            animator.SetTrigger("Caminar");



        Vector3 start = transform.position;
        Vector3 end = targetWorldPos;

        float distance = Vector3.Distance(start, end);
        float duration = distance / moveSpeed;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        GridPosition = targetGridPos;

        isMoving = false;
        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
        onFinished?.Invoke();
    }
}
