// See https://aka.ms/new-console-template for more information
//https://feeds.simplecast.com/bohNKLUL?_ga=2.236344321.1436236783.1735443888-528807958.1735443888

using System.CommandLine;
using Pyp.Commands;

PodcastCommand rootCommand = new PodcastCommand();

return await rootCommand.InvokeAsync(args);