# ADR-002 — Suppression de l’état Reserved pour le MVP

Date: 2026-02-26  
Status: Accepted  
Deciders: Architecture Team  

## Context

Le modèle incluait un état `Reserved` pour bloquer un lot avant retrait.

Cette fonctionnalité ajoute :
- Un état supplémentaire
- Des transitions complexes
- Une gestion de concurrence plus élaborée

## Decision

L’état `Reserved` ne fait pas partie du MVP.

Le retrait se fait directement depuis l’état `Stored`.

## Consequences

- Cycle de vie simplifié
- Moins de transitions
- Risque potentiel en environnement multi-opérateurs
- Ajout possible en V2 sans casser le modèle