using Marketplace.Application.Normalization;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Marketplace.Application.Commands;

public record NormalizeListingLocalesCommand() : ICommand<Result<NormalizationReport>>;
