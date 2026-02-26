# Brainstorm – Plateforme de stockage mutualisé multi-entrepôt

## 1. Contexte général

Conception from scratch d’une plateforme permettant à plusieurs clients 
d’entreposer leurs biens dans des entrepôts mutualisés.

Les entrepôts disposent d’emplacements physiques de tailles variables.
Plusieurs clients peuvent utiliser un même entrepôt.

Le système doit permettre le suivi, la traçabilité et la gestion des opérations
de dépôt, retrait et déplacement interne.

---

## 2. Problème à résoudre

Permettre à des clients :

- D’entreposer des biens
- De récupérer leurs biens (totalement ou partiellement)
- De connaître à tout moment ce qu’ils ont en stock
- D’avoir une traçabilité claire des mouvements

Permettre à l’organisation :

- De gérer plusieurs entrepôts
- De mutualiser les espaces
- D’assurer isolation stricte entre clients
- De tracer toutes les opérations
- De limiter les erreurs humaines

---

## 3. Acteurs potentiels

### Client
- Déposer des biens
- Consulter son inventaire
- Demander un retrait
- Consulter l’historique

### Opérateur d’entrepôt
- Enregistrer un dépôt
- Enregistrer un retrait
- Déplacer un lot
- Corriger un emplacement

### Responsable d’entrepôt
- Superviser les opérations
- Valider ajustements
- Gérer incidents
- Surveiller capacité

### Administrateur système
- Créer clients
- Créer entrepôts
- Gérer utilisateurs
- Paramétrer règles générales

---

## 4. Concepts métier à explorer

- Client
- Entrepôt
- Emplacement
- Lot (unité stockée)
- Mouvement
- Dépôt
- Retrait
- Déplacement interne
- Réservation
- Incident
- Capacité
- Historique
- Statut opérationnel

---

## 5. Cas d’usage possibles

### Gestion structurelle
- Créer un entrepôt
- Définir emplacements
- Gérer clients

### Dépôt
- Création d’un lot
- Attribution d’un emplacement
- Enregistrement quantité
- Horodatage et opérateur responsable

### Retrait
- Retrait total
- Retrait partiel
- Validation du retrait
- Historisation

### Déplacement interne
- Changement d’emplacement
- Historique conservé

### Consultation
- Vue par client
- Vue par entrepôt
- Vue consolidée globale

---

## 6. Problèmes métier potentiels

### Isolation
- Risque de mélange entre clients
- Mauvaise restitution

### Traçabilité
- Perte d’historique
- Mouvement non attribué

### Concurrence
- Deux opérateurs agissant sur le même lot

### Capacité
- Emplacement saturé
- Mauvaise gestion de l’espace

### Processus
- Retrait planifié non respecté
- Lot déplacé alors qu’il est réservé

---

## 7. Questions ouvertes

- Un lot contient-il une quantité interne ou est-il indivisible ?
- Le client a-t-il accès direct au système ?
- Y a-t-il une capacité maximale par emplacement ?
- Faut-il gérer des statuts opérationnels (En attente, Validé, Annulé) ?
- Les transferts inter-entrepôts sont-ils nécessaires dès le départ ?
- Faut-il gérer une réservation avant retrait ?