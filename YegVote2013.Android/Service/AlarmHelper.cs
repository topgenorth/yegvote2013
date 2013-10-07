namespace net.opgenorth.yegvote.droid.Service
{
    using Android.App;
    using Android.Content;
    using Android.Util;

    public class AlarmHelper
    {
        public static int UpdateElectionResultsAlarmId = 1313;
        private static readonly string Tag = typeof(AlarmHelper).FullName;
        private readonly Context _context;

        public AlarmHelper(Context context)
        {
            _context = context;
            ServiceIntent = new Intent(ElectionResultsService.ElectionServiceIntentFilterKey);
        }

        public AlarmManager AlarmManager { get { return (AlarmManager)_context.GetSystemService(Context.AlarmService); } }

        public bool IsAlarmSet { get; private set; }
        public Intent ServiceIntent { get; set; }

        public void CancelAlarm()
        {
            if (IsAlarmSet)
            {
                var pendingIntent = PendingIntent.GetService(_context, UpdateElectionResultsAlarmId, ServiceIntent, 0);
                AlarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
            IsAlarmSet = false;
            Log.Debug(Tag, "Alarms should all be cancelled.");
        }

        /// <summary>
        ///   Set the alarm to go off at certain intervals.
        /// </summary>
        /// <param name="interval">The interval between each alarm in milliseconds.</param>
        public void SetAlarm(int interval)
        {
            if (IsAlarmSet)
            {
                Log.Debug(Tag, "Alarm is already set.");
                return;
            }
            Log.Debug(Tag, "Setting the alarm for every {0} seconds.", (interval / 1000));

            var pendingIntent = PendingIntent.GetService(_context, UpdateElectionResultsAlarmId, ServiceIntent, PendingIntentFlags.CancelCurrent);
            var alarmManager = (AlarmManager)_context.GetSystemService(Context.AlarmService);
            alarmManager.SetRepeating(AlarmType.Rtc, 0, interval, pendingIntent);
            IsAlarmSet = true;
        }
    }
}
