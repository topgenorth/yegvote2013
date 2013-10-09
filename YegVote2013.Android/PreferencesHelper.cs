using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace net.opgenorth.yegvote.droid
{
	class PreferencesHelper
	{
		Context _context;

		public PreferencesHelper(Context context)
		{
			_context = context;
		}

		public string PreferenceFile
		{
			get
			{
				return _context.GetString(Resource.String.last_download_pref);
			}
		}

		public void UpdateDownloadTimestamp()
		{
			var sharedPref = _context.GetSharedPreferences(PreferenceFile, FileCreationMode.Private);
			var editor = sharedPref.Edit();
			editor.PutString("LAST_DOWNLOAD", DateTime.UtcNow.ToString());
			editor.Commit();
		}

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
	}
}

