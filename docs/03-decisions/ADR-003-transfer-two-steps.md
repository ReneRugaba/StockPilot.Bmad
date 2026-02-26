# ADR-003 — Transfert inter-entrepôt en deux temps

Date: 2026-02-26  
Status: Accepted  
Deciders: Architecture Team  

## Context

Deux approches étaient possibles :

- Transfert atomique (un seul mouvement)
- Transfert en deux temps :
  - Expédition (Stored → InTransit)
  - Réception (InTransit → Stored)

Le modèle à deux temps reflète mieux la réalité opérationnelle.

## Decision

Le transfert inter-entrepôt est implémenté en deux temps.

L’état `InTransit` est conservé dans le cycle de vie.

## Consequences

- Modèle plus réaliste
- Meilleure traçabilité
- Complexité légèrement accrue
- Meilleure extensibilité future (gestion incidents transport)