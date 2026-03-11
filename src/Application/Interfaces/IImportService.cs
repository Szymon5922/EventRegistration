using Application.Models;
using CSharpFunctionalExtensions;

namespace Application.Interfaces
{
    public interface IImportService
    {
        Task<Result> ImportAsync(IEnumerable<ImportUserModel> users);
    }
}
