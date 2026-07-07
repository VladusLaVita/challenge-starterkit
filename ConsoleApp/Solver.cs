using Challenge.DataContracts;
using System;

namespace ConsoleApp;

public class Solver
{
    public static string Solve(TaskResponse taskResponse)
    {
        string question = taskResponse.Question;
        string userHint = taskResponse.UserHint ?? "";

        if (DeterminantSolver.IsDeterminantTask(question, userHint))
        {
            return DeterminantSolver.Solve(question);
        }

        return "0";
    }
}