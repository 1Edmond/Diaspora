using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Freelance.Application.Handlers.Commands;

public class CreateJobCategoryCommandHandler(
    IJobCategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateJobCategoryCommand command, CancellationToken cancellationToken)
    {
        var existing = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == command.Name, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>("Une catégorie avec ce nom existe déjà.");

        var category = JobCategory.Create(command.Name, command.Description, command.IsActive, command.DisplayOrder);
        if (command.AllowedProfileTypeIds is not null)
        {
            category.SetAllowedProfileTypes(command.AllowedProfileTypeIds);
        }

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}
