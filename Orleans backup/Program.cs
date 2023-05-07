using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace OrleansMath
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansMath";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IMathGrain).Assembly).WithReferences())
                .AddMemoryGrainStorageAsDefault();

            using (var host = builder.Build())
            {
                await host.StartAsync();

                var mathGrain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IMathGrain>(Guid.NewGuid());
                var result = await mathGrain.CalculateExpression(2, 3, 4, 5);

                Console.WriteLine($"Result: {result}");

                await host.StopAsync();
            }
        }
    }

    public interface IMathGrain : IGrainWithGuidKey
    {
        Task<double> CalculateExpression(double a, double b, double z, double y);
    }

    public class MathGrain : Grain, IMathGrain
    {
        public async Task<double> CalculateExpression(double a, double b, double z, double y)
        {
            var addGrain1 = GrainFactory.GetGrain<IAddGrain>(Guid.NewGuid());
            var addGrain2 = GrainFactory.GetGrain<IAddGrain>(Guid.NewGuid());
            var multiplyGrain = GrainFactory.GetGrain<IMultiplyGrain>(Guid.NewGuid());

            var task1 = addGrain1.Add(a, b);
            var task2 = addGrain2.Add(z, y);

            await Task.WhenAll(task1, task2);

            var addResult1 = await task1;
            var addResult2 = await task2;

            var multiplyResult = await multiplyGrain.Multiply(addResult1, addResult2);

            return multiplyResult;
        }
    }

    public interface IAddGrain : IGrainWithGuidKey
    {
        Task<double> Add(double a, double b);
    }

    public class AddGrain : Grain, IAddGrain
    {
        public Task<double> Add(double a, double b)
        {
            return Task.FromResult(a + b);
        }
    }

    public interface IMultiplyGrain : IGrainWithGuidKey
    {
        Task<double> Multiply(double a, double b);
    }

    public class MultiplyGrain : Grain, IMultiplyGrain
    {
        public Task<double> Multiply(double a, double b)
        {
            return Task.FromResult(a * b);
        }
    }
}