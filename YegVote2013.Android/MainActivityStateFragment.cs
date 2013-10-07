namespace net.opgenorth.yegvote.droid
{
    using System.Collections.Generic;
    using System.Linq;

    using Android.OS;
    using Android.Support.V4.App;

    using net.opgenorth.yegvote.droid.Model;

    internal class MainActivityStateFragment : Fragment
    {
        public static readonly string Tag = typeof(MainActivityStateFragment).FullName.Replace(".", "_");

        public bool HasBoundService { get; set; }

        public bool HasData { get { return Wards != null && Wards.Any(); } }

        public bool IsAlarmed { get; set; }
        public bool IsDisplayingHud { get; set; }
        public int SelectedGroup { get; set; }
        public IEnumerable<Ward> Wards { get; set; }

        public static MainActivityStateFragment NewInstance()
        {
            var frag = new MainActivityStateFragment
                       {
                           Wards = new Ward[0],
                           HasBoundService = false,
                           IsAlarmed = false,
                           IsDisplayingHud = false,
                           SelectedGroup = -1
                       };
            return frag;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }
    }
}
