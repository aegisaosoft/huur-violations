/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */
using HuurApi.Models;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace HuurVioaltionsAPI
{ 
    public class Loader
    {
        List<IHuurViloationCaller> callers = new List<IHuurViloationCaller>();

        public Loader() 
        {
            LoadCallers();
        }

        private void LoadCallers()
        {
            try
            {
                var loaderDir = Path.Combine(AppContext.BaseDirectory, "Loaders");
                if (!Directory.Exists(loaderDir))
                {
                    Directory.CreateDirectory(loaderDir);
                    return;
                }

                callers.Clear();

                foreach (var dll in Directory.EnumerateFiles(loaderDir, "*.dll"))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(dll);
                        var types = asm.GetTypes()
                            .Where(t => typeof(IHuurViloationCaller).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
                        foreach (var t in types)
                        {
                            try
                            {
                                if (Activator.CreateInstance(t) is IHuurViloationCaller instance)
                                {
                                    callers.Add(instance);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public async Task LoadAsync(List<(string Plate, string State)> allQueries)
        {
            if (callers.Count == 0)
            {
                Console.WriteLine("No violation callers found.");
                return;
            }

            // Get configuration for thread count
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var maxThreads = configuration["MaxThreads"] != null ? int.Parse(configuration["MaxThreads"]) : 1;
            Console.WriteLine($"Providers discovered: {callers.Count}");
            Console.WriteLine($"Processing with {maxThreads} thread(s)");

            var aggregated = new List<ViolationResult>();
            var huurApiBase = Environment.GetEnvironmentVariable("HUUR_API_BASE");

            if (null == huurApiBase) throw new Exception("spicify Huur API url");

            using var huurApi = new HuurApiClient(huurApiBase);

            // Create semaphore to limit concurrent threads
            using var semaphore = new SemaphoreSlim(maxThreads, maxThreads);

            // Process all queries in parallel with limited concurrency
            var tasks = allQueries.Select(async query =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await ProcessQueryAsync(query.Plate, query.State, huurApi, aggregated);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            Console.WriteLine();
            Console.WriteLine($"Total violations submitted: {aggregated.Count}");
            foreach (var v in aggregated)
            {
                Console.WriteLine($"- {v.CitationNumber} [{v.State}] {v.LicensePlate}: {v.Description} ${v.Amount:F2} on {v.IssueDate:yyyy-MM-dd} [{v.Status}]");
            }
        }

        private async Task ProcessQueryAsync(string plate, string state, HuurApiClient huurApi, List<ViolationResult> aggregated)
        {
            Console.WriteLine($"Searching plate={plate}, state={state}...");
            
            foreach (var c in callers)
            {
                try
                {
                    IHuurAPIFinder? caller = c as IHuurAPIFinder;
                    if (caller == null) continue;

                    var found = await caller.Find(plate, state);
                    if (found != null && found.Count > 0)
                    {
                        int created = 0;
                        foreach (var v in found)
                        {
                            var ok = await huurApi.CreateViolationAsync(v);
                        }
                        Console.WriteLine($"  + {created}/{found.Count} created from {caller.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  x Error from {c.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
