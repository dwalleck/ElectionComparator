using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElectionComparator.ElectionData.Format2020
{
    public class Candidate
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("candidatePartyCode")]
        public string Party { get; set; }

        [JsonPropertyName("voteNum")]
        public int Votes { get; set; }
    }

    public class County
    {
        [JsonPropertyName("countyName")]
        public string Name { get; set; }

        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        [JsonPropertyName("percentReporting")]
        public int PercentReporting { get; set; }
    }
}
