using DotNetEnv;

namespace ConsoleApp
{
    public static class DotEnv
    {
        public static readonly string SecretKey;
        static DotEnv()
        {
            Env.Load();
            SecretKey = Env.GetString("SECRET_KEY");
        }
    }
}