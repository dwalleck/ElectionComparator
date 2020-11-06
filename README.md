ElectionComparator (US)
=======================

![Sample Output](/Images/sample-output.png)

This is a simple command line tool that given a two character state abbreviation,
returns the presidential election results for each party in that state by county.
It also includes the percent changed for votes for each party and if the winner
of that county flipped from the 2016 presidential election.

Example Usage:
```
dotnet run --state WA
```

I created this side project to provide easy to consume data on how the historic
voter turnout in 2020 scaled for each party. If errors exist in the data shown
by this tool, they are non-intentional and likely the result of too much
caffeine and too little sleep. If you run into any errors or inconsistencies,
please let me know so that I can address them.

This tool relies on data that is available on the web site of a popular news
network and will only continue to function as long as that data stays public.
If that data source is cut off, this project will be deprecated.
