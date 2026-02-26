# ADR-005 — Système interne uniquement pour le MVP

Date: 2026-02-26  
Status: Accepted  
Deciders: Architecture Team  

## Context

Deux options :

- Portail client externe
- Système interne uniquement

Le portail client introduit :
- Complexité sécurité
- Multi-tenant technique
- Authentification avancée

## Decision

Le MVP est un système interne.
Le client reste un concept métier sans accès direct.

## Consequences

- Simplification de la sécurité
- Réduction complexité technique
- Possibilité d’ajouter un portail en phase ultérieure