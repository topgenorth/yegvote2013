namespace net.opgenorth.yegvote.droid.Service
{
    using Android.OS;

    public class ElectionResultsServiceBinder : Binder
    {
        public ElectionResultsServiceBinder(ElectionResultsService service)
        {
            Service = service;
        }

        public ElectionResultsService Service { get; private set; }
    }
}