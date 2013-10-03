namespace net.opgenorth.yegvote.droid.Service
{
    using Android.Content;
    using Android.OS;

    public class ElectionResultsServiceConnection : Java.Lang.Object, IServiceConnection
    {
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
            _activity.IsBound = true;
            _activity.Binder = binder;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _activity.IsBound = false;
        }
    }
}