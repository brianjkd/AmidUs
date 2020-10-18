using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmidUs.Ui
{
    public class VotingResults : MonoBehaviour
    {
        private List<VoterIcon> _voterIcons = new List<VoterIcon>();
        public GameObject VoteResults;
        public GameObject VoterIconPrefab;

        public void ClearVoterIcons()
        {
            foreach (var voterIcon in _voterIcons)
            {
                Destroy(voterIcon.gameObject);
            }
            _voterIcons.Clear();
        }
        
        public void ReceiveVote(Player voter)
        {
            var voterIconGo = Instantiate(VoterIconPrefab);
            var voterIcon = voterIconGo.GetComponent<VoterIcon>();
            voterIcon.Init(voter.PlayerColor.Value);
            voterIcon.enabled = false;
            voterIcon.transform.parent = VoteResults.transform;
            voterIcon.transform.position = Vector3.zero;
            _voterIcons.Add(voterIcon);
        }

        public void ShowVotingResults()
        {
            StartCoroutine(ShowVotingResultsAnimation());
        }

        private IEnumerator ShowVotingResultsAnimation()
        {
            foreach (var voterIcon in _voterIcons)
            {
                voterIcon.enabled = true;
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}