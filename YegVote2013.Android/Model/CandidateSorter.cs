using System.Collections.Generic;

namespace YegVote2013.Droid.Model
{
    /// <summary>
    ///   Acclaimed candiate should be first, then sort by candidate name
    /// </summary>
    public class CandidateSorter : IComparer<Candidate>
    {
        public int Compare(Candidate x, Candidate y)
        {
            if (x.Acclaimed)
            {
                return 1;
            }
            if (y.Acclaimed)
            {
                return -1;
            }

            var result = y.Percentage.CompareTo(x.Percentage);
            if (result == 0)
            {
                result = x.Name.CompareTo(y.Name);
            }

            return result;
        }
    }
}
