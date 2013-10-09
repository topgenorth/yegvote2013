namespace net.opgenorth.yegvote.droid
{
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

    using net.opgenorth.yegvote.droid.Model;
    using net.opgenorth.yegvote.droid.Service;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : FragmentActivity
    {
        public static readonly string Tag = typeof(MainActivity).FullName;
        private ElectionResultAdapter _adapter;
        private AlarmHelper _alarmHelper;
        private DisplayElectionResultsReceiver _displayElectionResultsReceiver;
        private ExpandableListView _listView;
        private ElectionResultsServiceConnection _serviceConnection;
        private MainActivityStateFragment _stateFrag;

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

            if (_stateFrag.HasData)
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
					AndHUD.Shared.Show(this, "Retrieving Election Data");
					_stateFrag.IsDisplayingHud = true;
				}
            }
        }

		protected override void OnResume()
		{
			base.OnResume();
			var mgr = (NotificationManager) GetSystemService(Context.NotificationService);

			mgr.Cancel(ElectionResultsNotificationReceiver.NewElectionResultsNotificationId);
		}
        protected override void OnStart()
        { 
            base.OnStart();
            Log.Debug(Tag, "OnStart");
            BindElectionResultsService();
            ScheduleElectionUpdateAlarm();
        }

        protected override void OnStop()
        {
            CancelElectionUpdateAlarm();
            UnbindElectionResultsService();
            Log.Debug(Tag, "OnStop");
            base.OnStop();
        }

        private void BindElectionResultsService()
        {
            Log.Debug(GetType().FullName, "BindElectionResultsService");
            var intentFilter = new IntentFilter(ElectionResultsService.ElectionResultsUpdatedActionKey)
                               {
                                   Priority = (int)IntentFilterPriority.HighPriority
                               };
            RegisterReceiver(_displayElectionResultsReceiver, intentFilter);
            _serviceConnection = new ElectionResultsServiceConnection(this);
            _stateFrag.HasBoundService = BindService(_alarmHelper.ServiceIntent, _serviceConnection, Bind.AutoCreate);
        }

        private void CancelElectionUpdateAlarm()
        {
            _alarmHelper.CancelAlarm();
            _stateFrag.IsAlarmed = _alarmHelper.IsAlarmSet;
        }

        private void HandleGroupCollapse(object sender, ExpandableListView.GroupCollapseEventArgs e)
        {
            _stateFrag.SelectedGroup = e.GroupPosition;
        }

        private void HandleGroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            _stateFrag.SelectedGroup = e.GroupPosition;
        }

        private void ScheduleElectionUpdateAlarm()
        {
            if (_stateFrag.IsAlarmed)
            {
                return;
            }
#if DEBUG 
            _alarmHelper.SetAlarm(AlarmHelper.Debug_Interval);
#else
            _alarmHelper.SetAlarm(AlarmHelper.Thirty_Minutes);
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
