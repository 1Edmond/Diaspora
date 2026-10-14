using Marketplace.Application.Abstractions;
using Marketplace.Application.Commands;
using Marketplace.Application.Normalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class NormalizeListingLocalesCommandHandler(
    IApplicationDbContext _context,
    IListingDataNormalizer _normalizer,
    ILogger<NormalizeListingLocalesCommandHandler> _logger)
{
    public async Task<Result<NormalizationReport>> Handle(
        NormalizeListingLocalesCommand command,
        CancellationToken cancellationToken = default)
    {
        var listings = await _context.ServiceListings.ToListAsync(cancellationToken);
        var report = _normalizer.NormalizeListings(listings);

        if (report.TotalUpdated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (report.UnrecognizedListings.Count > 0)
        {
            foreach (var item in report.UnrecognizedListings)
            {
                _logger.LogWarning(
                    "Listing {ListingId} has unrecognized locales: Country='{RawCountry}', Currency='{RawCurrency}'",
                    item.ListingId, item.RawCountry, item.RawCurrency);
            }
        }

        _logger.LogInformation(
            "Listing locale normalization completed: {TotalScanned} scanned, {TotalUpdated} updated, {UnrecognizedCount} unrecognized",
            report.TotalScanned, report.TotalUpdated, report.UnrecognizedListings.Count);

        return Result.Success(report);
    }
}
