using System.Threading;
using System.Threading.Tasks;
using WorkTogetherly.Application.Contracts;

namespace WorkTogetherly.Application.Services;

public interface IAccountService
{
    Task<RegisterResult> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
}
