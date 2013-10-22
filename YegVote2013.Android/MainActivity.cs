using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;

using AndroidHUD;

using YegVote2013.Droid.Model;
using YegVote2013.Droid.Service;
using Android.Content;

namespace YegVote2013.Droid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : FragmentActivity
    {
        public static readonly string Tag = typeof(MainActivity).FullName;

        ElectionResultAdapter _adapter;
        AlarmHelper _alarmHelper;
        DisplayElectionResultsReceiver _displayElectionResultsReceiver;
        ExpandableListView _listView;
        ElectionResultsServiceConnection _serviceConnection;
        MainActivityStateFragment _stateFrag;

        internal ElectionResultsServiceBinder Binder { get; set; }

        public void DisplayElectionResults()
        {
            var builder = new WardBuilder();

            IEnumerable<Ward> wards = builder.GetWards(Binder.Service.ElectionResults);

            DisplayElectionResults(wards);
        }

        public void DisplayElectionResults(IEnumerable<Ward> wards)
        {
            _stateFrag.Wards = wards ?? new Ward[0];

            if (wards.Any())
            {
                Log.Debug(GetType().FullName, "Updating the List with new results.");
                RunOnUiThread(() =>{
                    _adapter = new ElectionResultAdapter(this, wards);
                    _listView.SetAdapter(_adapter);

                    if (_stateFrag.SelectedGroup > -1)
                    {
                        _listView.SetSelectedGroup(_stateFrag.SelectedGroup);
                        _listView.ExpandGroup(_stateFrag.SelectedGroup);
                    }
                    AndHUD.Shared.Dismiss(this);
                    _stateFrag.IsDisplayingHud = false;
                });
            }
            else
            {
                RunOnUiThread(() =>{
                    Log.Debug(GetType().FullName, "No data to display.");
                    _listView.Visibility = ViewStates.Gone;
                    AndHUD.Shared.ShowToast(this, "There is no data to display!", MaskType.Clear, TimeSpan.FromSeconds(2));
                });
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			InitializeActionBar();
            SetContentView(Resource.Layout.activity_main);

            _stateFrag = SupportFragmentManager.FindFragmentByTag(MainActivityStateFragment.LogTag) as MainActivityStateFragment;
            if (_stateFrag == null)
            {
                _stateFrag = MainActivityStateFragment.NewInstance();
                var tx = SupportFragmentManager.BeginTransaction();
                tx.Add(_stateFrag, MainActivityStateFragment.LogTag);
                tx.Commit();
            }

            _alarmHelper = new AlarmHelper(BaseContext);
            _displayElectionResultsReceiver = new DisplayElectionResultsReceiver();


            _listView = FindViewById<ExpandableListView>(Resource.Id.electionResultsListView);
            _listView.GroupCollapse += HandleGroupCollapse;
            _listView.GroupExpand += HandleGroupExpand;
        }

		void InitializeActionBar() 
		{
			if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.Honeycomb)
			{
				RequestWindowFeature(WindowFeatures.ActionBar);
			}
		}
        protected override void OnResume()
        {
            base.OnResume();
            var mgr = (NotificationManager)GetSystemService(NotificationService);
            mgr.Cancel(ElectionResultsNotificationReceiver.NewElectionResultsNotificationId);

            if (_stateFrag.HasCurrentData)
            {
                DisplayElectionResults(_stateFrag.Wards);
            }
            else
            {
                var settings = new ElectionServiceDownloadDirectory(this);
                if (settings.ResultsAreDownloaded)
                {
                    var wardBuilder = new WardBuilder();
                    DisplayElectionResults(wardBuilder.GetWards(settings.GetResultsXmlFileName()));
                    _stateFrag.IsDisplayingHud = false;
                }
                else
                {
                    AndHUD.Shared.Show(this, "Updating...");
                    _stateFrag.IsDisplayingHud = true;
                }
            }
        }

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.menu_refresh_data:
					var intent = new Intent(ElectionResultsService.ElectionResultsUpdatedActionKey);
					Log.Debug(Tag, "User asked to update the data.");
					SendOrderedBroadcast(intent, null);
					Toast.MakeText(this, "Data refresh requested...", ToastLength.Short).Show();
					return true;
				default:
					Log.Warn(Tag, "Don't know how to handle MenuItem {0}, {1}.", item.TitleFormatted, item, item.ItemId);
					return true;
			}
		}

        protected override void OnStart()
        {
            base.OnStart();
            BindElectionResultsService();
            ScheduleElectionUpdateAlarm();
        }

        protected override void OnStop()
        {
            CancelElectionUpdateAlarm();
            UnbindElectionResultsService();
            base.OnStop();
        }

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_mainactivity, menu);
			return true;
		}

        void BindElectionResultsService()        
		{
            var intentFilter = new IntentFilter(ElectionResultsService.ElectionResultsUpdatedActionKey)
                               {
                                   Priority = (int)IntentFilterPriority.HighPriority
                               };
            RegisterReceiver(_displayElectionResultsReceiver, intentFilter);
            _serviceConnection = new ElectionResultsServiceConnection(this);
            _stateFrag.HasBoundService = BindService(_alarmHelper.ServiceIntent, _serviceConnection, Bind.AutoCreate);
        }

        void CancelElectionUpdateAlarm()
        {
            _alarmHelper.CancelAlarm();
            _stateFrag.IsAlarmed = _alarmHelper.IsAlarmSet;
        }

        void HandleGroupCollapse(object sender, ExpandableListView.GroupCollapseEventArgs e)
        {
            _stateFrag.SelectedGroup = e.GroupPosition;
        }

        void HandleGroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            _stateFrag.SelectedGroup = e.GroupPosition;
        }

        void ScheduleElectionUpdateAlarm()
        {
            if (_stateFrag.IsAlarmed)
            {
                return;
            }
#if DEBUG
            _alarmHelper.SetAlarm(AlarmHelper.DebugInterval);
#else
            _alarmHelper.SetAlarm(AlarmHelper.FiveMinutes);
#endif
            _stateFrag.IsAlarmed = _alarmHelper.IsAlarmSet;
        }

        private void UnbindElectionResultsService()
        {
            var msg = string.Empty;
            if (_stateFrag.HasBoundService)
            {
                msg += "Unbinding the service.";
                UnbindService(_serviceConnection);
                if (Binder != null)
                {
                    Binder.Service.StopSelf();
                }
                _serviceConnection = null;

                msg += " Unregistering the BroadcastReceiver.";
                UnregisterReceiver(_displayElectionResultsReceiver);
                Log.Debug(GetType().FullName, msg);
                _stateFrag.HasBoundService = false;
            }
        }
    }
}
