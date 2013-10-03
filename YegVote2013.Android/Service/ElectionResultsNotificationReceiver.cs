using Android.Util;

namespace net.opgenorth.yegvote.droid.Service
{
    using Android.App;
    using Android.Content;
    using Android.Support.V4.App;

    /// <summary>
    /// This Broadcase Receiver will display a notification when new updates have been received.
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new[] { ElectionResultsService.ElectionResultsUpdatedActionKey }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class ElectionResultsNotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

			Log.Debug(GetType().FullName, "New data received.");

            var mgr = (NotificationManager)context.GetSystemService(Context.NotificationService);

            // TODO [TO201310011104] 
            var pendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0);
            var builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentTitle("Election Updated")
                .SetContentText("New election results received.");
            var notification = builder.Build();
            mgr.Notify(0, notification);
        }
    }
}
