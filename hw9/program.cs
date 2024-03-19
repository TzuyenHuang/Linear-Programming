using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.IO;
using System.Transactions;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

DateTime start = DateTime.Now;
DateTime end;
string filePath = @"C:\Users\Tzuyen\Desktop\hw9\Test_instance.csv";
StreamReader streamReader = new StreamReader(filePath);

//存數值到陣列
//int i = 0;
double[][] arr = new double[1][];
int m = 0, n = 0;
while (!streamReader.EndOfStream) //讀檔
{
    string lines = streamReader.ReadLine(); //讀行
    string[] num = lines.Split(','); //分開所有數值

    double value;
    arr[m] = new double[num.Length + 1];
    n = num.Length;

    for (int j = 0; j < num.Length; j++)
    {
        value = double.Parse(num[j]);
        arr[m][j] = value;
    }
    m++;
    Array.Resize(ref arr, arr.Length + 1);
}

//Print matrix
/*for(int i = 0; i < m; i++)
{
    for (int j = 0; j < n; j++)
    {
        Console.Write(arr[i][j] + " ");
    }
    Console.WriteLine();
}
Console.WriteLine();*/

//------------------------------------------------------------------------------------------------

//Initialize

double r = 0.9;
double delta = 0.1;
double epsilon = 0.01;

double[,] A = new double[m - 1, n - m - 2];
for(int i = 0; i < m - 1; i++)
{
    for(int j = 0;j < n - m - 2; j++)
    {
        A[i, j] = arr[i + 1][j + 2];
        //Console.Write(A[i,j] + " ");
    }
    //Console.WriteLine() ;
}

double[] c = new double[n - m - 2];
double[] x = new double[n - m - 2];
double[] z = new double[n - m - 2];
for (int j = 0; j < n - m - 2; j++)
{
    c[j] = -arr[0][j + 2];
    x[j] = 1;
    z[j] = 1;
    //Console.WriteLine("c[" + j + "] = " + c[j]);
}

double[] b = new double[m - 1];
double[] w = new double[m - 1];
double[] y = new double[m - 1];
for (int i = 0; i < m - 1; i++)
{
    b[i] = arr[i + 1][n - 1];
    w[i] = 1;
    y[i] = 1;
    //Console.WriteLine("b[" + i + "] = " + b[i]);
}


//------------------------------------------------------------------------------------------------

int iteration = 0;
double[] rho = new double[m - 1];
double[] sigma = new double[n - m - 2];
double gamma = 0;
double mu;

double norm_x = 0;
double norm_y = 0;
double norm_rho = 0;
double norm_sigma = 0;

//compute rho, sigma, gamma, mu
for (int i = 0; i < m - 1; i++)
{
    rho[i] = b[i];
    for (int j = 0; j < n - m - 2; j++) rho[i] = rho[i] - A[i, j] * x[j];
    rho[i] = rho[i] - w[i];
    //Console.WriteLine("rho[" + i + "] = " + rho[i]);
}

for (int i = 0; i < n - m - 2; i++)
{
    sigma[i] = c[i];
    for (int j = 0; j < m - 1; j++) sigma[i] = sigma[i] - A[j, i] * y[j];
    sigma[i] = sigma[i] + z[i];
    //Console.WriteLine("sigma[" + i + "] = " + sigma[i]);
}

for (int i = 0; i < n - m - 2; i++) gamma = gamma + z[i] * x[i];
for (int j = 0; j < m - 1; j++) gamma = gamma + y[j] * w[j];

mu = delta * (gamma / (n - 3));

for (int i = 0; i < m - 1; i++) norm_rho = norm_rho + Math.Abs(rho[i]);
for (int i = 0; i < n - m - 2; i++) norm_sigma = norm_sigma + Math.Abs(sigma[i]);
//Console.WriteLine("norm_rho = " + norm_rho + ", norm_sigma = " +  norm_sigma + ", gamma = " + gamma + ", mu = " + mu);

