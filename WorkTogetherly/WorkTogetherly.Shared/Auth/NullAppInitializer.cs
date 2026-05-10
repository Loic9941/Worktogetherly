namespace WorkTogetherly.Shared.Auth
{
    public class NullAppInitializer : IAppInitializer
    {
        public Task InitializeAsync() => Task.CompletedTask;
    }
}
