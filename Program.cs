using System;
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

namespace ElectionComparitor
{
    class Program
    {
        static int Main(string[] args)
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
                var client = new HttpClient();
                var resp2020 = await client.GetFromJsonAsync<List<ElectionComparitor.Models.Data2020.County>>(
                    $"https://politics-elex-results.data.api.cnn.io/results/view/2020-county-races-PG-{state.ToUpper()}.json"
                );
                var counties = resp2020.ConvertAll(c => {
                    var county = new CountyResult
                    {
                        Name = c.Name
                    };

                    county.Elections[Elections.Presidential2020] = new Election();
                    foreach (var candidate in c.Candidates)
                    {
                        county.Elections[Elections.Presidential2020].Results[candidate.Party] = candidate.Votes;
                    }
                    return county;
                });

                var resp2016 = await client.GetFromJsonAsync<VoteResponse>(
                    $"https://data.cnn.com/ELECTION/2016/{state.ToUpper()}/county/P_county.json"
                );
                foreach (var r in resp2016.Counties)
                {
                    var county = counties.Where(c => c.Name == r.Name).FirstOrDefault();
                    county.Elections[Elections.Presidential2016] = new Election();
                    foreach(var candidate in r.Race.Candidates.Where(cand => cand.Party == "D" || cand.Party == "R"))
                    {
                        county.Elections[Elections.Presidential2016].Results[candidate.Party] = candidate.Votes;
                    }
                }

                var grid = new Table { Border = TableBorder.Rounded };
                grid.AddColumn(new TableColumn("[grey]County[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[red]Rep. 2020 votes[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[red]Rep. 2016 delta[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[blue]Dem. 2020 votes[/]") { NoWrap = true });
                grid.AddColumn(new TableColumn("[blue]Dem. 2016 delta[/]") { NoWrap = true });

                foreach (var c in counties)
                {
                    var rDiff = c.Elections[Elections.Presidential2020].Results["R"] - c.Elections[Elections.Presidential2016].Results["R"];
                    var rDiffOpp = rDiff > 0 ? "+" : "";
                    var dDiff = c.Elections[Elections.Presidential2020].Results["D"] - c.Elections[Elections.Presidential2016].Results["D"];
                    var dDiffOpp = dDiff > 0 ? "+" : "";
                    grid.AddRow(
                        $"[grey]{c.Name}[/]",
                        $"[red]{c.Elections[Elections.Presidential2020].Results["R"]}[/]",
                        $"[red]{rDiffOpp}{rDiff}[/]",
                        $"[blue]{c.Elections[Elections.Presidential2020].Results["D"]}[/]",
                        $"[blue]{dDiffOpp}{dDiff}[/]"
                    );
                }
                AnsiConsole.Render(grid);
            });

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