int N = 2 * (n - 3);
while (true)
{
    //Console.WriteLine("Iteration: " + iteration);

    //solve
    double[] F = new double[N];
    for (int i = 0; i < m - 1; i++) F[i] = -rho[i];
    for (int i = m - 1; i < n - 3; i++) F[i] = -sigma[i - (m - 1)];
    for (int i = n - 3; i < 2 * n - m - 5; i++) F[i] = x[i - (n - 3)] * z[i - (n - 3)] - mu;
    for (int i = 2 * n - m - 5; i < N; i++) F[i] = y[i - (2 * n - m - 5)] * w[i - (2 * n - m - 5)] - mu;
    //for(int i = 0; i  < N; i++) Console.WriteLine("F[" + i + ", 0] = " + F[i]);

    double[,] dF = new double[N, N];
    for (int i = 0; i < m - 1; i++)
    {
        for (int j = 0; j < n - m - 2; j++) dF[i, j] = A[i, j];
        for (int j = n - m - 2; j < n - 3; j++)
        {
            if (j == i + (n - m - 2)) dF[i, j] = 1;
            else dF[i, j] = 0;
        }
        for (int j = n - 3; j < N; j++) dF[i, j] = 0;
    }
    for (int i = m - 1; i < n - 3; i++)
    {
        for (int j = 0; j < n - 3; j++) dF[i, j] = 0;
        for (int j = n - 3; j < n + m - 4; j++) dF[i, j] = A[j - (n - 3), i - (m - 1)];
        for (int j = n + m - 4; j < N; j++)
        {
            if (i - (m - 1) == j - (n + m - 4)) dF[i, j] = -1;
            else dF[i, j] = 0;
        }
    }
    for (int i = n - 3; i < 2 * n - m - 5; i++)
    {
        for (int j = 0; j < n - m - 2; j++)
        {
            if (i - (n - 3) == j) dF[i, j] = z[j];
            else dF[i, j] = 0;
        }
        for (int j = n - m - 2; j < n + m - 4; j++) dF[i, j] = 0;
        for (int j = n + m - 4; j < N; j++)
        {
            if (i - (n - 3) == j - (n + m - 4)) dF[i, j] = x[i - (n - 3)];
            else dF[i, j] = 0;
        }
    }
    for (int i = 2 * n - m - 5; i < N; i++)
    {
        for (int j = 0; j < n - m - 2; j++) dF[i, j] = 0;
        for (int j = n - m - 2; j < n - 3; j++)
        {
            if (i - (2 * n - m - 5) == j - (n - m - 2)) dF[i, j] = y[i - (2 * n - m - 5)];
            else dF[i, j] = 0;
        }
        for (int j = n - 3; j < n + m - 4; j++)
        {
            if (i - (2 * n - m - 5) == j - (n - 3)) dF[i, j] = w[i - (2 * n - m - 5)];
            else dF[i, j] = 0;
        }
        for (int j = n + m - 4; j < N; j++) dF[i, j] = 0;
    }
    /*for (int i = 0; i < N; i++)
    {
        for (int j = 0; j < N; j++)
        {
            Console.Write(dF[i, j] + " ");
        }
        Console.Write("\n");
    }*/

    Matrix<double> coef = Matrix<double>.Build.DenseOfArray(dF);
    Vector<double> rhs = Vector<double>.Build.DenseOfArray(F);
    Vector<double> xi = coef.Solve(rhs);
    //for(int i = 0; i < N; i++) Console.WriteLine("xi[" + i + "] = " + xi[i]);

    double Max = -100;
    int index = -1;
    for (int i = 0; i < n - m - 2; i++)
    {
        if (xi[i] / x[i] > Max)
        {
            Max = xi[i] / x[i];
            index = i;
        }
    }
    for (int i = n - m - 2; i < n - 3; i++)
    {
        if (xi[i] / w[i - (n - m - 2)] > Max)
        {
            Max = xi[i] / w[i - (n - m - 2)];
            index = i;
        }
    }
    for (int i = n - 3; i < n + m - 4; i++)
    {
        if (xi[i] / y[i - (n - 3)] > Max)
        {
            Max = xi[i] / y[i - (n - 3)];
            index = i;
        }
    }
    for (int i = n + m - 4; i < N; i++)
    {
        if (xi[i] / z[i - (n + m - 4)] > Max)
        {
            Max = xi[i] / z[i - (n + m - 4)];
            index = i;
        }
    }

    double theta = r * (1 / Max);
    //Console.WriteLine("index = " + index + ", Max = " + Max + ", theta = " +  theta);

    //------------------------------------------------------------------------------------------------

    //replace (x, w, y, z)
    for (int i = 0; i < n - m - 2; i++)
    {
        x[i] = x[i] - theta * xi[i];
        //Console.Write("x[" + i + "] = " + Math.Round(x[i], 2) + ", ");
    }
    //Console.WriteLine();
    for (int i = n - m - 2; i < n - 3; i++)
    {
        w[i - (n - m - 2)] = w[i - (n - m - 2)] - theta * xi[i];
        //Console.Write("w[" + (i - (n - m - 2)) + "] = " + Math.Round(w[i - (n - m - 2)], 2) + ", ");
    }
    //Console.WriteLine();
    for (int i = n - 3; i < n + m - 4; i++)
    {
        y[i - (n - 3)] = y[i - (n - 3)] - theta * xi[i];
        //Console.Write("y[" + (i - (n - 3)) + "] = " + Math.Round(y[i - (n - 3)], 2) + ", ");
    }
    //Console.WriteLine();
    for (int i = n + m - 4; i < N; i++)
    {
        z[i - (n + m - 4)] = z[i - (n + m - 4)] - theta * xi[i];
        //Console.Write("z[" + (i - (n + m - 4)) + "] = " + Math.Round(z[i - (n + m - 4)], 2) + ", ");
    }
    //Console.WriteLine();

    /*double objective = 0;
    for (int i = 0; i < n - m - 2; i++)
    {
        objective = objective + c[i] * x[i];
        //Console.WriteLine("c[" + i + "] = " + c[i] + ", x[" + i + "] = " + x[i]);
    }*/
    //Console.WriteLine("Objective value = " + Math.Round(objective, 2));
    //Console.WriteLine("--------------------------------------------------");
    iteration++;

    double M = 1.79E+308;
    norm_x = x[0];
    for(int i = 0; i < n - m - 2; i++) if (x[i] > norm_x) norm_x = x[i];
    norm_y = y[0];
    for (int i = 0; i < m - 1; i++) if (y[i] > norm_y) norm_y = y[i];

    for (int i = 0; i < m - 1; i++)
    {
        rho[i] = b[i];
        for (int j = 0; j < n - m - 2; j++) rho[i] = rho[i] - A[i, j] * x[j];
        rho[i] = rho[i] - w[i];
        //Console.WriteLine("rho[" + i + "] = " + rho[i]);
    }

    for (int i = 0; i < n - m - 2; i++)
    {
        sigma[i] = c[i];
        for (int j = 0; j < m - 1; j++) sigma[i] = sigma[i] - A[j, i] * y[j];
        sigma[i] = sigma[i] + z[i];
        //Console.WriteLine("sigma[" + i + "] = " + sigma[i]);
    }

    gamma = 0;
    for (int i = 0; i < n - m - 2; i++) gamma = gamma + z[i] * x[i];
    for (int j = 0; j < m - 1; j++) gamma = gamma + y[j] * w[j];
    //Console.WriteLine("gamma = " + gamma);

    mu = delta * (gamma / (n - 3));

    norm_rho = 0;
    for (int i = 0; i < m - 1; i++) norm_rho = norm_rho + Math.Abs(rho[i]);
    norm_sigma = 0;
    for (int i = 0; i < n - m - 2; i++) norm_sigma = norm_sigma + Math.Abs(sigma[i]);
    //Console.WriteLine("norm_rho = " + norm_rho + ", norm_sigma = " + norm_sigma + ", gamma = " + gamma + ", mu = " + mu);

    if (norm_x > M)
    {
        Console.WriteLine("Solution status: Primal unbounded.");
        end = DateTime.Now;
        Console.WriteLine("Total run time = " + (end - start).TotalMilliseconds.ToString() + " ms");
        return; //End
    }
    if (norm_y > M)
    {
        Console.WriteLine("Solution status: Dual unbounded.");
        end = DateTime.Now;
        Console.WriteLine("Total run time = " + (end - start).TotalMilliseconds.ToString() + " ms");
        return; //End
    }
    if (norm_rho < epsilon && norm_sigma < epsilon && gamma < epsilon) break;
}

Console.WriteLine("Number of iterations = " + (iteration - 1));
Console.WriteLine("Solution status: Optimal");
for (int i = 0; i < n - m - 2; i++) Console.Write("x[" + i + "] = " + Math.Round(x[i], 2) + ", ");
Console.WriteLine();
for (int i = n - m - 2; i < n - 3; i++) Console.Write("w[" + (i - (n - m - 2)) + "] = " + Math.Round(w[i - (n - m - 2)], 2) + ", ");
Console.WriteLine();
for (int i = n - 3; i < n + m - 4; i++) Console.Write("y[" + (i - (n - 3)) + "] = " + Math.Round(y[i - (n - 3)], 2) + ", ");
Console.WriteLine();
for (int i = n + m - 4; i < N; i++) Console.Write("z[" + (i - (n + m - 4)) + "] = " + Math.Round(z[i - (n + m - 4)], 2) + ", ");
Console.WriteLine();

double objective = 0;
for (int i = 0; i < n - m - 2; i++) objective = objective + c[i] * x[i];
Console.WriteLine("Objective value = " + Math.Round(objective, 2));

end = DateTime.Now;
Console.WriteLine("Total run time = " + (end - start).TotalMilliseconds.ToString() + " ms");
return; //End
