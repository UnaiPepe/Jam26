using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public enum GamePhase
    {
        Setup,
        Move,
        Act,
        Start,
        Kill,
        Victory,
        Defeat
    }

    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        [Header("Phase State")]
        public GamePhase currentPhase = GamePhase.Setup;

        [Header("Move Phase")]
        public GameObject moveAnnouncement;
        public GameObject moveIndicator;

        [Header("Act Phase")]
        public GameObject actAnnouncement;
        public GameObject actIndicator;

        [Header("Start Phase")]
        public GameObject startAnnouncement;
        public GameObject startIndicator;

        [Header("Kill Phase")]
        public GameObject killAnnouncement;
        public GameObject killIndicator;

        [Header("Debug")]
        public bool showDebugButtons = true;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Initial setup - skip announcements if needed or start with Move
            ChangePhase(GamePhase.Move);
        }

        public void NextPhase()
        {
            GamePhase next = GamePhase.Move;

            switch (currentPhase)
            {
                case GamePhase.Move:
                    next = GamePhase.Act;
                    break;
                case GamePhase.Act:
                    next = GamePhase.Start;
                    break;
                case GamePhase.Start:
                    next = GamePhase.Kill;
                    break;
                case GamePhase.Kill:
                    HandleKillPhaseEnd();
                    return;
                case GamePhase.Victory:
                case GamePhase.Defeat:
                    Debug.Log("Game Over.");
                    return;
                default:
                    next = GamePhase.Move;
                    break;
            }

            ChangePhase(next);
        }

        private void HandleKillPhaseEnd()
        {
            // Get counts from CharacterManager
            int playersAlive = 0;
            int enemiesAlive = 0;

            if (CharacterManager.Instance != null)
            {
                playersAlive = CharacterManager.Instance.GetAliveCount(Character.Team.Player);
                enemiesAlive = CharacterManager.Instance.GetAliveCount(Character.Team.Enemy);
            }
            else
            {
                Debug.LogWarning("CharacterManager not found! Cannot check win/loss conditions.");
            }

            Debug.Log($"Kill Phase End - Players: {playersAlive}, Enemies: {enemiesAlive}");

            // TODO: Implement actual win/loss logic when ready
            // For now, always cycle back to Move
            GamePhase nextPhase = GamePhase.Move;

            /*
            if (playersAlive == 0 && enemiesAlive == 0)
            {
                nextPhase = GamePhase.Defeat; // Draw = Defeat
            }
            else if (playersAlive == 0)
            {
                nextPhase = GamePhase.Defeat;
            }
            else if (enemiesAlive == 0)
            {
                nextPhase = GamePhase.Victory;
            }
            else
            {
                nextPhase = GamePhase.Move;
            }
            */

            ChangePhase(nextPhase);
        }

        public void ChangePhase(GamePhase newPhase)
        {
            // 1. Disable previous phase's Indicator and any lingering Announcement
            DisableAllIndicators();
            DisableAllAnnouncements();

            currentPhase = newPhase;
            Debug.Log($"Changing Phase to: {newPhase}");

            // 2. Activate BOTH the Announcement and Indicator for the new phase
            GameObject announcement = GetAnnouncement(newPhase);
            GameObject indicator = GetIndicator(newPhase);

            if (announcement != null)
            {
                announcement.SetActive(true);
                // Also activate the first child (the actual text with PhaseAnnouncement script)
                if (announcement.transform.childCount > 0)
                {
                    announcement.transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            if (indicator != null)
            {
                indicator.SetActive(true);
                Debug.Log($"Activated Indicator for {newPhase}");
            }

            // Special Logic
            if (newPhase == GamePhase.Victory) Debug.Log("VICTORY SCENE");
            if (newPhase == GamePhase.Defeat) Debug.Log("DEFEAT SCENE");
        }

        private GameObject GetAnnouncement(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Move => moveAnnouncement,
                GamePhase.Act => actAnnouncement,
                GamePhase.Start => startAnnouncement,
                GamePhase.Kill => killAnnouncement,
                _ => null
            };
        }

        private GameObject GetIndicator(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Move => moveIndicator,
                GamePhase.Act => actIndicator,
                GamePhase.Start => startIndicator,
                GamePhase.Kill => killIndicator,
                _ => null
            };
        }

        private void DisableAllIndicators()
        {
            if (moveIndicator) moveIndicator.SetActive(false);
            if (actIndicator) actIndicator.SetActive(false);
            if (startIndicator) startIndicator.SetActive(false);
            if (killIndicator) killIndicator.SetActive(false);
        }

        private void DisableAllAnnouncements()
        {
            if (moveAnnouncement) moveAnnouncement.SetActive(false);
            if (actAnnouncement) actAnnouncement.SetActive(false);
            if (startAnnouncement) startAnnouncement.SetActive(false);
            if (killAnnouncement) killAnnouncement.SetActive(false);
        }

        private void OnGUI()
        {
            if (!showDebugButtons) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            
            GUILayout.Label($"Phase: {currentPhase}");

            if (GUILayout.Button("Next Phase"))
            {
                NextPhase();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Force Move")) ChangePhase(GamePhase.Move);
            if (GUILayout.Button("Force Act")) ChangePhase(GamePhase.Act);
            if (GUILayout.Button("Force Start")) ChangePhase(GamePhase.Start);
            if (GUILayout.Button("Force Kill")) ChangePhase(GamePhase.Kill);

            GUILayout.EndArea();
        }
    }
}
