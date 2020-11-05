using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ElectionComparitor.Models.Data2020
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
