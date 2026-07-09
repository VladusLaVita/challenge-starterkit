using System;
using Challenge.DataContracts;

namespace ConsoleApp;

public class Solver
{
    public static string Solve(TaskResponse taskResponse, string taskType)
    {
        switch (taskType) {
            case "polynomial-roots":
                return PolynomialSolver.Solve(taskResponse.Question);

            case "cypher":
                return Cypher.Solve(taskResponse.Question);

            case "math":
                return MathSolver.Solve(taskResponse.Question);

            default:
                throw new ArgumentException("I dunno the task bro :/");

        }
    }
}
