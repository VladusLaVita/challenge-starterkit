using System;
using Challenge.DataContracts;

namespace ConsoleApp;

public class Solver
{
    public static string Solve(TaskResponse taskResponse, string taskType)
    {
        return PolynomialSolver.Solve(taskResponse.Question);
    }
}
