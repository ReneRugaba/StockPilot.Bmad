# Domain Model — Plateforme de stockage mutualisé multi-entrepôt

> Document de modélisation métier (BMAD — phase M).
> Objectif : conceptualiser le domaine (concepts, relations, flux) de manière **visuelle**.
> Hors-scope : API, base de données, choix techniques, optimisation.

---

## 1) Vue d’ensemble (concepts & interactions)

```mermaid
flowchart LR
  Client[Client] -->|Possède| Lot[Lot / Unité stockée]
  Warehouse[Entrepôt] -->|Contient| Location[Emplacement]
  Location -->|Stocke| Lot
  Lot -->|Historique| Movement[Mouvement]
  Operator[Opérateur] -->|Enregistre| Movement
  Admin[Admin système] -->|Gère| Client
  Admin -->|Gère| Warehouse
  Admin -->|Gère| Location
```

## 2) Diagramme de classes Mermaid 

```mermaid
classDiagram
direction TB

class Client {
  +ClientId: Guid
  +Name: string
  +Status: ClientStatus
  +ContactEmail: string
  +CreatedAt: DateTime
  +UpdatedAt: DateTime
}

class Warehouse {
  +WarehouseId: Guid
  +Name: string
  +Address: string
  +Status: WarehouseStatus
  +CreatedAt: DateTime
  +UpdatedAt: DateTime
}

class Location {
  +LocationId: Guid
  +WarehouseId: Guid
  +Code: string
  +Label: string
  +Zone: string
  +Status: LocationStatus
  +Capacity: int?
  +MaxWeight: decimal?
  +CreatedAt: DateTime
}

class Lot {
  +LotId: Guid
  +ClientId: Guid
  +LocationId: Guid?
  +Reference: string
  +Description: string
  +Status: LotStatus
  +Quantity: int?
  +Weight: decimal?
  +Dimensions: string?
  +CreatedAt: DateTime
  +UpdatedAt: DateTime
  +ExpiryDate: DateTime?
}

class Movement {
  +MovementId: Guid
  +LotId: Guid
  +Type: MovementType
  +FromLocationId: Guid?
  +ToLocationId: Guid?
  +OccurredAt: DateTime
  +PerformedBy: Guid
  +Reason: string?
  +Quantity: int?
  +Notes: string?
}

class User {
  +UserId: Guid
  +DisplayName: string
  +Email: string
  +Role: UserRole
  +IsActive: bool
  +CreatedAt: DateTime
  +LastLoginAt: DateTime?
}

class ClientStatus {
  <<enumeration>>
  ACTIVE
  INACTIVE
  SUSPENDED
}

class WarehouseStatus {
  <<enumeration>>
  ACTIVE
  MAINTENANCE
  CLOSED
}

class LocationStatus {
  <<enumeration>>
  AVAILABLE
  OCCUPIED
  RESERVED
  MAINTENANCE
}

class LotStatus {
  <<enumeration>>
  IN_TRANSIT
  STORED
  RESERVED
  SHIPPED
  DAMAGED
}

class MovementType {
  <<enumeration>>
  INBOUND
  OUTBOUND
  INTERNAL_MOVE
  TRANSFER
  ADJUSTMENT
  DAMAGE_REPORT
}

class UserRole {
  <<enumeration>>
  ADMIN
  WAREHOUSE_MANAGER
  OPERATOR
  CLIENT_USER
  VIEWER
}

%% Relations principales
Client "1" --> "0..n" Lot : owns
Warehouse "1" --> "1..n" Location : contains
Location "0..n" --> "0..1" Lot : stores (current)
Lot "1" --> "0..n" Movement : generates

%% Relations utilisateur
User "1" --> "0..n" Movement : performs

%% Relations de mouvement (emplacements optionnels)
Movement "0..n" --> "0..1" Location : from
Movement "0..n" --> "0..1" Location : to

%% Notes détaillées
note for Lot "Unité de stockage physique (colis, palette, conteneur).\nPeut avoir des contraintes (poids, dimensions, expiration)."
note for Movement "Traçabilité complète des mouvements.\nSupporte les mouvements partiels via Quantity."
note for Location "Emplacements avec contraintes physiques optionnelles.\nZone permet le regroupement logique."
note for User "Rôles pour contrôler les permissions.\nTraçabilité via PerformedBy."
```

## 3) Types de mouvements (taxonomie métier)

Les mouvements représentent des événements métier traçables.
Ils constituent la source de vérité de l’historique.

```mermaid
flowchart LR

%% Taxonomie orientée impact (BMAD - Model)
%% Objectif : classer les mouvements par effet métier

subgraph Stock_Increase["Augmentation de stock"]
  INBOUND["INBOUND / Dépôt"]
  ADJUSTMENT_PLUS["ADJUSTMENT(+) / Ajustement +"]
end

subgraph Stock_Decrease["Diminution de stock"]
  OUTBOUND["OUTBOUND / Retrait"]
  ADJUSTMENT_MINUS["ADJUSTMENT(-) / Ajustement -"]
end

subgraph Relocalisation["Relocalisation (changement de localisation)"]
  INTERNAL_MOVE["INTERNAL_MOVE / Déplacement interne"]
  TRANSFER["TRANSFER / Transfert inter-entrepôt"]
end

subgraph Etat["Changement d'état"]
  DAMAGE_REPORT["DAMAGE_REPORT / Signalement dommage"]
end

%% Notes d'impact (facultatif, mais utile à la lecture)
INBOUND --> I_NOTE["Effet: + stock (création/entrée)"]
OUTBOUND --> O_NOTE["Effet: - stock (sortie)"]
INTERNAL_MOVE --> M_NOTE["Effet: stock inchangé, localisation modifiée"]
TRANSFER --> T_NOTE["Effet: stock inchangé globalement, localisation + entrepôt modifiés"]
DAMAGE_REPORT --> D_NOTE["Effet: statut modifié (ex: DAMAGED)"]
```

