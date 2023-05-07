using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace OrleansMath
{
    class Program
    {
        static double[,] y1;
        static double[,] y1t;
        static double[,] y2;
        static double[,] y2t;
        //step 3
        static double[,] A2;
        static double[,] B2;
        static double[,] C2;
        static double[,] Y3;
        static double K1 = 0.0000001;
        //find X
        static double[,] firstGap;
        static double[,] secondGap;
        static double[,] X;
        static async Task Main(string[] args)
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansMath";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IInitializeGrain).Assembly).WithReferences())
                .AddMemoryGrainStorageAsDefault();
            using (var host = builder.Build())
            {
                await host.StartAsync();
                int choose;
                int n = 3;
                do
                {
                    Console.WriteLine("Заповнити масив: \n" +
                        "0 - числа для перевірки роботи програми\n" +
                        "1 - випадкові числа");
                    choose = int.Parse(Console.ReadLine());
                }
                while (choose < 0 || choose > 1);
                if (choose != 0)
                {
                    do
                    {
                        Console.WriteLine("Enter matrix size: ");
                        n = int.Parse(Console.ReadLine());
                    }
                    while (n < 2);
                }
                var mathGrain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IInitializeGrain>(Guid.NewGuid());
                var mathGrain2 = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IInitializeGrain>(Guid.NewGuid());
                var mathGrain3 = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IInitializeGrain>(Guid.NewGuid());
                await mathGrain.Set_y1(choose, n);
                await mathGrain2.Set_y2(choose, n);
                await mathGrain3.Set_Y3(choose, n);
                y1 = await mathGrain.Get_y1();
                y1t = await mathGrain.Get_y1t();
                y2 = await mathGrain2.Get_y2();
                y2t = await mathGrain2.Get_y2t();
                Y3 = await mathGrain3.Get_Y3();
                var grainKey = "fdfdsfsd-fdfs-fdsf";
                var mathGrain4 = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IDoCalculationsGrain>("my-key");
                var mathGrain5 = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IDoCalculationsGrain>(grainKey);
                await mathGrain4.Set_values(y1,y1t,y2,y2t,Y3);
                await mathGrain5.Set_values(y1,y1t,y2,y2t,Y3);
                firstGap = await mathGrain4.CalculateFirstGap();
                secondGap = await mathGrain5.CalculateSecondGap();
                X = MatrixFunctions.MatrixMultiply(firstGap, secondGap);
                MatrixFunctions.PrintMatrix(X);
                await host.StopAsync();
            }
        }
    }
    public interface IInitializeGrain : IGrainWithGuidKey
    {
        public Task Set_y1(int choose, int n);
        public Task Set_y2(int choose, int n);
        public Task Set_Y3(int choose, int n);
        Task<double[,]> Get_y1();
        Task<double[,]> Get_y2();
        Task<double[,]> Get_y1t();
        Task<double[,]> Get_Y3();
        Task<double[,]> Get_y2t();
    }
    public class InitializeGrain : Grain, IInitializeGrain
    {
        private int n;
        private double[,] A;
        private double[,] b;
        private double[,] y1;
        private double[,] y1t;
        //step 1
        public Task Set_y1(int choose, int n)
        {
            this.n = n;
            SetA(choose);
            Initialize_b();
            y1 = MatrixFunctions.MatrixMultiply(A, b);
            y1t = MatrixFunctions.TMatrix(y1);
            return Task.CompletedTask;
        }
        private Task SetA(int choose)
        {
            if(choose == 0)
            A = new double[,] { { 3, 4, 2 }, { 1, 2, 2 }, { 3, 1, 12 } };
            else
            {
                A = MatrixFunctions.FillArray(n);
            }
            return Task.CompletedTask;
        }
        private Task Initialize_b()
        {
            b = MatrixFunctions.Initialize_b(n);
            return Task.CompletedTask;
        }
        public Task<double[,]> Get_y1()
        {
            return Task.FromResult(y1);
        }
        public Task<double[,]> Get_y1t()
        {
            return Task.FromResult(y1t);
        }
        //step 2
        private double[,] A1;
        private double[,] b1;
        private double[,] c1;
        private double[,] y2;
        private double[,] y2t;
        public Task Set_y2(int choose, int n)
        {
            this.n = n;
            if (choose == 0)
            {
                A1 = new double[,] { { 3, 7, 2 }, { 1, 5, 5 }, { 1, 2, 2 } };
                b1 = new double[,] { { 3, 0, 0 }, { 5, 0, 0 }, { 2, 0, 0 } };
                c1 = new double[,] { { 4, 0, 0 }, { 4, 0, 0 }, { 2, 0, 0 } };
            }
            else
            {
                A1 = MatrixFunctions.FillArray(n);
                b1 = MatrixFunctions.FillArrayColumn(n);
                c1 = MatrixFunctions.FillArrayColumn(n);
            }
            y2 = MatrixFunctions.MatrixMultiply(A1, (MatrixFunctions.MatrixSum(MatrixFunctions.MatrixMultiplyOnNumber(b1, 2), MatrixFunctions.MatrixMultiplyOnNumber(c1, 3))));
            y2t = MatrixFunctions.TMatrix(y2);
            return Task.CompletedTask;
        }
        public Task<double[,]> Get_y2()
        {
            return Task.FromResult(y2);
        }
        public Task<double[,]> Get_y2t()
        {
            return Task.FromResult(y2t);
        }
        //step 3
        private double[,] A2;
        private double[,] B2;
        private double[,] C2;
        private double[,] Y3;
        public Task Set_Y3(int choose, int n)
        {
            this.n = n;
            C2 = MatrixFunctions.Initialize_C2(n);
            if (choose == 0)
            {
                A2 = new double[,] { { 3, 5, 5 }, { 2, 2, 6 }, { 1, 3, 4 } };
                B2 = new double[,] { { 3, 3, 2 }, { 8, 3, 2 }, { 11, 7, 1 } };
            }
            else
            {
                A2 = MatrixFunctions.FillArray(n);
                B2 = MatrixFunctions.FillArray(n);
            }
            Y3 = MatrixFunctions.MatrixMultiply(A2, MatrixFunctions.MatrixSubstruction(B2, C2));
            return Task.CompletedTask;
        }
        public Task<double[,]> Get_Y3()
        {
            return Task.FromResult(Y3);
        }
    }
    public interface IDoCalculationsGrain : IGrainWithStringKey
    {
        public Task Set_values(double[,] y1, double[,] y1t, double[,] y2, double[,] y2t, double[,] Y3);
        Task<double[,]> CalculateFirstGap();
        Task<double[,]> CalculateSecondGap();

    }
    public class AddGrain : Grain, IDoCalculationsGrain
    {
        private double[,] y1;
        private double[,] y1t;
        private double[,] y2;
        private double[,] y2t;
        private double[,] Y3;
        private double K1 = 0.0000001;
        public Task Set_values(double[,] y1, double[,] y1t, double[,] y2, double[,] y2t, double[,] Y3)
        {
            this.y1 = y1;
            this.y1t = y1t;
            this.y2 = y2;
            this.y2t = y2t;
            this.Y3 = Y3;
            return Task.CompletedTask;
        }
        public async Task<double[,]> CalculateFirstGap()
        {
            var firstGap = MatrixFunctions.MatrixSum(MatrixFunctions.MatrixSum(MatrixFunctions.MatrixMultiplyOnNumber(MatrixFunctions.MatrixMultiply(MatrixFunctions.MatrixMultiply(y2, MatrixFunctions.MatrixMultiply(MatrixFunctions.MatrixMultiply(y1t, Y3), y1)), y2t), K1), MatrixFunctions.MatrixMultiply(Y3, Y3)), MatrixFunctions.MatrixMultiply(y1, y2t));
            return firstGap;
        }
        public async Task<double[,]> CalculateSecondGap()
        {
            var secondGap = MatrixFunctions.MatrixMultiplyOnNumber(MatrixFunctions.MatrixSum(MatrixFunctions.MatrixMultiply(y1, MatrixFunctions.MatrixMultiply(MatrixFunctions.MatrixMultiply(y2t, Y3), y2)), y1), K1);
            return secondGap;
        }
    }
   static class MatrixFunctions
    {
        //Functions
        public static double[,] FillArray(int size)
        {
            Random r = new Random();
            var array = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    array[i, j] = r.Next(0, 1000) + r.NextDouble();
                }
            }
            return array;
        }
        public static double[,] FillArrayColumn(int size)
        {
            Random r = new Random();
            var array = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                array[i, 0] = r.Next(0, 1000) + r.NextDouble();
            }
            return array;
        }
        public static double[,] MatrixMultiply(double[,] array1, double[,] array2)
        {
            int n = array1.GetLength(0);
            double[,] array = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int k = 0;
                    double sum = 0;
                    while (k < n)
                    {
                        sum += array1[i, k] * array2[k, j];
                        k++;
                    }
                    array[i, j] = sum;
                }
            }
            return array;
        }
        public static void PrintMatrix(double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                    Console.Write($"{Math.Round(array[i, j], 4)}\t");
                Console.WriteLine();
            }
        }
        public static double[,] TMatrix(double[,] array)
        {
            double[,] res = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    res[j, i] = array[i, j];
                }
            }
            return res;
        }
        public static double[,] Initialize_b(int n)
        {
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                res[i, 0] = 8 / (double)(i + 1);
            }
            return res;
        }
        public static double[,] MatrixMultiplyOnNumber(double[,] array, double number)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = array[i, j] * number;
                }
            }
            return array;
        }
        public static double[,] MatrixSum(double[,] array, double[,] array2)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = array2[i, j] + array[i, j];
                }
            }
            return array;
        }
        public static double[,] MatrixSubstruction(double[,] array, double[,] array2)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = array[i, j] - array2[i, j];
                }
            }
            return array;
        }
        public static double[,] Initialize_C2(int n)
        {
            double[,] array = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    array[i, j] = (double)1 / (i + 1 + j + 1 + 2);
                }
            }
            return array;
        }
    }
}