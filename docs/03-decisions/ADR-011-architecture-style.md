# ADR-011 — Style d’architecture : Monolithe modulaire (pédagogique)

Date: 2026-02-26  
Status: Accepted  
Deciders: Project team  

## Context

Le but est de démontrer BMAD sur un projet concret. Une Clean Architecture complète
peut introduire trop de couches et ralentir les itérations au MVP.

## Decision

Le projet adopte un **monolithe modulaire** :

- un seul déploiement
- séparation logique par modules (Domain / Application / Infrastructure / Api)
- discipline de dépendances (Domain ne dépend de rien)

## Consequences

- Lisible et pédagogique
- Bon compromis : structure saine sans surcoût
- Évolution possible vers Clean/Hexa plus stricte si besoin