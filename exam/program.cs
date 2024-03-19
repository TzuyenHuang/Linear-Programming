using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.IO;
using System.Transactions;

string filePath = @"C:\Users\Tzuyen\Desktop\exam\Instance 1.csv";
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

void Print(bool phase, double[][] arr, int m, int n)
{
    for (int i = 0; i < m; i++)
    {
        for (int j = 0; j < n; j++) Console.Write("{0, -8}", Math.Round(arr[i][j], 2) + " ");
        Console.WriteLine();
    }
    if (phase) //Phase II
    {
        Console.Write("BFS: ");
        for (int i = 1; i < m; i++) Console.Write("x" + arr[i][0] + " = " + Math.Round(arr[i][n - 1], 2) + ", ");
        Console.WriteLine("Objective value = " + Math.Round(arr[0][n - 1], 2));
    }
    else //Phase I
    {
        Console.Write("BFS: ");
        int num;
        for (int i = 1; i < m - 1; i++)
        {
            if (arr[i][0] == n - 3) num = 0;
            else num = (int)arr[i][0];
            Console.Write("x" + num + " = " + Math.Round(arr[i][n - 2], 2) + ", ");
        }
        Console.WriteLine("Objective value = " + Math.Round(arr[m - 1][n - 2], 2));
    }
    Console.WriteLine("----------------------------------------------------------------------");
}

bool Detect(double[][] arr, int m, int n)
{
    for (int i = 1; i < m; i++)
    {
        if (arr[i][n - 1] < 0) return false; //Phase I
    }
    return true; //Phase II
}

bool CheckOpt(bool phase, double[][] arr, int m, int n)
{
    int i = 0;
    if (phase == false) i = m - 1;
    for (int j = 2; j < n - 1; j++)
    {
        if (arr[i][j] < 0) return false;
    }
    return true;
}

int Enter(bool phase, double[][] arr, int m, int n)
{
    double min = 0;
    int i = 0;
    if (phase == false) i = m - 1;
    int enter = 1;
    for (int j = 2; j < n - 1; j++)
    {
        if ((phase == false) && (j == n - 3)) continue; //防止Phase I找enter時讀到RHS那行
        if (arr[i][j] < min)
        {
            min = arr[i][j];
            enter = j - 1;
        }
    }
    return enter;
}

int Leave(bool phase, double[][] arr, int m, int n, int enter)
{
    double ratio = 1;
    double minr = 10000;
    int leave = 1;

    int end = m;
    int rhs = n - 1;
    if (phase == false)
    {
        end = m - 1;
        rhs = n - 3;
    }
    for (int i = 1; i < end; i++)
    {
        if (arr[i][enter + 1] > 0) ratio = arr[i][rhs] / arr[i][enter + 1];
        else continue;
        if (ratio < minr)
        {
            minr = ratio;
            leave = i;
        }
    }
    if(minr == 10000) leave = 0;
    return leave;
}

void Operation(bool phase, double[][] arr, int enter, int leave, int m, int n)
{
    //Row operations
    arr[leave][0] = enter;
    double divide = arr[leave][enter + 1];
    for (int j = 2; j < n; j++) arr[leave][j] = arr[leave][j] / divide;

    //Replace entering variable for other equations
    int start = 0;
    if (phase == false) start = 1;
    for (int i = start; i < m; i++)
    {
        if (i == leave) continue;
        double coef = arr[i][enter + 1];
        for (int j = 2; j < n; j++) arr[i][j] = arr[i][j] + (-1) * coef * arr[leave][j];
    }
}

//Print the initial dictionary
int it = 0;
Console.WriteLine("[Iteration: " + it + "]");
Print(true, arr, m, n);

//Detect RHS
bool phase = Detect(arr, m, n);

bool optimal = false;

//infeasibe -> Phase I
if (phase == false)
{
    Console.WriteLine("< Phase I >\n");
    m = m + 1;
    //add row
    Array.Resize(ref arr, arr.Length + 1);
    arr[m - 1] = new double[n + 1];
    for(int  j = 0; j < n; j++)
    {
        if (j == 1) arr[m - 1][j] = 1;
        else arr[m - 1][j] = 0;
    }
    arr[0][n] = 0;
    for (int i = 1; i < m - 1; i++) arr[i][n] = -1;
    arr[m - 1][n] = 1;
    Print(phase, arr, m, n + 1);
    //xo enter -> enter = n -1
    double a = arr[1][n - 1];
    int b = 1;
    for(int i = 2; i < m - 1; i++)
    {
        if (arr[i][n - 1] < a)
        {
            a = arr[i][n - 1]; //找最負的RHS
            b = i;
        }
    }
    Console.WriteLine("enter: x0, leave: x" + arr[b][0]);
    //operation
    Operation(phase, arr, n - 1, b, m, n + 1);
    Print(phase, arr, m, n + 1);
    optimal = CheckOpt(phase, arr, m, n);
    while (!optimal)
    {
        //find enter, leave -> operation
        int enter = Enter(phase, arr, m, n + 2); //entering variable
        int leave = Leave(phase, arr, m, n + 2, enter); //leaving variable
        Console.WriteLine("enter: x" + enter + ", leave: x" + arr[leave][0]);
        Operation(phase, arr, enter, leave, m, n + 1);
        Print(phase, arr, m, n + 1);
        //optimal = true;
        optimal = CheckOpt(phase, arr, m, n);
    }

    if (arr[m -1][n - 1] == 0) //check infeasibility
    {
        //整理
        phase = true; //Go to PhaseII
        m = m - 1;
        double[] coef = new double[m - 1];
        for (int i = 1; i < m; i++) {
            int c = (int)arr[i][0];
            coef[i - 1] = arr[0][c + 1];
        }
        for (int j = 2; j < n; j++)
        {
            for (int i = 1; i < m; i++)
            {
                arr[0][j] = arr[0][j] + (-1) * coef[i - 1] * arr[i][j];
            }
        }
        Console.WriteLine("Drop xo from the equations and reintroduce the original objective function.");
        Print(phase, arr, m, n);
    }
    else
    {
        Console.WriteLine("Infeasible");
        return; //End
    }
}

//feasible -> Phase II
Console.WriteLine("< Phase II >\n");
optimal = CheckOpt(phase, arr, m, n);
while (!optimal)
{
    it++;
    int enter = Enter(phase, arr, m, n); //entering variable
    int leave = Leave(phase, arr, m, n, enter); //leaving variable
    if(leave == 0)
    {
        Console.WriteLine("Unbounded");
        return; //End
    }
    Console.WriteLine("enter: x" + enter + ", leave: x" + arr[leave][0]);
    Operation(phase, arr, enter, leave, m, n);
    Console.WriteLine("[Iteration: " + it + "]");
    Print(phase, arr, m, n);
    optimal = CheckOpt(phase, arr, m, n);
}
Console.WriteLine("Optimal value = " + Math.Round(arr[0][n - 1], 2));
return; //End
