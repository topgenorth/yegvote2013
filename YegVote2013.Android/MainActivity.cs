namespace net.opgenorth.yegvote.droid
{
    using System.Collections.Generic;
    using System.Linq;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Support.V4.App;
    using Android.Util;
    using Android.Views;
    using Android.Widget;

    using net.opgenorth.yegvote.droid.Model;
    using net.opgenorth.yegvote.droid.Service;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : FragmentActivity
    {
        private ElectionResultAdapter _adapter;
        private DisplayElectionResultsReceiver _displayElectionResultsReceiver;
        private Intent _electionServiceIntent;
        private bool _hasData = false;
        private ExpandableListView _listView;
        private LinearLayout _noDataLayout;
        private int _selectedGroup = -1;
        private ElectionResultsServiceConnection _serviceConnection;

        internal ElectionResultsServiceBinder Binder { get; set; }

        internal bool IsBound { get; set; }

        private bool IsAlarmSet { get { return PendingIntent.GetBroadcast(this, 0, _electionServiceIntent, PendingIntentFlags.NoCreate) != null; } }

        public void DisplayElectionResults()
        {
            var wards = Binder.Service.GetWards();
            DisplayElectionResults(wards);
        }

        public void DisplayElectionResults(IEnumerable<Ward> wards)
        {
            if (wards.Any())
            {
                Log.Debug(GetType().FullName, "Updating the List with new results.");
                RunOnUiThread(() =>{
                    _adapter = new ElectionResultAdapter(this, wards);
                    _listView.SetAdapter(_adapter);
                    _listView.Visibility = ViewStates.Visible;
                    _noDataLayout.Visibility = ViewStates.Gone;
                    _hasData = true;

                    if (_selectedGroup > -1)
                    {
                        _listView.SetSelectedGroup(_selectedGroup);
                        _listView.ExpandGroup(_selectedGroup);
                    }
                });
            }
            else
            {
                RunOnUiThread(() =>{
                    Log.Debug(GetType().FullName, "No data to display.");
                    _listView.Visibility = ViewStates.Gone;
                    _noDataLayout.Visibility = ViewStates.Visible;
                    _hasData = false;
                });
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);

            _electionServiceIntent = new Intent(ElectionResultsService.ElectionServiceIntentFilterKey);

            _displayElectionResultsReceiver = new DisplayElectionResultsReceiver();
            _noDataLayout = FindViewById<LinearLayout>(Resource.Id.noDataLayout);

            _listView = FindViewById<ExpandableListView>(Resource.Id.electionResultsListView);
            _listView.GroupCollapse += HandleGroupCollapse;
            _listView.GroupExpand += HandleGroupExpand;
        }

        protected override void OnStart()
        {
            base.OnStart();

            var intentFilter = new IntentFilter(ElectionResultsService.ElectionResultsUpdatedActionKey)
                               {
                                   Priority = (int)IntentFilterPriority.HighPriority
                               };
            RegisterReceiver(_displayElectionResultsReceiver, intentFilter);

            _serviceConnection = new ElectionResultsServiceConnection(this);
            BindService(_electionServiceIntent, _serviceConnection, Bind.AutoCreate);

            ScheduleElectionUpdates();
        }

        protected override void OnStop()
        {
            var msg = string.Empty;
            base.OnStop();
            if (IsBound)
            {
                msg += "Unbinding the service.";
                UnbindService(_serviceConnection);
                _serviceConnection = null;
                IsBound = false;
            }
            msg += " Unregistering the BroadcastReceiver.";
            UnregisterReceiver(_displayElectionResultsReceiver);
            Log.Debug(GetType().FullName, msg);
        }

        private void HandleGroupCollapse(object sender, ExpandableListView.GroupCollapseEventArgs e)
        {
            _selectedGroup = e.GroupPosition;
        }

        private void HandleGroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            if (_selectedGroup != e.GroupPosition)
            {
                _selectedGroup = e.GroupPosition;
            }
        }

        private void ScheduleElectionUpdates()
        {
            if (IsAlarmSet)
            {
                Log.Debug(GetType().FullName, "Alarm is already set.");
                return;
            }
            var alarm = (AlarmManager)GetSystemService(AlarmService);
            var pendingServiceIntent = PendingIntent.GetService(this, 0, _electionServiceIntent, PendingIntentFlags.CancelCurrent);
            alarm.SetRepeating(AlarmType.Rtc, 0, 15000, pendingServiceIntent);
        }
    }
}
