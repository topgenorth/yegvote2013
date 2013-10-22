using Android.App;
using Android.Content;
using Android.Util;

namespace YegVote2013.Droid.Service
{
    public class AlarmHelper
    {
        public const int DebugInterval = 30 * 1000;
        public const int FiveMinutes = 5 * 60 * 1000;
        public static int UpdateElectionResultsAlarmId = 1313;
        static readonly string LogTag = typeof(AlarmHelper).FullName;
        readonly Context _context;

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
            Log.Debug(LogTag, "Alarms should all be cancelled.");
        }

        public void SetAlarm(int interval)
        {
            if (IsAlarmSet)
            {
                Log.Debug(LogTag, "Alarm is already set.");
                return;
            }
            Log.Debug(LogTag, "Setting the alarm for every {0} seconds.", (interval / 1000));

            var pendingIntent = PendingIntent.GetService(_context, UpdateElectionResultsAlarmId, ServiceIntent, PendingIntentFlags.CancelCurrent);
            var alarmManager = (AlarmManager)_context.GetSystemService(Context.AlarmService);
            alarmManager.SetRepeating(AlarmType.Rtc, 0, interval, pendingIntent);
            IsAlarmSet = true;
        }
    }
}
