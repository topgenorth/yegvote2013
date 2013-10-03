namespace net.opgenorth.yegvote.droid.Service
{
    using Android.Content;
    using Android.Util;

    /// <summary>
    /// This Broadcast receiver is used to to update MainActivity when new election results have been downloaded.
    /// </summary>
    public class DisplayElectionResultsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug(GetType().FullName, "Election results broadcast received.");
			var activity = (MainActivity)context;
			activity.DisplayElectionResults();

            
            InvokeAbortBroadcast();
        }
    }
}
