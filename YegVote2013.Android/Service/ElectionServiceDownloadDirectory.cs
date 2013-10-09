namespace YegVote2013.Droid.Service
{
    using System;
    using System.IO;

    using Android.Content;

    using Environment = Android.OS.Environment;

    /// <summary>
    ///   This class will figure out where to store files.
    /// </summary>
    /// <remarks>Different versions of Android have different diretories for storage.</remarks>
    internal class ElectionServiceDownloadDirectory
    {
        private readonly Context _context;
        private readonly PreferencesHelper _prefHelper;

        public ElectionServiceDownloadDirectory(Context context)
        {
            _context = context;
            _prefHelper = new PreferencesHelper(context);
        }

        public bool ResultsAreDownloaded
        {
            get
            {
                var hasCurrentResults = false;
                if (File.Exists(GetResultsXmlFileName()))
                {
                    var date = _prefHelper.GetDownloadTimestamp();
                    if (date.HasValue)
                    {
                        var delta = DateTime.UtcNow.Subtract(date.Value);
#if DEBUG
                        hasCurrentResults = delta.TotalMilliseconds <= AlarmHelper.Debug_Interval;
#else
						hasCurrentResults = delta.TotalMilliseconds <= AlarmHelper.Fifteen_Minutes;
#endif
                    }
                }

                return hasCurrentResults;
            }
        }

        public void EnsureExternalStorageIsUsable()
        {
            if (Environment.MediaMountedReadOnly.Equals(Environment.ExternalStorageState))
            {
                throw new ApplicationException(String.Format("External storage {0} is mounted read-only.", _context.GetExternalFilesDir(null)));
            }
            if (!Environment.MediaMounted.Equals(Environment.ExternalStorageState))
            {
                throw new ApplicationException("External storage is not mounted.");
            }
        }

        public string GetResultsXmlFileName()
        {
            var dir = _context.ExternalCacheDir.AbsolutePath;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "election_results.xml");
        }
    }
}
