# ADR-004 — Pas de gestion de capacité au MVP

Date: 2026-02-26  
Status: Accepted  
Deciders: Architecture Team  

## Context

La gestion de capacité (volume/poids/unités max) complexifie :

- Les validations de mouvement
- Les règles métier
- Les calculs

## Decision

La capacité des emplacements n’est pas gérée dans le MVP.

## Consequences

- Modèle simplifié
- Moins de validations métier
- Fonctionnalité introduisible ultérieurement