namespace net.opgenorth.yegvote.droid.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Util;

    using net.opgenorth.yegvote.droid.Model;

    [Service]
    [IntentFilter(new[] { ElectionServiceIntentFilterKey })]
    public class ElectionResultsService : IntentService
    {
        /// <summary>
        ///   A string specifying that the results have been updated.
        /// </summary>
        public const string ElectionResultsUpdatedActionKey = "ElectionResultsUpdated";
        /// <summary>
        ///   This is the name of the IntentService.
        /// </summary>
        public const string ElectionServiceIntentFilterKey = "net.opgenorth.yegvote.droid.downloaderservice";
        /// <summary>
        ///   The URL to get the data from.
        /// </summary>
        public static readonly string ResultsXmlFile = "https://data.edmonton.ca/api/views/b6ng-fzk2/rows.xml?accessType=DOWNLOAD";
        public static readonly string Tag = typeof(ElectionResultsService).FullName;
        private IBinder _binder;

        private List<ElectionResult> ElectionResults { get; set; }

        /// <summary>
        ///   Return a list of Wards.
        /// </summary>
        /// <returns>The wards.</returns>
        public List<Ward> GetWards()
        {
            var wards = new List<Ward>();
            var currentRaceId = -1;
            Ward currentWard = null;
            foreach (var electionResult in ElectionResults.OrderBy(er => er.RaceId))
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

        public override IBinder OnBind(Intent intent)
        {
            _binder = new ElectionResultsServiceBinder(this);
            Log.Debug(Tag, "OnBind");
            return _binder;
        }

        public override void OnRebind(Intent intent)
        {
            base.OnRebind(intent);
            Log.Debug(Tag, "OnRebind");
        }

        public override bool OnUnbind(Intent intent)
        {
            Log.Debug(Tag, "OnUnbind");
            return base.OnUnbind(intent);
        }

        protected override async void OnHandleIntent(Intent intent)
        {
            Log.Debug(Tag, "OnHandleIntent");
            ElectionResults = await UpdateElectionResults();
            var resultsUpdatedIntent = new Intent(ElectionResultsUpdatedActionKey);
            SendOrderedBroadcast(resultsUpdatedIntent, null);
        }

        private async Task<string> DownloadXmlAsync(WebClient webClient)
        {
            var uri = new Uri(ResultsXmlFile);
            string resultsXml;
            resultsXml = await webClient.DownloadStringTaskAsync(uri);
            return resultsXml;
        }

        private async Task DownloadXmlToFileAsync(WebClient webClient)
        {
            var settings = new AndroidExternalStorageDirectoryFactory(this);
            var fileName = settings.GetResultsXmlFile();
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                fileInfo.MoveTo(fileName + ".old");
            }
            var uri = new Uri(ResultsXmlFile);
            await webClient.DownloadFileTaskAsync(uri, fileName);
            Log.Debug(Tag, "Download file to " + fileName + ".");
        }

        private ElectionResult ParseElectionResultXml(XElement result)
        {
            var r = new ElectionResult();

            try
            { // ReSharper disable PossibleNullReferenceException
                r.Id = Int32.Parse(result.Attribute("_id").Value);
                r.UUID = new Guid(result.Attribute("_uuid").Value);
                r.Address = result.Attribute("_address").Value;
                // TODO [TO201310010954] 
                //            r.ReportedAt =  DateTime.Parse(result.Element("reported_at").Value);
                r.RaceId = Int32.Parse(result.Element("race_id").Value);
                r.Contest = result.Element("contest").Value;
                r.WardName = result.Element("ward_name").Value;
                r.Acclaimed = result.Element("acclaimed").Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                r.Reporting = Int32.Parse(result.Element("reporting").Value);
                r.OutOf = Int32.Parse(result.Element("out_of").Value);
                r.VotesCast = Int32.Parse(result.Element("votes_cast").Value);
                r.Race = Int32.Parse(result.Element("race").Value);
                r.CandidateName = result.Element("candidate_name").Value;
                r.VotesReceived = Int32.Parse(result.Element("votes_received").Value);
                r.Percentage = float.Parse(result.Element("percentage").Value);
                // ReSharper restore PossibleNullReferenceException
            }
            catch (Exception e)
            {
                Log.Error(Tag, e.ToString());
                r = null;
            }
            return r;
        }

        private async Task<List<ElectionResult>> UpdateElectionResults()
        {
            string resultsXml;
            using (var webClient = new WebClient())
            {
                resultsXml = await DownloadXmlAsync(webClient);
                //				await this.DownloadXmlToFileAsync(webClient);
            }

            var xdoc = XDocument.Parse(resultsXml);
            var lvl1 = xdoc.Descendants("row");
            var lvl2 = lvl1.Descendants("row");
            var rows = lvl2.Select(ParseElectionResultXml).ToList();
            return rows;
        }
    }
}
