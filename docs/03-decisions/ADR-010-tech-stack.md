# ADR-010 — Tech stack pédagogique (.NET)

Date: 2026-02-26  
Status: Accepted  
Deciders: Project team  

## Context

Le projet est pédagogique (méthode BMAD). Le stack doit :
- être simple à démarrer
- permettre d’itérer US par US rapidement
- rester lisible et standard
- offrir un bon support tooling

## Decision

Le MVP pédagogique utilise :

- Language/Runtime : **.NET 8 (C#)**
- Type d’app : **Web API**
- Tests : **xUnit**
- Validation : validations simples au niveau application (sans framework lourd au départ)
- Documentation : Markdown + Mermaid

## Consequences

- Démarrage rapide et itératif
- Stack courant et bien supporté
- Facile à faire évoluer vers une architecture plus complexe si nécessaire