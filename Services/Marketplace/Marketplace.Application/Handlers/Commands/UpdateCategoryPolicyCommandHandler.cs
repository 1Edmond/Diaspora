using Marketplace.Application.Abstractions;
using Marketplace.Application.Commands;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class UpdateCategoryPolicyCommandHandler(IApplicationDbContext context)
{
    public async Task<Result> Handle(UpdateCategoryPolicyCommand command, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);

        if (category is null)
            return Result.Failure("Category not found.");

        if (category.Policy is null)
        {
            var policy = CategoryPolicy.Create(
                command.CategoryId,
                command.MinPublishLevel,
                command.MinBookLevel,
                command.MaxWeightKg,
                command.MaxValueAmount,
                command.RequiresEscrow,
                command.RequiresPreModeration,
                command.DefaultCommissionRate
            );
            category.SetPolicy(policy);
            context.CategoryPolicies.Add(policy);
        }
        else
        {
            category.Policy.UpdatePolicy(
                command.MinPublishLevel,
                command.MinBookLevel,
                command.MaxWeightKg,
                command.MaxValueAmount,
                command.RequiresEscrow,
                command.RequiresPreModeration,
                command.DefaultCommissionRate
            );
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
