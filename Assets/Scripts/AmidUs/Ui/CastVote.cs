using System;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui
{
    public class CastVote : MonoBehaviour
    {
        public void Init(Func<bool> canVote, Action voteAction)
        {
            _canVote = canVote;
            _voteAction = voteAction;
            
            _vote.onClick.RemoveAllListeners();
            _vote.onClick.AddListener(OnCastVote);
            
            _cancelVote.onClick.RemoveAllListeners();
            _cancelVote.onClick.AddListener(HideVoteButtons);
            
            _lastSelected = null;
            _hasVoted = false;
            HideVoteButtons();
        }
        
        public void ShowVoteButtons()
        {
            if(!_hasVoted && _canVote.Invoke())
            {
                if (_lastSelected != null)
                {
                    _lastSelected.HideVoteButtons(); // turn off so only one Cast Vote on screen at once
                }
                _votesPanel.SetActive(true);
                _lastSelected = this;
            }
        }

        private void HideVoteButtons()
        {
            _votesPanel.SetActive(false);
            _lastSelected = null;
        }
        
        private void OnCastVote()
        {
            _voteAction.Invoke(); 
            HideVoteButtons();
            _hasVoted = true; // TODO wait for success from server?
        }
        
        public GameObject _votesPanel;
        public Button _vote;
        public Button _cancelVote;

        private Func<bool> _canVote;
        private Action _voteAction;

        private bool _hasVoted;
        private static CastVote _lastSelected;
    }
}