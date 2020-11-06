using System;
using System.Collections.Generic;

namespace ElectionComparitor
{
    public class CountyResults
    {
        public string Name { get; set; }

        public int PercentReporting { get; set; }

        public Dictionary<Elections, Election> Elections { get; set; } = new Dictionary<Elections, Election>();
    }

    public enum Elections
    {
        Presidential2016,
        Presidential2020
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
