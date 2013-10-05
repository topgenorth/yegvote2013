namespace net.opgenorth.yegvote.droid.Service
{
    using System;
    using System.IO;

    using Android.Content;

    using Environment = Android.OS.Environment;

    /// <summary>
    ///   This class will figure out where ReportGraffiti should store it's pictures and thumbnails
    /// </summary>
    internal class AndroidExternalStorageDirectoryFactory
    {
        private readonly Context _context;

        public AndroidExternalStorageDirectoryFactory(Context context)
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

        public string GetResultsXmlFile()
        {
            var javaDir = Environment.DataDirectory;
            var dir = javaDir.AbsolutePath;
            return Path.Combine(dir, "election_results.xml");
        }
    }
}
