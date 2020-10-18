using System;

namespace AmidUs.Ui
{
    public class VoteTally : IComparable
    {
        public VoteTally(ulong playerId, int votesAgainstCount)
        {
            PlayerId = playerId;
            VotesAgainstCount = votesAgainstCount;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var other = obj as VoteTally;
            return VotesAgainstCount.CompareTo(other.VotesAgainstCount);
        }
        
        public ulong PlayerId { get; }
        public int VotesAgainstCount { get; }
    }
    
}