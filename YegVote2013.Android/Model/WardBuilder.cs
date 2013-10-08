namespace net.opgenorth.yegvote.droid.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    public class WardBuilder
    {

		public List<Ward> GetWards(string xmlFileName) 
		{
			var electionParser = new ElectionResultsParser();
			var electionResults = electionParser.ParseElectionResultFromFile(xmlFileName);
			return GetWards(electionResults);
		}

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
                        currentWard.LastUpdatedAt = GetTimeUpdated(currentWard);
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

        private DateTime GetTimeUpdated(Ward ward)
        {
            var candidates = from c in ward.Candidates
                orderby c.ReportedAt descending
                select c;

			var newestCandidate = candidates.First();
			return newestCandidate.ReportedAt;
        }
    }
}