using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Character;

namespace Assets.Scripts.Managers
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        private List<CharacterAttributes> _allCharacters = new List<CharacterAttributes>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterCharacter(CharacterAttributes character)
        {
            if (!_allCharacters.Contains(character))
            {
                _allCharacters.Add(character);
                Debug.Log($"Registered: {character.gameObject.name} ({character.team})");
            }
        }

        public void UnregisterCharacter(CharacterAttributes character)
        {
            if (_allCharacters.Contains(character))
            {
                _allCharacters.Remove(character);
                Debug.Log($"Unregistered: {character.gameObject.name}");
            }
        }

        public int GetAliveCount(Team team)
        {
            return _allCharacters.Count(c => c.team == team && c.IsAlive);
        }

        public int GetDeadCount(Team team)
        {
            return _allCharacters.Count(c => c.team == team && !c.IsAlive);
        }

        public List<CharacterAttributes> GetCharactersByTeam(Team team)
        {
            return _allCharacters.Where(c => c.team == team).ToList();
        }

        public List<CharacterAttributes> GetAliveCharacters(Team team)
        {
            return _allCharacters.Where(c => c.team == team && c.IsAlive).ToList();
        }

        [ContextMenu("Debug: Print Character Counts")]
        public void DebugPrintCounts()
        {
            int playersAlive = GetAliveCount(Team.Player);
            int playersDead = GetDeadCount(Team.Player);
            int enemiesAlive = GetAliveCount(Team.Enemy);
            int enemiesDead = GetDeadCount(Team.Enemy);

            Debug.Log($"=== CHARACTER STATUS ===");
            Debug.Log($"Players: {playersAlive} alive, {playersDead} dead");
            Debug.Log($"Enemies: {enemiesAlive} alive, {enemiesDead} dead");
            Debug.Log($"========================");
        }

    }
}
