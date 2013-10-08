namespace net.opgenorth.yegvote.droid.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class WardBuilder
    {
        /// <summary>
        ///   Return a list of Wards.
        /// </summary>
        /// <returns>The wards.</returns>
        public List<Ward> GetWards(IEnumerable<ElectionResult> electionResults)
        {
            var wards = new List<Ward>();
            var currentRaceId = -1;
            Ward currentWard = null;
            foreach (var electionResult in electionResults.OrderBy(er => er.RaceId))
            {
                if (currentRaceId != electionResult.RaceId)
                {
                    if (currentWard != null)
                    {
                        currentWard.Candidates.Sort(new CandidateSorter());
                    }
                    currentWard = Ward.NewInstance(electionResult);
                    wards.Add(currentWard);
                    currentRaceId = electionResult.RaceId;
                }
                else
                {
                    currentWard.AddCandiate(electionResult);
                }
            }
            return wards;
        }
    }
}