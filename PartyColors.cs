using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionComparator
{
    public class PartyColors
    {
        public record PartyColorTheme
        {
            public string Positive { get; set; }
            public string Negative { get; set; }
            public string Neutral { get; set; }
        }

        public static PartyColorTheme DemocratColors = new PartyColorTheme
        {
            Positive = "dodgerblue1",
            Negative = "dodgerblue2",
            Neutral = "dodgerblue1"
        };

        public static PartyColorTheme RepublicanColors = new PartyColorTheme
        {
            Positive = "red1",
            Negative = "red3",
            Neutral = "red1"
        };
    }
}
