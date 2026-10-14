CONTEXTE
Diaspora : plateforme .NET 10 en microservices (Wolverine + RabbitMQ, EF Core/PostgreSQL, YARP, Aspire)
et app Flutter (Clean Architecture, Riverpod, go_router, GetIt). Objectif : Marketplace + Documents
multi-pays africains.

CONVENTIONS À RESPECTER (observées dans le code)
- C# : primary constructors, dépendances injectées nommées _camelCase, Result / Result<T>
  (Shared.Domain.Commons) pour les erreurs métier, jamais d'exception pour une entrée client invalide.
- Handlers Wolverine : classe publique avec méthode Handle ; commandes = records ICommand<Result<T>>.
- Entités : AggregateRoot, setters privés, fabrique statique Create, événements de domaine.
- EF : configuration héritant de BaseEntityConfiguration<T> ; repositories héritant de Repository<T> ;
  soft delete via ISoftDeletable (filtre global). Pas de SQL brut avec noms de colonnes non vérifiés.
- Endpoints : [Permission(resource, action, description, recommendedRoles)] sur chacun.
- Commentaires et messages métier en français ; identifiants en anglais.

INVARIANTS DE SÉCURITÉ (non négociables)
1. Jamais d'identité, de rôle, de statut « vérifié », de propriétaire ou de montant dû lu depuis le client.
   L'identité vient de ICurrentUserService (jeton) ; le jeton ne contient QUE le UserId.
2. Tout endpoint recevant un id de ressource vérifie la propriété (ou un rôle explicite) AVANT d'agir.
3. État inter-services = événements + projections locales. Pas d'appel HTTP synchrone entre services.
4. Tout consumer est idempotent (rejouable, ordre non garanti).
5. Aucun secret, aucune donnée personnelle dans les logs.

MÉTHODE
- Une branche feat/multipays-marketplace ; un commit par tâche ; messages « type(scope): sujet ».
- Fichiers livrés COMPLETS (pas de diff partiel) ; ne modifier que ce que la tâche exige.
- Avant de modifier un fichier : le lire en entier. Ne rien supposer sur une signature : la vérifier.
- Après chaque tâche : `dotnet build Diaspora.sln` (aucune nouvelle erreur ni nouveau warning),
  tests du projet concerné, et `flutter analyze` + `flutter test` côté mobile.
- Migrations EF : les dossiers Migrations ne sont PAS dans le dépôt. Demander à l'utilisateur la
  politique avant d'en committer ; les générer avec `dotnet ef migrations add <Nom> -p <Infra> -s <Api>`
  et relire le SQL (colonnes, index, nullabilité) avant application.
- Si une hypothèse du plan est fausse : arrêter, le dire, proposer une correction. Ne pas contourner.

RAPPORT À LA FIN DE CHAQUE TÂCHE
fichiers modifiés/créés · commandes exécutées + résultat réel · tests ajoutés · points ouverts.
Ne jamais écrire « terminé » sans la sortie du build et des tests.
