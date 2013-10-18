using System;
using System.Collections.Generic;
using System.Linq;

using Android.Content;
using Android.Views;
using Android.Widget;

using YegVote2013.Droid.Model;

using Object = Java.Lang.Object;

namespace YegVote2013.Droid
{
    public class ElectionResultAdapter : BaseExpandableListAdapter
    {
        private readonly Context _context;
        private readonly List<Ward> _wards;

        public ElectionResultAdapter(Context context, IEnumerable<Ward> rows)
        {
            _context = context;
            _wards = rows.ToList();
        }

        public override int GroupCount { get { return _wards.Count; } }

        public override bool HasStableIds { get { return false; } }
        private LayoutInflater Inflater { get { return (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService); } }

        public override Object GetChild(int groupPosition, int childPosition)
        {
            var ward = _wards[groupPosition];
            var candidate = ward.Candidates[childPosition];
            var wrapper = new Wrapper<Candidate>(candidate);
            return wrapper;
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var ward = _wards[groupPosition];
            var candidate = ward.Candidates[childPosition];

            var view = convertView ?? Inflater.Inflate(Resource.Layout.electionresult_row, null);
            var candidateName = view.FindViewById<TextView>(Resource.Id.candidateNameTextView);
            var votes = view.FindViewById<TextView>(Resource.Id.votesTextView);

            candidateName.Text = candidate.Name;
            votes.Text = string.Format("{0} votes ({1}%).", candidate.VotesReceived, candidate.Percentage);
            return view;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return _wards[groupPosition].Candidates.Count;
        }

        public override Object GetGroup(int groupPosition)
        {
            return new Wrapper<Ward>(_wards[groupPosition]);
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var ward = _wards[groupPosition];

            var view = convertView ?? Inflater.Inflate(Resource.Layout.electionresult_header, null);

            var title = view.FindViewById<TextView>(Resource.Id.wardHeaderTextView);
            title.Text = FormatWardTitle(ward);

            var lastUpdate = view.FindViewById<TextView>(Resource.Id.mostRecentUpdateTextView);
            lastUpdate.Text = "Last Updated:" + ward.LastUpdatedAt.ToString("h:mm tt");

            var pollsReporting = view.FindViewById<TextView>(Resource.Id.pollsReportingTextView);
            pollsReporting.Text = string.Format("{0} polls out of {1} reporting.", ward.Reporting, ward.OutOf);

            var numberOfVotes = view.FindViewById<TextView>(Resource.Id.numberOfVotesTextView);
            numberOfVotes.Text = string.Format("{0} votes cast", ward.VotesCast);

            return view;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return false;
        }

        private string FormatWardTitle(Ward ward)
        {
            if ("Councillor".Equals(ward.Contest, StringComparison.OrdinalIgnoreCase))
            {
                return ward.Contest + ", " + ward.Name;
            }
            else if ("Mayor".Equals(ward.Contest, StringComparison.OrdinalIgnoreCase))
            {
                return ward.Contest;
            }
            else if (ward.Contest.StartsWith("Public", StringComparison.OrdinalIgnoreCase))
            {
				return string.Format("{0} - Public School Trustee", ward.Name);
            }
            else if (ward.Contest.StartsWith("Catholic", StringComparison.OrdinalIgnoreCase))
            {
				return string.Format("{0} - Catholic School Trustee", ward.Name);
            }
            return ward.Name + " - " + ward.Contest;
        }
    }
}
