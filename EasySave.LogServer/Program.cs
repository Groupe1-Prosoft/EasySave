using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Folder to save logs. It will be shared with Docker.
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "CentralizedLogs");
Directory.CreateDirectory(logDirectory);

// Lock to stop two computers from writing at the same time.
var fileLock = new object();

// URL endpoint to receive logs from EasySave clients.
app.MapPost("/logs", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var jsonContent = await reader.ReadToEndAsync();

    // Stop if the log is empty.
    if (string.IsNullOrWhiteSpace(jsonContent))
    {
        return Results.BadRequest("Empty log ignored.");
    }

    // Create a daily file name like 2026-02-25.json.
    var fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
    var filePath = Path.Combine(logDirectory, fileName);

    // Write the log safely to the file.
    lock (fileLock)
    {
        // Add the log at the end of the file with a new line.
        File.AppendAllText(filePath, jsonContent + Environment.NewLine);
    }

    Console.WriteLine($"[LOG RECEIVED] Added to {fileName}");
    return Results.Ok();
});

// Default port for Docker.
app.Urls.Add("http://*:8080");

app.Run();