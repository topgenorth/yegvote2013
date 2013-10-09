using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace YegVote2013.Droid.Model
{
    public class ElectionResultsParser
    {
        public ElectionResult CreateElectionResult(XElement result)
        {
            var r = new ElectionResult();

            // ReSharper disable PossibleNullReferenceException
            r.Id = Int32.Parse(result.Attribute("_id").Value);
            r.UUID = new Guid(result.Attribute("_uuid").Value);
            r.Address = result.Attribute("_address").Value;
            r.ReportedAt = ParseDate(result.Element("reported_at").Value);
            r.RaceId = Int32.Parse(result.Element("race_id").Value);
            r.Contest = result.Element("contest").Value;
            r.WardName = result.Element("ward_name").Value;
            r.Acclaimed = result.Element("acclaimed").Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
            r.Reporting = Int32.Parse(result.Element("reporting").Value);
            r.OutOf = Int32.Parse(result.Element("out_of").Value);
            r.VotesCast = Int32.Parse(result.Element("votes_cast").Value);
            r.Race = Int32.Parse(result.Element("race").Value);
            r.CandidateName = result.Element("candidate_name").Value;
            r.VotesReceived = Int32.Parse(result.Element("votes_received").Value);
            r.Percentage = float.Parse(result.Element("percentage").Value);
            // ReSharper restore PossibleNullReferenceException
            return r;
        }

        public DateTime ParseDate(string date)
        {
            //            "Friday, September 27, 9:29 am"
            var result = DateTime.ParseExact(date, "dddd, MMMM d, H:mm tt", CultureInfo.InvariantCulture);
            return result;
        }

        public IEnumerable<ElectionResult> ParseElectionResultFromFile(string pathToFile)
        {
            if (!File.Exists(pathToFile))
            {
                throw new FileNotFoundException("The election results file does not exist.", pathToFile);
            }
            using (var xmlReader = XmlReader.Create(pathToFile))
            {
                return ParseResults(xmlReader);
            }
        }

        public IEnumerable<ElectionResult> ParseElectionResults(string resultsXml)
        {
            if (String.IsNullOrWhiteSpace(resultsXml))
            {
                return new ElectionResult[0];
            }

            using (var xmlReader = XmlReader.Create(new StringReader(resultsXml)))
            {
                return ParseResults(xmlReader);
            }
        }

        private IEnumerable<ElectionResult> ParseResults(XmlReader xmlReader)
        {
            var xdoc = XDocument.Load(xmlReader);
            var lvl1 = xdoc.Descendants("row");
            var lvl2 = lvl1.Descendants("row");
            var rows = lvl2.Select(CreateElectionResult).ToList();
            return rows;
        }
    }
}
