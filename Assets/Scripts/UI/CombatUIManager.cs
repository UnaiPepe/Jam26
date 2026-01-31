using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using Assets.Scripts.Character;
using Assets.Scripts.Managers;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class CombatUIManager : MonoBehaviour
    {
        public static CombatUIManager Instance { get; private set; }

        [Header("Main Containers")]
        public GameObject mainContainer; // The whole UI panel
        public GameObject subMenuContainer; // Where attack buttons go

        [Header("Category Buttons")]
        public Button btnMoveCat;
        public Button btnActCat;
        public Button btnStartCat;
        public Button btnKillCat;

        [Header("Sub Menu Settings")]
        [Tooltip("Prefab for the attack button")]
        public GameObject attackButtonPrefab; // Prefab with Button & Text
        public Transform attackButtonParent;  // Layout group for sub-buttons

        //private CharacterAttributes _selectedCharacter;
        private List<GameObject> _spawnedButtons = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Hide initially
            if (mainContainer) mainContainer.SetActive(false);
            if (subMenuContainer) subMenuContainer.SetActive(false);

            // Setup Listeners (Hover events need EventTriggers, but for now we can do clicks or use simple pointer enter scripts)
            // For simplicity in this first pass, we will verify with Clicks/Hovers if user adds EventTriggers.
            // I'll add a helper to add triggers at runtime.
            SetupHoverEvents(btnMoveCat.gameObject, "Move");
            SetupHoverEvents(btnActCat.gameObject, "Act");
            SetupHoverEvents(btnStartCat.gameObject, "Start");
            SetupHoverEvents(btnKillCat.gameObject, "Kill");
        }

        private void SetupHoverEvents(GameObject go, string category)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            if (trigger == null) trigger = go.AddComponent<EventTrigger>();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { ShowSubMenu(category); });
            trigger.triggers.Add(entryEnter);

            // Optional: Close on exit? Maybe better to keep open until another is hovered.
            // EventTrigger.Entry entryExit = new EventTrigger.Entry();
            // entryExit.eventID = EventTriggerType.PointerExit;
            // entryExit.callback.AddListener((data) => { HideSubMenu(); });
            // trigger.triggers.Add(entryExit);
        }
        /*
        public void ShowUIFor(CharacterAttributes character)
        {
            _selectedCharacter = character;
            if (mainContainer) mainContainer.SetActive(true);
            
            // Default: Hide sub menu until hover
            if (subMenuContainer) subMenuContainer.SetActive(false);

            Debug.Log($"Combat UI Opened for {character.name}");
        }
        */
        public void HideUI()
        {
            if (mainContainer) mainContainer.SetActive(false);
            if (subMenuContainer) subMenuContainer.SetActive(false);
            //_selectedCharacter = null;
        }

        public void ShowSubMenu(string category)
        {
            /*
            if (_selectedCharacter == null || subMenuContainer == null) return;
            */
            subMenuContainer.SetActive(true);
            
            // Clear old buttons
            foreach (var btn in _spawnedButtons) Destroy(btn);
            _spawnedButtons.Clear();

            // Get Attacks
            //Attack[] attacks = null;
            switch (category)
            {
                /*
                case "Move": attacks = _selectedCharacter.moveAttacks; break;
                case "Act": attacks = _selectedCharacter.actAttacks; break;
                case "Start": attacks = _selectedCharacter.startAttacks; break;
                case "Kill": attacks = _selectedCharacter.killAttacks; break;
                */
            }

            //if (attacks == null) return;

            // Create Buttons
            /*
            foreach (var atk in attacks)
            {
                if (string.IsNullOrEmpty(atk.attackName)) continue;

                GameObject btnObj = Instantiate(attackButtonPrefab, attackButtonParent);
                _spawnedButtons.Add(btnObj);

                TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt) txt.text = atk.attackName;

                Button btn = btnObj.GetComponent<Button>();
                if (btn)
                {
                    btn.onClick.AddListener(() => OnAttackClicked(atk));
                }
            }
            */
        }
        /*
        private void OnAttackClicked(Attack attack)
        {
            Debug.Log($"Selected Attack: {attack.attackName}");
            // Here we would trigger the attack logic
        }
        */
    }
}
