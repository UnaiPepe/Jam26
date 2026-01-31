using System;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts.Character
{
    public enum Team
    {
        Player,
        Enemy
    }

    [Serializable]
    public class Attack
    {
        public string attackName;
        // TODO: Add more properties later (damage, range, etc.)
    }

    public class CharacterAttributes : MonoBehaviour
    {
        [Header("Stats")]
        public int health = 3;
        public Team team = Team.Player;

        [Header("Move Phase Attacks")]
        public Attack[] moveAttacks = new Attack[3];

        [Header("Act Phase Attacks")]
        public Attack[] actAttacks = new Attack[3];

        [Header("Start Phase Attacks")]
        public Attack[] startAttacks = new Attack[3];

        [Header("Kill Phase Attacks")]
        public Attack[] killAttacks = new Attack[3];

        public bool IsAlive => health > 0;

        private void OnEnable()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.RegisterCharacter(this);
            }
        }

        private void OnDisable()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.UnregisterCharacter(this);
            }
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                health = 0;
                Debug.Log($"{gameObject.name} has been defeated!");
            }
        }
    }
}
