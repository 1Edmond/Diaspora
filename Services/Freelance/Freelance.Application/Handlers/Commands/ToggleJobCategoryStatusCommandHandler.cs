using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Freelance.Application.Handlers.Commands;

public class ToggleJobCategoryStatusCommandHandler(
    IJobCategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(ToggleJobCategoryStatusCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure("Job category not found.");

        category.Update(category.Name, category.Description, command.IsActive, category.DisplayOrder);
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
