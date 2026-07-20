using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Procedure.Domain.Entities;
using Shared.Application.Extensions;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;

namespace Procedure.Infrastructure.Persistences;

public class ProcedureSeeder(ProcedureDbContext dbContext, ILogger<ProcedureSeeder> logger)
{
    private readonly ProcedureDbContext _dbContext = dbContext;
    private readonly ILogger<ProcedureSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        var existProcedure = await _dbContext.Procedures.AnyAsync();
        if (existProcedure)
        {
            _logger.LogInformation("Procedures already exist");
            return;
        }

        // ── Locations ────────────────────────────────────────────────────

        var ambassadeRuTg = Location.Create(
            "Ambassade de la Russie à Lomé",
            Address.Create(
                "Rue des Ambassades",
                "Lomé",
                "Maritime",
                "BP 2662",
                "Togo"
                ).Value,
            50, 
            50,
            "+228 22 21 21 21",
            "consulat.togo@mid.ru",
            new OpeningHours(
                [
                    new DaySchedule(DayOfWeek.Monday,    false, new TimeOnly(9,0), new TimeOnly(13,0)),
                    new DaySchedule(DayOfWeek.Tuesday,   false, new TimeOnly(9,0), new TimeOnly(13,0)),
                    new DaySchedule(DayOfWeek.Wednesday, false, new TimeOnly(9,0), new TimeOnly(13,0)),
                    new DaySchedule(DayOfWeek.Thursday,  false, new TimeOnly(9,0), new TimeOnly(13,0)),
                    new DaySchedule(DayOfWeek.Friday,    false, new TimeOnly(9,0), new TimeOnly(13,0)),
                    new DaySchedule(DayOfWeek.Saturday,  true, new TimeOnly(), new TimeOnly()),
                    new DaySchedule(DayOfWeek.Sunday,    true, new TimeOnly(), new TimeOnly()),
                ]
                )
            ).Value;

        var anacTogo = Location.Create(
            "ANaC - Agence Nationale de Sécurité Aéroportuaire",
            Address.Create(
                "Aéroport International Gnassingbé Eyadéma",
                "Lomé",
                "Maritime",
                "BP 393",
                "Togo"
                ).Value,
            20,
            20,
            "+228 22 26 34 34",
            "contact@anac.tg",
            new OpeningHours(
                [
                    new DaySchedule(DayOfWeek.Monday,    false, new TimeOnly(7,30), new TimeOnly(16,0)),
                    new DaySchedule(DayOfWeek.Tuesday,   false, new TimeOnly(7,30), new TimeOnly(16,0)),
                    new DaySchedule(DayOfWeek.Wednesday, false, new TimeOnly(7,30), new TimeOnly(16,0)),
                    new DaySchedule(DayOfWeek.Thursday,  false, new TimeOnly(7,30), new TimeOnly(16,0)),
                    new DaySchedule(DayOfWeek.Friday,    false, new TimeOnly(7,30), new TimeOnly(16,0)),
                    new DaySchedule(DayOfWeek.Saturday,  true, new TimeOnly(), new TimeOnly()),
                    new DaySchedule(DayOfWeek.Sunday,    true, new TimeOnly(), new TimeOnly()),
                ]
                )
            ).Value;

        // ── Procédures internes (Togo) ──────────────────────────────────

        var demandeVisaRussie = Domain.Entities.Procedure.Create(
            "Demande de Visa",
            "Vous devez demander votre visa auprès de l'ambassade concernée.",
            Money.Create(35000, "CFA").Value,
            ProcedureProfileType.Internal,
            ServiceDataConstants.TgInternalProfileTypeId,
            20
            ).Value;
        demandeVisaRussie.AddLocation(ambassadeRuTg.Id);
        //demandeVisaRussie.AddRequiredDocument(ServiceDataConstants.PassportDocumentTypeId);
        //demandeVisaRussie.AddRequiredDocument(ServiceDataConstants.PhotoIdentiteDocumentTypeId);

        var demandePasseport = Domain.Entities.Procedure.Create(
            "Établissement de Passeport",
            "Faire établir ou renouveler son passeport togolais.",
            Money.Create(50000, "CFA").Value,
            ProcedureProfileType.Internal,
            ServiceDataConstants.TgInternalProfileTypeId,
            15
            ).Value;
        //demandePasseport.AddRequiredDocument(ServiceDataConstants.CniDocumentTypeId);
        //demandePasseport.AddRequiredDocument(ServiceDataConstants.ExtraitNaissanceDocumentTypeId);

        var demandeCni = Domain.Entities.Procedure.Create(
            "Carte d'Identité Nationale",
            "Faire établir sa carte nationale d'identité.",
            Money.Create(5000, "CFA").Value,
            ProcedureProfileType.Internal,
            ServiceDataConstants.TgInternalProfileTypeId,
            10
            ).Value;
        //demandeCni.AddRequiredDocument(ServiceDataConstants.ExtraitNaissanceDocumentTypeId);

        // Dépendance métier : le passeport nécessite la CNI au préalable
        demandePasseport.AddDependency(demandeCni.Id);

        // Le visa dépend du passeport
        demandeVisaRussie.AddDependency(demandePasseport.Id);

        var internalsProcedures = new List<Domain.Entities.Procedure>()
        {
            demandeCni,
            demandePasseport,
            demandeVisaRussie,
        };

        // ── Procédures externes ──────────────────────────────────────────

        var achatNumeroRusse = Domain.Entities.Procedure.Create(
            "Achat de numéro Russe",
            "Vous devez acheter votre numéro russe. Sans cela impossible de vivre sereinement en Russie (banque, livraisons, etc.).",
            Money.Create(15000, "CFA").Value,
            ProcedureProfileType.External,
            ServiceDataConstants.RuExternalProfileTypeId, // TODO: adapte le nom exact de la constante
            2
            ).Value;
        //achatNumeroRusse.AddRequiredDocument(ServiceDataConstants.PassportDocumentTypeId);

        var ouvertureCompteBancaire = Domain.Entities.Procedure.Create(
            "Ouverture de Compte Bancaire",
            "Ouvrir un compte bancaire russe une fois sur place.",
            Money.Create(0, "CFA").Value,
            ProcedureProfileType.External,
            ServiceDataConstants.RuExternalProfileTypeId,
            3
            ).Value;
        ouvertureCompteBancaire.AddDependency(achatNumeroRusse.Id);

        var externalProcedures = new List<Domain.Entities.Procedure>()
        {
            achatNumeroRusse,
            ouvertureCompteBancaire,
        };

        // ── Persistance ───────────────────────────────────────────────────

        await _dbContext.Locations.AddRangeAsync(ambassadeRuTg, anacTogo);
        await _dbContext.Procedures.AddRangeAsync(internalsProcedures);
        await _dbContext.Procedures.AddRangeAsync(externalProcedures);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} procedures.", internalsProcedures.Count + externalProcedures.Count);
    }
}