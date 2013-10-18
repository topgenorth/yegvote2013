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

using YegVote2013.Droid.Model;
using System.Linq.Expressions;

namespace YegVote2013.Droid.Service
{
    [Service]
    [IntentFilter(new[] { ElectionServiceIntentFilterKey })]
    public class ElectionResultsService : IntentService, IHaveElectionResults
    {
        public const string ElectionResultsUpdatedActionKey = "ElectionResultsUpdated";
        public const string ElectionServiceIntentFilterKey = "net.opgenorth.yegvote.droid.downloaderservice";
        public static readonly string LogTag = typeof(ElectionResultsService).FullName;
        public static readonly string ResultsXmlFile = "https://data.edmonton.ca/api/views/b6ng-fzk2/rows.xml?accessType=DOWNLOAD";

		static bool _isDownloading = false;
		IBinder _binder;
        readonly ElectionResultsParser _electionResultParser = new ElectionResultsParser();

        public List<ElectionResult> ElectionResults { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new ElectionResultsServiceBinder(this);
            return _binder;
        }

        protected override async void OnHandleIntent(Intent intent)
        {
			if (_isDownloading)
			{
				Log.Info(LogTag, "It seems we're already downloading results.");
				var settings = new ElectionServiceDownloadDirectory(this);
				var fileName = settings.GetResultsXmlFileName();
				ElectionResults = _electionResultParser.ParseElectionResultFromFile(fileName).ToList();
			}
			else
			{
				try 
				{
					_isDownloading = true;
					ElectionResults = await UpdateElectionResults();
				}
				catch (Exception ex)
				{
					Log.Error(LogTag, "Oh no, error trying to update: " + ex);
				}
				finally
				{
					_isDownloading = false;
				}
			}
            var resultsUpdatedIntent = new Intent(ElectionResultsUpdatedActionKey);
            SendOrderedBroadcast(resultsUpdatedIntent, null);
        }

        async Task<string> DownloadXmlToFileAsync(WebClient webClient)
        {
            var fileName = GetFilenameOfDownload();
            var uri = new Uri(ResultsXmlFile);
			var downloaded = false;
			try 
			{
            	await webClient.DownloadFileTaskAsync(uri, fileName);
				downloaded=true;
			}
			catch (Exception ex)
			{
				Log.Error(LogTag, "Couldn't download the file {0} to {1}: {2}", ResultsXmlFile, fileName, ex);
				downloaded = false;
			}

			if (File.Exists(fileName))
			{
				SaveDownloadTimestamp();
				if (downloaded)
				{
					Log.Debug(LogTag, "Download file to " + fileName + ".");
				}
				return fileName;
			}
			else
			{
				throw new FileNotFoundException("Don't have the results XML file.", fileName);
			}
        }

        string GetFilenameOfDownload()
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
            return fileName;
        }

        void SaveDownloadTimestamp()
        {
            var prefHelper = new PreferencesHelper(this);
            prefHelper.UpdateDownloadTimestamp();
        }

        async Task<List<ElectionResult>> UpdateElectionResults()
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
