using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;

using YegVote2013.Droid.Model;
using YegVote2013.Droid.Service;

using Fragment = Android.Support.V4.App.Fragment;

namespace YegVote2013.Droid
{
    internal class MainActivityStateFragment : Fragment
    {
        public static readonly string LogTag = typeof(MainActivityStateFragment).FullName.Replace(".", "_");
        private PreferencesHelper _prefHelper;

        public bool HasBoundService { get; set; }

        public bool HasCurrentData
        {
            get
            {
                var isCurrentData = false;
                if ((Wards != null) && Wards.Any())
                {
                    var date = _prefHelper.GetDownloadTimestamp();
                    if (date.HasValue)
                    {
                        var delta = DateTime.UtcNow.Subtract(date.Value);
#if DEBUG
                        isCurrentData = delta.TotalMilliseconds <= AlarmHelper.Debug_Interval;
#else
						isCurrentData = delta.TotalMilliseconds <= AlarmHelper.Fifteen_Minutes;
						#endif
                    }
                }
                return isCurrentData;
            }
        }

        public bool IsAlarmed { get; set; }

        public bool IsDisplayingHud { get; set; }

        public int SelectedGroup { get; set; }

        public IEnumerable<Ward> Wards { get; set; }

        public static MainActivityStateFragment NewInstance()
        {
            var frag = new MainActivityStateFragment
                       {
                           Wards = new Ward[0],
                           HasBoundService = false,
                           IsAlarmed = false,
                           IsDisplayingHud = false,
                           SelectedGroup = -1
                       };
            return frag;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            _prefHelper = new PreferencesHelper(activity);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }
    }
}
