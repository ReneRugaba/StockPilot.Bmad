# ADR-001 — Lot indivisible pour le MVP

Date: 2026-02-26  
Status: Accepted  
Deciders: Architecture Team  

## Context

Le modèle prévoit qu’un Lot représente une unité stockée appartenant à un client.
Deux options étaient envisagées :

- Lot quantifié (quantité interne, retraits partiels)
- Lot indivisible (retrait total uniquement)

La gestion d’une quantité interne introduit de la complexité :
- Concurrence
- Invariants supplémentaires
- Mouvements partiels
- Tests plus nombreux

## Decision

Pour le MVP, un Lot est **indivisible**.

Un retrait correspond à la sortie complète du Lot.

## Consequences

- Simplification du diagramme d’état
- Suppression de la logique de retrait partiel
- Réduction des risques d’incohérence
- Possibilité d’évolution ultérieure vers un modèle quantifié