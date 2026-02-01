using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;

public enum TeamTurn
{
    Jugador1,
    Jugador2,
    NPC
}

public enum TurnState
{
    Planning,
    MovementExecution
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState CurrentState { get; private set; }
    public TeamTurn CurrentTeamTurn { get; private set; }

    // Contador global de turnos (CLAVE)
    public int TurnCounter { get; private set; }

    // Solo jugadores humanos tienen turno
    private readonly TeamTurn[] playerTurnOrder =
    {
        TeamTurn.Jugador1,
        TeamTurn.Jugador2
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        List<TeamTurn> alivePlayers = GetAlivePlayerTeams();

        if (alivePlayers.Count == 0)
        {
            Debug.LogError("No hay jugadores vivos al iniciar la partida");
            return;
        }

        CurrentTeamTurn = alivePlayers[0];
        CurrentState = TurnState.Planning;
        TurnCounter = 0;
    }

    // ================= TURN FLOW =================

    public void StartMovementExecution()
    {
        if (CurrentState != TurnState.Planning)
            return;

        CurrentState = TurnState.MovementExecution;
        StartCoroutine(ExecuteMovementWithDelay());
    }

    private System.Collections.IEnumerator ExecuteMovementWithDelay()
    {
        // Show Act announcement
        PhaseManager.Instance.PhaseAct();
        
        // Wait for announcement to finish before movement starts
        
        MovementExecution.Instance?.Begin();
        yield return new WaitForSeconds(3f);
        PhaseManager.Instance.PhaseMove();


    }

    public void EndMovementExecution()
    {
        AdvanceTurn();
    }

    // ================= CORE LOGIC =================

    private void AdvanceTurn()
    {
        ResetUnitsTurnState();

        //FIN DE PARTIDA (jugadores + NPC)
        List<TeamTurn> aliveTeams = GetAliveTeams();
        if (aliveTeams.Count <= 1)
        {
            EndGame(aliveTeams[0]);
            return;
        }

        //ROTACIÓN DE TURNOS (solo jugadores vivos)
        List<TeamTurn> alivePlayers = GetAlivePlayerTeams();

        int index = System.Array.IndexOf(playerTurnOrder, CurrentTeamTurn);

        for (int i = 1; i <= playerTurnOrder.Length; i++)
        {
            TeamTurn candidate =
                playerTurnOrder[(index + i) % playerTurnOrder.Length];

            if (alivePlayers.Contains(candidate))
            {
                CurrentTeamTurn = candidate;
                break;
            }
        }

        //Nuevo turno
        TurnCounter++;
        CurrentState = TurnState.Planning;

        Debug.Log($"Turno {TurnCounter} - Turno de {CurrentTeamTurn}");
    }

    // ================= HELPERS =================

    // Jugadores humanos vivos (para turnos)
    private List<TeamTurn> GetAlivePlayerTeams()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        HashSet<TeamTurn> alive = new HashSet<TeamTurn>();

        foreach (Unit u in units)
        {
            if (!u.gameObject.activeInHierarchy)
                continue;

            if (u.team == Unit.Team.NPC)
                continue;

            alive.Add(ConvertTeam(u.team));
        }

        return alive.ToList();
    }

    // Todos los equipos vivos (para victoria)
    private List<TeamTurn> GetAliveTeams()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        HashSet<TeamTurn> alive = new HashSet<TeamTurn>();

        foreach (Unit u in units)
        {
            if (!u.gameObject.activeInHierarchy)
                continue;

            alive.Add(ConvertTeam(u.team));
        }

        return alive.ToList();
    }

    private TeamTurn ConvertTeam(Unit.Team unitTeam)
    {
        return unitTeam switch
        {
            Unit.Team.Jugador1 => TeamTurn.Jugador1,
            Unit.Team.Jugador2 => TeamTurn.Jugador2,
            Unit.Team.NPC => TeamTurn.NPC,
            _ => TeamTurn.NPC
        };
    }

    private void ResetUnitsTurnState()
    {
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            u.ResetPushState();
            u.ClearPlannedMovement();
        }
    }

    private void EndGame(TeamTurn winner)
    {
        Debug.Log($"FIN DEL JUEGO - Gana {winner}");
        // Aquí UI, animaciones, cambio de escena, etc.
        PhaseManager.Instance.PhaseKill();    
    }

    // ================= STATE HELPERS =================

    public bool IsExecutingMovement()
    {
        return CurrentState == TurnState.MovementExecution;
    }

    public bool IsPlanning()
    {
        return CurrentState == TurnState.Planning;
    }
}
