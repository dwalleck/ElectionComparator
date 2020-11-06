ElectionComparator (US)
=======================

![Sample Output](/Images/sample-output.png)

This is a simple command-line tool that, given a two-character state
abbreviation, returns the number of votes cast for each party in that
state by county. It also includes the percentage votes increased or decreased
for each party from the 2016 presidential election.

Example Usage:

```powershell
dotnet run --state WA
```

I created this side project to provide easily consumable data on how the historic
voter turnout in 2020 scaled for each party. If errors exist in the data shown
by this tool, they are non-intentional and likely the result of too much
caffeine and too little sleep. If you run into any errors or inconsistencies,
please let me know so that I can address them.

This tool relies on data that is available on the web site of a popular news
network and will only continue to function as long as that data stays public.
If that data source is cut off, this project will be deprecated.

Near future additions

- Output to CSV
- Sorting of output by column
