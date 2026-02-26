# Analyse — MVP (Phase A de BMAD)

Objectif : analyser les options structurantes du modèle métier
avant validation officielle en phase D (Decide).

Le MVP privilégie la simplicité, tout en conservant
une cohérence métier réaliste.

---

## 1. Lot : indivisible vs quantifié

### Option A — Lot indivisible

Description :
- Un lot représente une unité logique unique.
- Retrait toujours total.
- Pas de gestion de quantité interne.

Avantages :
- Simplifie diagramme d’état.
- Pas de gestion de retrait partiel.
- Réduit les risques de concurrence.
- Simplifie invariants métier.
- Moins de logique métier à tester.

Risques :
- Moins réaliste dans certains cas.
- Peut nécessiter création de plusieurs lots au dépôt.

---

### Option B — Lot quantifié

Description :
- Un lot contient une quantité interne.
- Retraits partiels possibles.

Avantages :
- Plus réaliste.
- Plus flexible.

Risques :
- Complexité sur mouvements partiels.
- Risque de stock incohérent.
- Concurrence sur Quantity.
- Invariants plus complexes.
- Tests plus nombreux.

---

### Analyse

Pour un MVP simple et maîtrisé :

👉 Recommandation : **Lot indivisible**

La complexité ajoutée par la gestion de quantité
n’apporte pas une valeur suffisante au stade MVP.

---

## 2. Réservation

### Option A — Sans réservation

Description :
- Retrait direct.
- Pas d’état Reserved.

Avantages :
- Modèle plus simple.
- Moins de transitions.
- Moins de gestion de concurrence.

Risques :
- Conflits possibles en environnement multi-opérateurs.

---

### Option B — Avec réservation

Description :
- État intermédiaire avant retrait.
- Bloque opérations concurrentes.

Avantages :
- Plus robuste.
- Prévention métier claire.

Risques :
- Ajoute état supplémentaire.
- Complexifie transitions.
- Nécessite règles supplémentaires.

---

### Analyse

Pour le MVP :

👉 Recommandation : **Pas de réservation**

La complexité ne justifie pas l’ajout immédiat.
Peut être introduite en V2.

---

## 3. Transfert inter-entrepôt

### Option A — Transfert atomique

Description :
- Un seul mouvement logique.
- Pas d’état InTransit.

Avantages :
- Simple.
- Moins de transitions.

Risques :
- Moins réaliste.
- Pas de gestion claire du transport.

---

### Option B — Transfert en deux temps

Description :
- Expédition (Stored → InTransit)
- Réception (InTransit → Stored)

Avantages :
- Modèle métier cohérent.
- Aligné avec réalité opérationnelle.
- Bonne valeur pédagogique.
- Prépare évolutions futures.

Risques :
- Légère complexité supplémentaire.

---

### Analyse

👉 Recommandation : **Conserver le transfert en 2 temps**

La valeur métier est importante
et la complexité reste raisonnable.

---

## 4. Capacité des emplacements

### Option A — Sans gestion de capacité

Avantages :
- Ultra simple.
- Pas de calcul.
- Moins de règles métier.

Risques :
- Pas de contrôle sur surcharge physique.

---

### Option B — Capacité simple (nombre max)

Avantages :
- Contrôle basique.
- Meilleure cohérence physique.

Risques :
- Ajoute validation systématique.
- Complexité supplémentaire.

---

### Analyse

👉 Recommandation : **Pas de gestion de capacité au MVP**

Peut être ajoutée ultérieurement sans casser le modèle.

---

## 5. Portail client

### Option A — Système interne uniquement

Avantages :
- Simplifie sécurité.
- Pas de multi-tenant technique.
- Moins d’authentification complexe.

Risques :
- Pas d’autonomie client.

---

### Option B — Portail client

Avantages :
- Plus réaliste.
- Valeur utilisateur directe.

Risques :
- Sécurité.
- Isolation forte.
- Complexité supplémentaire.

---

### Analyse

👉 Recommandation : **Système interne uniquement pour le MVP**

Le client reste un concept métier.
L’accès externe pourra être une évolution future.

---

# 6. Synthèse des recommandations MVP

| Sujet | Décision recommandée |
|--------|----------------------|
| Lot | Indivisible |
| Réservation | Non |
| Transfert | 2 temps |
| Capacité | Non |
| Portail client | Non |

---