## 4) Cycle de vie du Lot (diagramme d’état)

Le lot traverse différents états au cours de sa vie.
Les transitions sont déclenchées par des mouvements métier.

```mermaid
stateDiagram-v2

  [*] --> Draft : Création (pré-enregistrement)

  Draft --> Stored : INBOUND / Dépôt confirmé

  Stored --> Stored : INTERNAL_MOVE
  Stored --> Reserved : Réservation (optionnel)

  %% Transfert inter-entrepôt
  Stored --> InTransit : TRANSFER (expédition)
  InTransit --> Stored : TRANSFER (réception)

  %% Retrait
  Stored --> Retrieved : OUTBOUND
  Reserved --> Retrieved : OUTBOUND

  %% Réservation
  Reserved --> Stored : Annulation réservation

  %% Dommage
  Stored --> Damaged : DAMAGE_REPORT
  Damaged --> Stored : Réintégration
  Damaged --> Retrieved : Sortie définitive

  Retrieved --> [*]
  ```

  ## 5) Diagrammes de séquence (scénarios métier)

Objectif : visualiser les interactions acteur ↔ système autour des opérations clés,
sans entrer dans des détails techniques (API/DB).

### 5.1 Dépôt (INBOUND)

```mermaid
sequenceDiagram
  title Dépôt (INBOUND)

  actor Client as Client
  actor Operator as Opérateur
  participant System as Système
  participant Lot as Lot
  participant Location as Emplacement
  participant Movement as Mouvement

  Client->>Operator: Dépôt de biens (physique)
  Operator->>System: Enregistrer dépôt (Client, LotRef, Qty?, Entrepôt)
  System->>Lot: Créer / Enregistrer Lot
  System->>Location: Assigner emplacement
  System->>Movement: Créer mouvement INBOUND
  System-->>Operator: Confirmation + identifiant lot + emplacement
````

### 5.2 Retrait (OUTBOUND)

```mermaid
sequenceDiagram
  title Retrait (OUTBOUND)

  actor Client as Client
  actor Operator as Opérateur
  participant System as Système
  participant Lot as Lot
  participant Movement as Mouvement

  Client->>Operator: Demande de retrait (physique)
  Operator->>System: Enregistrer retrait (LotRef, Qty? si partiel)
  System->>Lot: Vérifier état (Stored/Reserved?)
  System->>Movement: Créer mouvement OUTBOUND
  System->>Lot: Mettre à jour état (Retrieved ou Qty restante)
  System-->>Operator: Confirmation + reçu de retrait
```

### 5.3 Déplacement interne (INTERNAL_MOVE)

```mermaid
sequenceDiagram
  title Déplacement interne (INTERNAL_MOVE)

  actor Operator as Opérateur
  participant System as Système
  participant Lot as Lot
  participant Source as Emplacement source
  participant Destination as Emplacement destination
  participant Movement as Mouvement

  Operator->>System: Déplacer lot (LotRef, Destination)
  System->>Lot: Vérifier état (Stored, non Reserved/InTransit)
  System->>Source: Vérifier emplacement courant
  System->>Destination: Vérifier destination (disponible/capacité?)
  System->>Movement: Créer mouvement INTERNAL_MOVE
  System->>Lot: Mettre à jour emplacement courant = Destination
  System-->>Operator: Confirmation
```

### 5.4 Transfert inter-entrepôt (TRANSFER en 2 temps)

```mermaid
sequenceDiagram
  title Transfert inter-entrepôt (TRANSFER) — Expédition puis Réception

  actor OperatorA as Opérateur (Entrepôt A)
  actor OperatorB as Opérateur (Entrepôt B)
  participant System as Système
  participant Lot as Lot
  participant Movement as Mouvement
  participant LocA as Emplacement A (source)
  participant LocB as Emplacement B (destination)

  %% Temps 1 : Expédition
  OperatorA->>System: Initier transfert (LotRef, EntrepôtB, EmplacementB)
  System->>Lot: Vérifier état = Stored (non Reserved/Damaged)
  System->>LocA: Vérifier emplacement courant (source)
  System->>Movement: Créer mouvement TRANSFER (EXPEDITION)\nfrom=LocA, to=null
  System->>Lot: Mettre état = InTransit\n+ vider emplacement courant
  System-->>OperatorA: Confirmation expédition (id transfert)

  %% Temps 2 : Réception
  OperatorB->>System: Confirmer réception (id transfert)
  System->>Lot: Vérifier état = InTransit
  System->>LocB: Vérifier destination (disponible/capacité?)
  System->>Movement: Créer mouvement TRANSFER (RECEPTION)\nfrom=null, to=LocB
  System->>Lot: Mettre état = Stored\n+ affecter emplacement courant = LocB
  System-->>OperatorB: Confirmation réception
```