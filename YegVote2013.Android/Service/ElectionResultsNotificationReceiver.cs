﻿namespace net.opgenorth.yegvote.droid.Service
{
    using Android.App;
    using Android.Content;
    using Android.Support.V4.App;
    using Android.Util;

    /// <summary>
    ///   This Broadcase Receiver will display a notification when new updates have been received.
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new[] { ElectionResultsService.ElectionResultsUpdatedActionKey }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class ElectionResultsNotificationReceiver : BroadcastReceiver
    {
        public static readonly int NewElectionResultsNotificationId = 1000;

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug(GetType().FullName, "New data received.");

            var mgr = (NotificationManager)context.GetSystemService(Context.NotificationService);

            // TODO [TO201310011104] 
            var contentIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0);

            var builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentTitle("Election Updated")
                .SetContentIntent(contentIntent)
                .SetContentText("New election results received.");
            var notification = builder.Build();
            mgr.Notify(NewElectionResultsNotificationId, notification);
        }
    }
}
