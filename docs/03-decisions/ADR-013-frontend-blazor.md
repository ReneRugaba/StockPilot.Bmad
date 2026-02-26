# ADR-013 — Frontend pédagogique minimal (Blazor Web App)

Date: 2026-02-26  
Status: Accepted  
Deciders: Project team  

## Context

Le projet est pédagogique et vise à démontrer la méthode BMAD
de bout en bout.

Un frontend minimal permet :

- de visualiser les opérations métier
- de simuler les cas d’usage
- de démontrer l’impact des décisions architecturales
- d’illustrer les transitions d’état

## Decision

Le projet inclut un frontend minimal en **Blazor Web App (Server)** (.NET 8).

Le frontend :

- consomme l’API interne
- reste simple (pas de design complexe)
- vise uniquement à simuler les opérations métier

## Consequences

- Démonstration plus concrète
- Itérations US plus visibles
- Complexité maîtrisée (même stack .NET)
- Possibilité de retirer ou remplacer le front ultérieurement