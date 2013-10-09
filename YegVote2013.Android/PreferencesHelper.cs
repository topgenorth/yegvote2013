namespace YegVote2013.Droid
{
    using System;

    using Android.Content;

    internal class PreferencesHelper
    {
        private readonly Context _context;

        public PreferencesHelper(Context context)
        {
            _context = context;
        }

        public string PreferenceFile { get { return _context.GetString(Resource.String.last_download_pref); } }

        public DateTime? GetDownloadTimestamp()
        {
            var sharedPref = _context.GetSharedPreferences(PreferenceFile, FileCreationMode.Private);
            var val = sharedPref.GetString("LAST_DOWNLOAD", null);
            if (val == null)
            {
                return null;
            }

            var date = DateTime.Parse(val);
            return date;
        }

        public void UpdateDownloadTimestamp()
        {
            var sharedPref = _context.GetSharedPreferences(PreferenceFile, FileCreationMode.Private);
            var editor = sharedPref.Edit();
            editor.PutString("LAST_DOWNLOAD", DateTime.UtcNow.ToString());
            editor.Commit();
        }
    }
}
