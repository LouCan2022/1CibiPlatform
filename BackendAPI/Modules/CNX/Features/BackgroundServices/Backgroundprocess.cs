﻿namespace CNX.Features.BackgroundServices;

public class Backgroundprocess : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
