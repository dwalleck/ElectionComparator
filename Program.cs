using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using static ElectionComparator.PartyColors;
using ElectionComparator.ElectionData.Format2016;
using ElectionComparator.ElectionData.Format2020;

namespace ElectionComparitor
{
    class Program
    {
        private static decimal CalculatePercentChange(int originalValue, int newValue)
        {
            decimal change = ((newValue - originalValue) / (decimal) originalValue) * 100;
            return Math.Round(change, 2);
        }

        private static string DetermineWinner(CountyResults county, Elections election)
        {
            if (county.Elections[election].Results["D"] > county.Elections[election].Results["R"])
            {
                return "D";
            }
            return "R";
        }

        public record PartyElectionData
        {
            public int Votes { get; set; }
            public PercentChangez PercentChange { get; set; }
        }

        public class PercentChangez
        {
            public PartyColorTheme _textColors;
            public decimal _value;

            public PercentChangez(PartyColorTheme textColors, decimal value)
            {
                _textColors = textColors;
                _value = value;
            }

            public string TextColor => _value > 0 ? _textColors.Positive : _textColors.Negative;
            private string Operator => _value > 0 ? "+" : "";
            public string FormatttedPercentage => $"[{TextColor}]{Operator}{_value}%[/]";
        }

        public record CountyRowData
        {
            public string CountyName { get; set; }
            public string CountyTextColor { get; set; }
            public int PercentReporting { get; set; }
            public PartyElectionData RepublicanData { get; set; }
            public PartyElectionData DemocratData { get; set; }
            public string WasCountyFlipped { get; set; }
        }

        public record RawElectionResults
        {
            public List<ElectionComparator.ElectionData.Format2020.County> rawData2020 { get; init; }
            public ElectionResults rawData2016 { get; init; }
        }

        private static string CountyWasFlipped(string winner2016, string winner2020) => winner2016 != winner2020 ? "Yes" : "No";

        private static async Task<RawElectionResults> GetElectionDataFromWeb(string state)
        {
            var client = new HttpClient();
            try
            {
                var data2020 = await client.GetFromJsonAsync<List<ElectionComparator.ElectionData.Format2020.County>>(
                    $"https://politics-elex-results.data.api.cnn.io/results/view/2020-county-races-PG-{state}.json"
                ).ConfigureAwait(false);
                var data2016 = await client.GetFromJsonAsync<ElectionResults>(
                    $"https://data.cnn.com/ELECTION/2016/{state}/county/P_county.json"
                ).ConfigureAwait(false);
                return new RawElectionResults { rawData2016 = data2016, rawData2020 = data2020 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve election data: {ex.Message}");
                throw;
            }
        }

        private static CountyRowData CreateCountyRow(CountyResults county)
        {
            var winner2016 = DetermineWinner(county, Elections.Presidential2016);
            var winner2020 = DetermineWinner(county, Elections.Presidential2020);
            var countyNameColor = winner2020 == "R" ? RepublicanColors.Neutral : DemocratColors.Neutral;

            var rPercentChange = CalculatePercentChange(
                county.Elections[Elections.Presidential2016].Results["R"],
                county.Elections[Elections.Presidential2020].Results["R"]
            );
            var dPercentChange = CalculatePercentChange(
                county.Elections[Elections.Presidential2016].Results["D"],
                county.Elections[Elections.Presidential2020].Results["D"]
            );

            var data = new CountyRowData
            {
                CountyName = county.Name,
                CountyTextColor = countyNameColor,
                PercentReporting = county.PercentReporting,
                RepublicanData = new PartyElectionData
                {
                    Votes = county.Elections[Elections.Presidential2020].Results["R"],
                    PercentChange = new PercentChangez(RepublicanColors, rPercentChange)
                },
                DemocratData = new PartyElectionData
                {
                    Votes = county.Elections[Elections.Presidential2020].Results["D"],
                    PercentChange = new PercentChangez(DemocratColors, dPercentChange)
                },
                WasCountyFlipped = CountyWasFlipped(winner2016, winner2020)
            };

            return data;
        }

        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--state",
                    getDefaultValue: () => "TX",
                    description: "The two letter state code"),
            };

            rootCommand.Handler = CommandHandler.Create<string>(async (state) =>
            {
                var rawResults = await GetElectionDataFromWeb(state.ToUpper());
                var counties = rawResults.rawData2020.ConvertAll(c => {
                    var county = new CountyResults
                    {
                        Name = c.Name,
                        PercentReporting = c.PercentReporting
                    };

                    county.Elections[Elections.Presidential2020] = new Election();
                    foreach (var candidate in c.Candidates)
                    {
                        county.Elections[Elections.Presidential2020].Results[candidate.Party] = candidate.Votes;
                    }
                    return county;
                });

                foreach (var r in rawResults.rawData2016.Counties)
                {
                    var county = counties.Where(c => c.Name == r.Name).FirstOrDefault();
                    county.Elections[Elections.Presidential2016] = new Election();
                    foreach(var candidate in r.Race.Candidates.Where(cand => cand.Party == "D" || cand.Party == "R"))
                    {
                        county.Elections[Elections.Presidential2016].Results[candidate.Party] = candidate.Votes;
                    }
                }

                var grid = new Table { Border = TableBorder.Rounded };
                grid.AddColumn(new TableColumn("[white]County[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]% Reporting[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]Rep. 2020 votes[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]% Change from 2016[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]Dem. 2020 votes[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]% Change from 2016[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[white]Flipped?[/]") { NoWrap = true });

                foreach (var county in counties)
                {
                    var row = CreateCountyRow(county);
                    grid.AddRow(
                        $"[{row.CountyTextColor}]{row.CountyName}[/]",
                        $"[white]{row.PercentReporting}[/]",
                        $"[{RepublicanColors.Neutral}]{row.RepublicanData.Votes}[/]",
                        $"{row.RepublicanData.PercentChange.FormatttedPercentage}",
                        $"[{DemocratColors.Neutral}]{row.DemocratData.Votes}[/]",
                        $"{row.DemocratData.PercentChange.FormatttedPercentage}",
                        $"[white]{row.WasCountyFlipped}[/]"
                    );
                }
                AnsiConsole.Render(grid);
            });

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
