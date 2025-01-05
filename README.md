# Musikchartograf

This little tool can be used to load played tracks of a user from Last.fm and
to calculate charts from them.

Right now it can load data of a given year, calculate charts of a week, and
calculate yearly charts based on the weekly charts of a year.

## How to run

1. Head over to Last.fm and [get yourself an API key](https://www.last.fm/api).
2. Download the [.NET SDK](https://dotnet.microsoft.com/en-us/download) for your
   OS. You need the version specified in the `global.json` file.
3. Open up a terminal and navigate to the code.
4. To build the application run
   `dotnet publish ./src/Musikchartograf.Console --self-contained --output ./mcf`.
5. Switch to the output directory using `cd ./mcf`.
6. Run
   `./Musikchartograf.Console load-year -d ./data.db -u USERNAME --year YEAR` to
   load data of a year. Replace `USERNAME` with the user for which you want to
   load data and `YEAR` with the year you want to load. This will take some
   time.
7. Now you can use the loaded data to calculate some charts via the `yearly` or
   `weekly` commands. To calculate the yearly charts you can use
   `./Musikchartograf.Console yearly -d ./data.db -u USERNAME --year YEAR`
