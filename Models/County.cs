using System;

namespace ElectionComparitor.Models
{
    public class TexasCounty
    {
        public string Name { get; set; }
    }

    public enum Races
    {
        Presidential2016,
        Presidential2020,
        Senate2018
    }
}
