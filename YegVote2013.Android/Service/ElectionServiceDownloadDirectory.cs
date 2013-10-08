namespace net.opgenorth.yegvote.droid.Service
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

        public ElectionServiceDownloadDirectory(Context context)
        {
            _context = context;
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

        public bool ResultsAreDownloaded
        {
            get
            {
                // TODO [TO201310041632] Maybe check to see how old the file is, > 30 minutes should return false?
                return File.Exists(GetResultsXmlFile());
            }
        }
        public string GetResultsXmlFile()
        {
            var dir = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
            return Path.Combine(dir, "election_results.xml");
        }
    }
}
