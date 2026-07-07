using Challenge.DataContracts;

namespace ConsoleApp;

public class Solver
{
    public static string Solve(TaskResponse taskResponse)
    {
        return PolynomialSolver.Solve(taskResponse.Question);
    }
}
