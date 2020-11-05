using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ElectionComparitor.Models.Data2016
{
    public class VoteResponse
    {
        [JsonPropertyName("counties")]
        public List<County> Counties { get; set; }
    }

    public class County
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("race")]
        public Race Race { get; set; }
    }

    public class Race
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("fname")]
        public string FirstName { get; set; }

        [JsonPropertyName("lname")]
        public string LastName { get; set; }

        [JsonPropertyName("party")]
        public string Party { get; set; }

        [JsonPropertyName("votes")]
        public int Votes { get; set; }
    }
}
