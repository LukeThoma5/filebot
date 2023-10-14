// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using CommandLine;

using System.Data.SQLite;
using Dapper;

Console.WriteLine("Hello, World!");

var cliOptions = CliParser.Parse(args);

Console.WriteLine($"Database path: {cliOptions.DbPath.FullName}");
Console.WriteLine($"Media path: {cliOptions.MediaPath.FullName}");

// open the db with dapper
using var db = new SQLiteConnection($"Data Source={cliOptions.DbPath.FullName}");
db.Open();

// MovieBot.Run(db, cliOptions.MediaPath);
SeasonBot.Run(cliOptions.MediaPath);