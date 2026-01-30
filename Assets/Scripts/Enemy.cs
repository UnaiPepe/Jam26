using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Test_Movement playerMover;
    public float cellSize = 1f;

    [Header("Enemy grid")]
    public int gridWidth = 15;
    public int gridHeight = 15;
    public float stepDuration = 0.12f;
    [Range(1, 3)] public int stepsPerPlayerMove = 1;

    [Header("Collision")]
    public float ignoreCollisionAtStartSeconds = 0.15f; // NEW

    Animator anim;
    Vector2Int gridPos;
    bool isMoving;

    bool hasHitPlayer = false;           
    bool collisionEnabled = false;      

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        gridPos = WorldToGrid(transform.position);
        gridPos = ClampToBounds(gridPos);
        transform.position = GridToWorld(gridPos);

        
        anim.ResetTrigger("Hit");
        anim.SetBool("IsMoving", false);

       
        StartCoroutine(EnableCollisionAfterDelay());

        // suscribirse al movimiento del player
        playerMover.OnMoveStarted += HandlePlayerMoveStarted;
        playerMover.OnMoveFinished += HandlePlayerMoveFinished;
        anim.ResetTrigger("Hit");
        anim.SetBool("IsMoving", false);
        anim.Play("Quieto", 0, 0f);  // el nombre EXACTO del estado
        anim.Update(0f);
    }

    IEnumerator EnableCollisionAfterDelay() // NEW
    {
        yield return new WaitForSeconds(ignoreCollisionAtStartSeconds);
        collisionEnabled = true;
    }

    void OnDestroy()
    {
        if (playerMover != null)
        {
            playerMover.OnMoveStarted -= HandlePlayerMoveStarted;
            playerMover.OnMoveFinished -= HandlePlayerMoveFinished;
        }
    }

    void HandlePlayerMoveStarted()
    {
        if (hasHitPlayer) return;     
        if (anim == null) return;

        anim.SetBool("IsMoving", true);

        if (!isMoving)
            StartCoroutine(MoveWhilePlayerMoves());
    }

    void HandlePlayerMoveFinished()
    {
        if (anim == null) return;

        anim.SetBool("IsMoving", false);
    }

    IEnumerator MoveWhilePlayerMoves()
    {
        isMoving = true;

        int steps = Mathf.Clamp(stepsPerPlayerMove, 1, 3);
        Vector2Int dir = RandomCardinalDir();

        for (int i = 0; i < steps; i++)
        {
            if (hasHitPlayer) break; 

            Vector2Int next = gridPos + dir;
            if (!IsInsideBounds(next)) break;

            yield return StepTo(next);
            gridPos = next;
        }

        isMoving = false;
    }

    IEnumerator StepTo(Vector2Int targetGrid)
    {
        Vector3 start = transform.position;
        Vector3 target = GridToWorld(targetGrid);

        float t = 0f;
        while (t < 1f)
        {
            if (hasHitPlayer) yield break; 
            t += Time.deltaTime / stepDuration;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collisionEnabled) return;       
        if (hasHitPlayer) return;            

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHitPlayer = true;            
            Debug.Log("Han chocado");

            
            StopAllCoroutines();
            isMoving = false;

            
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
            anim.SetBool("IsMoving", false);
        }
    }

    // --- Grid helpers ---
    Vector2Int RandomCardinalDir()
    {
        int r = Random.Range(0, 4);
        return r switch
        {
            0 => new Vector2Int(1, 0),
            1 => new Vector2Int(-1, 0),
            2 => new Vector2Int(0, 1),
            _ => new Vector2Int(0, -1),
        };
    }

    bool IsInsideBounds(Vector2Int gp)
        => gp.x >= 0 && gp.x < gridWidth && gp.y >= 0 && gp.y < gridHeight;

    Vector2Int ClampToBounds(Vector2Int gp)
    {
        gp.x = Mathf.Clamp(gp.x, 0, gridWidth - 1);
        gp.y = Mathf.Clamp(gp.y, 0, gridHeight - 1);
        return gp;
    }

    Vector3 GridToWorld(Vector2Int gp)
        => new Vector3(gp.x * cellSize, transform.position.y, gp.y * cellSize);

    Vector2Int WorldToGrid(Vector3 world)
    {
        int x = Mathf.RoundToInt(world.x / cellSize);
        int z = Mathf.RoundToInt(world.z / cellSize);
        return new Vector2Int(x, z);
    }
}
