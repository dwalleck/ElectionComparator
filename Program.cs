﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using ElectionComparitor.Models.Data2020;
using ElectionComparitor.Models.Data2016;
using System.Linq;
using ElectionComparitor.Models;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace ElectionComparitor
{
    class Program
    {
        private static decimal PercentChange(int originalValue, int newValue)
        {
            decimal change = ((newValue - originalValue) / (decimal) originalValue) * 100;
            return Math.Round(change, 2);
        }

        private static string DetermineWinner(CountyResult county, Elections election)
        {
            if (county.Elections[election].Results["D"] > county.Elections[election].Results["R"])
            {
                return "D";
            }
            return "R";
        }

        public record DifferenceAttributes
        {
            public string Operator { get; init; }
            public string Color { get; init; }
        }

        public record PartyElectionData
        {
            public int Votes { get; set; }
            public PercentChangez PercentChange { get; set; }
        }

        public record PercentageColors
        {
            public string Positive { get; set; }
            public string Negative { get; set; }
        }

        public class PercentChangez
        {
            public PercentageColors _textColors;
            public decimal _value;

            public PercentChangez(PercentageColors textColors, decimal value)
            {
                _textColors = textColors;
                _value = value;
            }

            public string TextColor => _value > 0 ? _textColors.Positive : _textColors.Negative;

            private string Operator => _value > 0 ? "+" : "";

            public string GetFormatttedPercentage => $"[{TextColor}]{Operator}{_value}%[/]";
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
            public List<ElectionComparitor.Models.Data2020.County> rawData2020 { get; init; }
            public VoteResponse rawData2016 { get; init; }
        }

        private static string CountyWasFlipped(string winner2016, string winner2020) => winner2016 != winner2020 ? "Yes" : "No";

        private static async Task<RawElectionResults> GetElectionDataFromWeb(string state)
        {
            var client = new HttpClient();
            var data2020 = new List<ElectionComparitor.Models.Data2020.County>();
            VoteResponse data2016 = null;

            try
            {
                data2020 = await client.GetFromJsonAsync<List<ElectionComparitor.Models.Data2020.County>>(
                    $"https://politics-elex-results.data.api.cnn.io/results/view/2020-county-races-PG-{state}.json"
                ).ConfigureAwait(false);
                data2016 = await client.GetFromJsonAsync<VoteResponse>(
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

        private static CountyRowData CreateCountyRow(CountyResult county)
        {
            var winner2016 = DetermineWinner(county, Elections.Presidential2016);
            var winner2020 = DetermineWinner(county, Elections.Presidential2020);
            var countyNameColor = winner2020 == "R" ? "red1" : "dodgerblue1";
            PercentageColors democratColors = new PercentageColors
            {
                Positive = "dodgerblue1",
                Negative = "dodgerblue2"
            };

            PercentageColors republicanColors = new PercentageColors
            {
                Positive = "red1",
                Negative = "red3"
            };

            var rPercentChange = PercentChange(county.Elections[Elections.Presidential2016].Results["R"], county.Elections[Elections.Presidential2020].Results["R"]);
            var dPercentChange = PercentChange(county.Elections[Elections.Presidential2016].Results["D"], county.Elections[Elections.Presidential2020].Results["D"]);

            var data = new CountyRowData
            {
                CountyName = county.Name,
                CountyTextColor = countyNameColor,
                PercentReporting = county.PercentReporting,
                RepublicanData = new PartyElectionData
                {
                    Votes = county.Elections[Elections.Presidential2020].Results["R"],
                    PercentChange = new PercentChangez(republicanColors, rPercentChange)
                    
                },
                DemocratData = new PartyElectionData
                {
                    Votes = county.Elections[Elections.Presidential2020].Results["D"],
                    PercentChange = new PercentChangez(democratColors, dPercentChange)
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
                    var county = new CountyResult
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

                foreach (var c in counties)
                {
                    //var rDiff = PercentChange(
                    //    c.Elections[Elections.Presidential2016].Results["R"],
                    //    c.Elections[Elections.Presidential2020].Results["R"]
                    //);
                    //var rPercentAttrs = rDiff > 0
                    //    ? new DifferenceAttributes { Operator = "+", Color = "red1"}
                    //    : new DifferenceAttributes { Operator = "", Color = "red3"};
                    //var dDiff = PercentChange(
                    //    c.Elections[Elections.Presidential2016].Results["D"],
                    //    c.Elections[Elections.Presidential2020].Results["D"]
                    //);
                    //var dPercentAttrs = dDiff > 0
                    //    ? new DifferenceAttributes { Operator = "+", Color = "dodgerblue1"}
                    //    : new DifferenceAttributes { Operator = "", Color = "dodgerblue2"};
                    //var winner2016 = DetermineWinner(c, Elections.Presidential2016);
                    //var winner2020 = DetermineWinner(c, Elections.Presidential2020);
                    //var countyNameColor = winner2020 == "R" ? "red1" : "dodgerblue1";

                    //grid.AddRow(
                    //    $"[{countyNameColor}]{c.Name}[/]",
                    //    $"[white]{c.PercentReporting}[/]",
                    //    $"[red1]{c.Elections[Elections.Presidential2020].Results["R"]}[/]",
                    //    $"[{rPercentAttrs.Color}]{rPercentAttrs.Operator}{rDiff}%[/]",
                    //    $"[blue]{c.Elections[Elections.Presidential2020].Results["D"]}[/]",
                    //    $"[{dPercentAttrs.Color}]{dPercentAttrs.Operator}{dDiff}%[/]",
                    //    $"[white]{CountyWasFlipped(winner2016, winner2020)}[/]"
                    //);

                    var row = CreateCountyRow(c);
                    grid.AddRow(
                        $"[{row.CountyTextColor}]{row.CountyName}[/]",
                        $"[white]{row.PercentReporting}[/]",
                        $"[red1]{row.RepublicanData.Votes}[/]",
                        $"{row.RepublicanData.PercentChange.GetFormatttedPercentage}",
                        $"[dodgerblue1]{row.DemocratData.Votes}[/]",
                        $"{row.DemocratData.PercentChange.GetFormatttedPercentage}",
                        $"[white]{row.WasCountyFlipped}[/]"
                    //$"[blue]{c.Elections[Elections.Presidential2020].Results["D"]}[/]",
                    //$"[{dPercentAttrs.Color}]{dPercentAttrs.Operator}{dDiff}%[/]",
                    //$"[white]{CountyWasFlipped(winner2016, winner2020)}[/]"
                    );
                }
                AnsiConsole.Render(grid);
            });

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
