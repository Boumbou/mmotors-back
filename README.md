# MMOTORS Back


# MMotors — Backend

Backend Web API de **MMotors**, couvrant notamment :
- Catalogue véhicules (vente/location)
- Comptes clients & authentification
- Création / gestion des dossiers
- Téléversement et consultation de documents
- Opérations admin/staff (gestion des dossiers, des véhicules, des services, des templates de documents)

## Stack technique

- **C#** — **.NET 10**
- **ASP.NET Core Web API** (Controllers)
- **PostgreSQL**
- **Entity Framework Core** (ORM)
- **Microsoft Identity** + **JWT** (authentification/autorisation)
- **Docker** (conteneurisation)

## Architecture (inspirée DDD)

Organisation orientée “features” pour isoler les responsabilités et faciliter l’évolution.

```

Data

|__DbContext.cs 
| 
|__DataSeeder.cs 

Features
|__Feature A
|  |--Controllers
|  |--Dtos
|  |--Interfaces
|  |--Repositories
|  |--Services
|  
|__Feature n ...
|__Shared
   |__Interfaces
   |__Services

Mappers

Models

```

## Principes clés

- Séparation claire Controllers / Services / Repositories
- DTOs pour maîtriser les données exposées au client et limiter les fuites d’informations sensibles
- ORM pour réduire les risques d’injection SQL
- Tests unitaires priorisés sur les flux critiques (notamment repositories)
- Approche TDD sur les features complexes lorsque pertinent :
  1. écrire les tests (rouge)
  2. implémenter le minimum (vert)
  3. refactor / optimiser

## Démarrage rapide

### Prérequis
- .NET SDK 10
- PostgreSQL (local ou Docker)
- (Optionnel) configuration AWS si tests S3 en local

### Lancer en local

```

dotnet restore

dotnet run

```


### Migrations (EF Core)

```

dotnet ef database update

```

## Configuration

Via `appsettings.json` + `appsettings.Development.json` (ou variables d’environnement en prod).

Typiquement :
- connexion PostgreSQL
- paramètres JWT (issuer, audience, clé de signature, expiration)
- paramètres S3 (bucket médias/documents), si utilisé

Exemple (format variables d’environnement) :

```

ConnectionStrings__Default=Host=[localhost](http://localhost);Port=5432;Database=mmotors;Username=postgres;Password=postgres

Jwt__Issuer=mmotors

Jwt__Audience=mmotors-web

Jwt__SigningKey=CHANGE_ME_SUPER_SECRET

Aws__S3__BucketName=mmotors-media

```

## Documentation API

- Swagger (disponible sur `/swagger`) pour tester les endpoints en local 
- Diagnostic via :
  - codes HTTP
  - payloads
  - erreurs retournées par l’API
  - onglet Network du navigateur (pour distinguer front/back/infra)

## Déploiement (AWS) — Vue d’ensemble

Architecture de production :

- **Backend** packagé en image Docker
- Image stockée dans **ECR**
- Déploiement via **ECS** (Task + Service)
- Exposition interne via **ALB**
- Entrée publique via **CloudFront**
  - `/api/*` → ALB → ECS Service
  - `/*` → S3 Front bucket (SPA)

Données & assets :
- PostgreSQL sur **RDS** (privé, pas d’accès internet direct)
- **S3** pour les médias (images véhicules, documents clients)

Sécurité :
- Ressources en réseau privé, accès restreints
- Contrôle des flux via Security Groups
- TLS côté CloudFront
- Protection DDoS / limitations au niveau edge (selon config)

## Debug & Logging — pratiques

En développement :
- Reproduire l’erreur avant correction
- Débogage pas à pas (breakpoints, inspection variables)
- Logs ciblés et temporaires (retirés après résolution)
- Analyse des réponses HTTP (status/payload) pour isoler front vs back vs infra
- Ajout/ajustement de tests unitaires comme filet anti-régression

En run :
- Exploration des logs via la console AWS (ex : CloudWatch selon setup)

## Workflow Git

Flux Git “classique” :

- `main` → production
- `dev` → intégration / test
- `feature/<nom>` → branches de feature
- `fix/<nom>` → hotfix

## Demo / UAT

- Application (UAT) : https://www.mmotors-uat.click/
