using System;
using System.Collections.Generic;

namespace ElectionComparitor.Models
{
    public class CountyResult
    {
        public string Name { get; set; }

        public Dictionary<Elections, Election> Elections { get; set; } = new Dictionary<Elections, Election>();
    }

    public enum Elections
    {
        Presidential2016,
        Presidential2020,
        Senate2018
    }

    public enum Parties
    {
        Republican,
        Democrat
    }

    public class Election
    {
        public Dictionary<string, int> Results { get; set; } = new Dictionary<string, int>();
    }
}
