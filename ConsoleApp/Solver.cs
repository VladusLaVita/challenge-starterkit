using System;
using Challenge.DataContracts;
using ConsoleApp1;

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

            case "statistics":
                return StatisticsSolver.Solve(taskResponse.Question);

            case "steganography":
                return Steganography.Solve(taskResponse.Question);

            default:
                throw new ArgumentException("I dunno the task bro :/");

        }
    }
}
