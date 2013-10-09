namespace YegVote2013.Droid.Service
{
    using System;

    using Android.Content;
    using Android.Util;

    
    /// <summary>
    ///   This Broadcast receiver is used to to update MainActivity when new election results have been downloaded.
    /// </summary>
    public class DisplayElectionResultsReceiver : BroadcastReceiver
    {
        private static readonly string Tag = typeof(DisplayElectionResultsReceiver).FullName;

        public override void OnReceive(Context context, Intent intent)
        {
            if (ElectionResultsService.ElectionResultsUpdatedActionKey.Equals(intent.Action, StringComparison.OrdinalIgnoreCase))
            {
                Log.Debug(Tag, "Time to display election results");
                var activity = (MainActivity)context;
                activity.DisplayElectionResults();
                InvokeAbortBroadcast();
            }
            else
            {
                Log.Warn(Tag, "Don't know what to do with intent {0}.", intent.Action);
            }
        }
    }
}
