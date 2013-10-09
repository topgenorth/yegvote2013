namespace YegVote2013.Droid.Service
{
    using Android.Content;
    using Android.OS;
    using Android.Util;

    using Java.Lang;


    public class ElectionResultsServiceConnection : Object, IServiceConnection
    {
        private static readonly string Tag = typeof(ElectionResultsServiceConnection).FullName;
        private readonly MainActivity _activity;

        public ElectionResultsServiceConnection(MainActivity activity)
        {
            _activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = service as ElectionResultsServiceBinder;
            if (binder == null)
            {
                return;
            }
            _activity.Binder = binder;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Log.Debug(Tag, "Service disconnected.");
        }
    }
}
