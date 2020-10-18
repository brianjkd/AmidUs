using System.Collections.Generic;
using AmidUs.Server;
using AmidUs.Ui.Panels;
using UnityEngine;

namespace AmidUs.Ui
{
    public class UiManager : MonoBehaviour
    {
        void Awake()
        {
            _panelTypeToPanels= new Dictionary<PanelType, UiPanel>();
            _panelTypeToPanels.Add(PanelType.Gameplay, GameplayUi);
            _panelTypeToPanels.Add(PanelType.Voting, VotingUi);
            _panelTypeToPanels.Add(PanelType.VoteResults, VoteResultsUi);
            _panelTypeToPanels.Add(PanelType.Task, TaskUi);
            _panelTypeToPanels.Add(PanelType.Lobby, LobbyUi);
            _panelTypeToPanels.Add(PanelType.MainMenu, MainMenuUi);
            _panelTypeToPanels.Add(PanelType.GameOver, GameOverUi);

            foreach (var panel in _panelTypeToPanels.Values)
            {
                panel.Initialize();
            }

            Show(PanelType.MainMenu);
        }

        public void Show(PanelType panelType)
        {
            HideAll();
            _panelTypeToPanels[panelType].Show();
            CurrentDisplayed = panelType;
        }

        private void HideAll()
        {
            foreach (var panel in _panelTypeToPanels.Values)
            {
                panel.Hide();
            }
        }

        public void GoBackToMainMenu()
        {
            var hostGame = FindObjectOfType<HostGame>();
            hostGame.AbandonGame();
            var uiManager = FindObjectOfType<UiManager>();
            uiManager.Show(PanelType.MainMenu);
        }
        
        public GameplayUi GameplayUi;
        public VotingUi VotingUi;
        public VoteResultsUi VoteResultsUi;
        public TaskUi TaskUi;
        public LobbyUi LobbyUi;
        public MainMenuUi MainMenuUi;
        public GameOverUi GameOverUi;

        public PanelType CurrentDisplayed { get; private set; }
        private Dictionary<PanelType, UiPanel> _panelTypeToPanels;
    }
}