namespace net.opgenorth.yegvote.droid.Service
{
    using Android.App;
    using Android.Content;
    using Android.Util;

    public class AlarmHelper
    {
        private static readonly string Tag = typeof(AlarmHelper).FullName;

        private readonly Context _context;

        public AlarmHelper(Context context)
        {
            _context = context;
            ServiceIntent = new Intent(ElectionResultsService.ElectionServiceIntentFilterKey);
        }

        public bool IsAlarmSet { get { return QueryIfAlarmIsSet(); } }
        public Intent ServiceIntent { get; set; }

        public void CancelAlarm()
        {
            var alarmUp = QueryIfAlarmIsSet();
            var alarmManager = (AlarmManager)_context.GetSystemService(Context.AlarmService);

            while (QueryIfAlarmIsSet())
            {
                var pendingIntent = PendingIntent.GetBroadcast(_context, 0, ServiceIntent, 0);
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }

            Log.Debug(Tag, "Alarms should all be cancelled.");

            return;
        }

        /// <summary>
        ///   Set the alarm to go off at certain intervals.
        /// </summary>
        /// <param name="interval">The interval between each alarm in milliseconds.</param>
        public void SetAlarm(int interval)
        {
            var alarmUp = QueryIfAlarmIsSet();

            if (IsAlarmSet)
            {
                Log.Debug(Tag, "Alarm is already set.");
                return;
            }
            Log.Debug(Tag, "Setting the alarm.");

            var pendingIntent = PendingIntent.GetService(_context, 0, ServiceIntent, PendingIntentFlags.CancelCurrent);
            var alarmManager = (AlarmManager)_context.GetSystemService(Context.AlarmService);
            alarmManager.SetRepeating(AlarmType.Rtc, 0, interval, pendingIntent);
        }

        private bool QueryIfAlarmIsSet()
        {
            var pendingIntent = PendingIntent.GetBroadcast(_context, 0, ServiceIntent, PendingIntentFlags.NoCreate);
            return pendingIntent != null;
        }
    }
}
