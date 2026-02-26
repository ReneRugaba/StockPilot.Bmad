# ADR-012 — Stockage : SQLite pour le MVP pédagogique

Date: 2026-02-26  
Status: Accepted  
Deciders: Project team  

## Context

Pour un MVP pédagogique, on veut :
- zéro dépendance d’infrastructure (pas besoin de serveur DB)
- persistance simple
- reproductibilité sur n’importe quelle machine

## Decision

Le MVP utilise **SQLite** via **EF Core**.

## Consequences

- On garde une vraie persistance sans setup lourd
- Les migrations sont simples
- On pourra migrer vers PostgreSQL plus tard si nécessaire