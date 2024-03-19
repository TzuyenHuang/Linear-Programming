// --------------------------------------------------------------------------
// File: CplexServer.cs
// Version 22.1.1
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
// 5725-A06 5725-A29 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5655-Y21
// Copyright IBM Corporation 2003, 2022. All Rights Reserved.
//
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with
// IBM Corp.
// --------------------------------------------------------------------------
//
// CplexServer.cs - Entering a problem using CplexModeler and
//                  serializing it for solving
//

using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;



public class CplexServer
{
    // define class to transfer model to server
    [System.Serializable()]
    internal class ModelData
    {
        internal IModel model;
        internal INumVar[] vars;
        internal ModelData(IModel m, INumVar[] v)
        {
            model = m;
            vars = v;
        }
    }

    // define class to transfer back solution
    [System.Serializable()]
    internal class SolutionData
    {
        internal Cplex.CplexStatus status;
        internal double obj;
        internal double[] vals;
    }

    public static void Main(string[] args)
    {
        try
        {
            // setup files to transfer model to server
            string mfile = "Model.dat";
            string sfile = "Solution.dat";


            // build model
            INumVar[][] var = new INumVar[1][];
            IRange[][] rng = new IRange[1][];

            CplexModeler model = new CplexModeler();
            PopulateByRow(model, var, rng);

            FileStream mstream = new FileStream(mfile, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(mstream, new ModelData(model, var[0]));
            mstream.Close();

            // start server
            Server server = new Server(mfile, sfile);

            SolutionData sol = null;
            FileStream sstream = new FileStream(sfile, FileMode.Open);
            sol = (SolutionData)formatter.Deserialize(sstream);
            sstream.Close();

            System.Console.WriteLine("Solution status = " + sol.status);

            if (sol.status.Equals(Cplex.CplexStatus.Optimal))
            {
                System.Console.WriteLine("Solution value = " + sol.obj);
                int ncols = var[0].Length;
                for (int j = 0; j < ncols; ++j)
                    System.Console.WriteLine("Variable " + j + ": Value = " + sol.vals[j]);
            }

        }
        catch (ILOG.Concert.Exception e)
        {
            System.Console.WriteLine("Concert exception '" + e + "' caught");
        }
        catch (System.Exception t)
        {
            System.Console.WriteLine("terminating due to exception " + t);
        }
    }


    // The following method populates the problem with data for the
    // following linear program:
    //
    //    Maximize
    //     x1 + 2 x2 + 3 x3
    //    Subject To
    //     - x1 + x2 + x3 <= 20
    //     x1 - 3 x2 + x3 <= 30
    //    Bounds
    //     0 <= x1 <= 40
    //    End
    //
    // using the IModeler API

    internal static void PopulateByRow(IModeler model,
                              INumVar[][] var,
                              IRange[][] rng)
    {
        /*double[] lb = { 0.0, 0.0, 0.0 };
        double[] ub = { 40.0, double.MaxValue, double.MaxValue };
        string[] varname = { "x1", "x2", "x3" };
        INumVar[] x = model.NumVarArray(3, lb, ub, varname);
        var[0] = x;

        double[] objvals = { 1.0, 2.0, 3.0 };
        model.AddMaximize(model.ScalProd(x, objvals));

        rng[0] = new IRange[2];
        rng[0][0] = model.AddLe(model.Sum(model.Prod(-1.0, x[0]),
                                          model.Prod(1.0, x[1]),
                                          model.Prod(1.0, x[2])), 20.0, "c1");
        rng[0][1] = model.AddLe(model.Sum(model.Prod(1.0, x[0]),
                                          model.Prod(-3.0, x[1]),
                                          model.Prod(1.0, x[2])), 30.0, "c2");*/

        /*
        存係數到coef[i][j]矩陣 
        第0列為目標函數, m-1條限制式, n個變數, 第n+1行為rhs

        for(int i = 0; i < m; i++)
        {
            for(int j = 0; j <= n; j++)
            {
                coef[i][j];
            }
        }

                     x1          x2                  xn          |    rhs
        obj.     coef[0][0]   coef[0][1]     ...  coef[0][n-1]   | coef[0][n]
        const.   coef[1][0]   coef[1][1]     ...  coef[1][n-1]   | coef[1][n]
                     :              :                   :        |    :
                     :              :                   :        |    :
                 coef[m-1][0] coef[m-1][1]   ... coef[m-1][n-1]  | coef[m-1][n]

        */
        int m = 3; 
        int n = 2;
        double[,] coef = new double[4, 3]
        {
            { 3.0, 5.0, 0.0 }, { 1.0, 0.0, 4.0 }, { 0.0, 2.0, 12.0 }, { 3.0, 2.0, 18.0 }
        };
        double[] lb = new double[n];
        double[] ub = new double[n];
        for (int j = 0; j < n; j++)
        {
            lb[j] = 0.0;
            ub[j] = double.MaxValue;
        }//定義變數上下界
        //string[] varname = { "x1", "x2", "x3" };
        INumVar[] x = model.NumVarArray(n, lb, ub); //變數個數
        var[0] = x;

        double[] objvals = new double[n + 1];
        for(int j = 0; j <= n; j++)
        {
            objvals[j] = coef[0, j];//存目標函數係數
        }//目標函數
        model.AddMaximize(model.ScalProd(x, objvals));

        rng[0] = new IRange[m]; //限制式個數
        double[] consvals = new double[n];
        for (int i = 1; i <= m; i++)
        {
            for(int j = 0; j < n; j++)
            {
                consvals[j] = coef[i, j];//存限制式係數
                Console.WriteLine(consvals[j]);
            }
            Console.WriteLine(coef[i, n]);
            rng[0][i] = model.AddLe(model.ScalProd(x, consvals), coef[i, n]); //限制式
            Console.WriteLine(rng[0][i]);
        }
    }


    // The server class
    internal class Server
    {
        internal string sfile;
        internal string mfile;

        internal Server(string modelfile, string solutionfile)
        {
            mfile = modelfile;
            sfile = solutionfile;
            try
            {
                FileStream mstream = new FileStream(mfile, FileMode.Open);
                FileStream sstream = new FileStream(sfile, FileMode.Create);

                Cplex cplex = new Cplex();
                ModelData data = null;

                BinaryFormatter formatter = new BinaryFormatter();
                data = (ModelData)formatter.Deserialize(mstream);
                mstream.Close();


                cplex.SetModel(data.model);

                SolutionData sol = new SolutionData();
                if (cplex.Solve())
                {
                    sol.obj = cplex.ObjValue;
                    sol.vals = cplex.GetValues(data.vars);
                }
                sol.status = cplex.GetCplexStatus();

                formatter.Serialize(sstream, sol);
                sstream.Close();

                cplex.End();
            }
            catch (System.Exception t)
            {
                System.Console.WriteLine("server terminates due to " + t);
            }
        }
    }
}
