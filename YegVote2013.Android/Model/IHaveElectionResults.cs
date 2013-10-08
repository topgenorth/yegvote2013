namespace net.opgenorth.yegvote.droid.Model
{
    using System.Collections.Generic;

    public interface IHaveElectionResults
    {
        List<ElectionResult> ElectionResults { get; }
    }
}