namespace YegVote2013.Droid.Service
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
