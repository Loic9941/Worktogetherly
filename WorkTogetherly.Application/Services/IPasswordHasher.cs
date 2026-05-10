namespace WorkTogetherly.Application.Services;

public interface IPasswordHasher
{
    string Hash(string password);
}
