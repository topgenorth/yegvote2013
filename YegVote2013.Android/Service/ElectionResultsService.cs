namespace net.opgenorth.yegvote.droid.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Util;

    using net.opgenorth.yegvote.droid.Model;

    [Service]
    [IntentFilter(new[] { ElectionServiceIntentFilterKey })]
    public class ElectionResultsService : IntentService, IHaveElectionResults
    {
        public const string ElectionResultsUpdatedActionKey = "ElectionResultsUpdated";
        public const string ElectionServiceIntentFilterKey = "net.opgenorth.yegvote.droid.downloaderservice";
        public static readonly string ResultsXmlFile = "https://data.edmonton.ca/api/views/b6ng-fzk2/rows.xml?accessType=DOWNLOAD";
        public static readonly string LogTag = typeof(ElectionResultsService).FullName;
        private IBinder _binder;
        private ElectionResultsParser _electionResultParser = new ElectionResultsParser();

        public List<ElectionResult> ElectionResults { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new ElectionResultsServiceBinder(this);
            Log.Debug(LogTag, "OnBind");
            return _binder;
        }

        protected override async void OnHandleIntent(Intent intent)
        {
            Log.Debug(LogTag, "OnHandleIntent");
            ElectionResults = await UpdateElectionResults();
            var resultsUpdatedIntent = new Intent(ElectionResultsUpdatedActionKey);
            SendOrderedBroadcast(resultsUpdatedIntent, null);
        }

        private async Task<string> DownloadXmlToFileAsync(WebClient webClient)
        {
            var settings = new ElectionServiceDownloadDirectory(this);
            var fileName = settings.GetResultsXmlFileName();
			var oldFileName = fileName + ".old";
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
				if (File.Exists(oldFileName))
				{
					File.Delete(oldFileName);
					Log.Debug(LogTag, "Deleting the backup results file.");
				}
				Log.Debug(LogTag, "Backing up the existing results file.");
                fileInfo.MoveTo(fileName + ".old");
            }

            var uri = new Uri(ResultsXmlFile);
            await webClient.DownloadFileTaskAsync(uri, fileName);
            Log.Debug(LogTag, "Download file to " + fileName + ".");
            return fileName;
        }

        private async Task<List<ElectionResult>> UpdateElectionResults()
        {
            string xmlFile;
            using (var webClient = new WebClient())
            {
                xmlFile = await DownloadXmlToFileAsync(webClient);
            }
            var rows = _electionResultParser.ParseElectionResultFromFile(xmlFile).ToList();
            return rows;
        }
    }
}
