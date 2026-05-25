# FabLab Étiquette

Application de bureau Windows pour la création et la mise en page d'étiquettes destinées à la **découpeuse laser** du FabLab Orano.

---

## Fonctionnalités

### Créer des étiquettes PDF
- Saisie des informations utilisateur (nom, service, numéro)
- Choix du style de plaque (souple, rigide, très rigide), de la couleur et du nombre d'impressions
- Éditeur visuel avec prévisualisation du plateau **600 × 300 mm**
- Ajout, duplication et suppression d'étiquettes
- **Sélection multiple** (Ctrl+Clic, Maj+Clic) avec modification simultanée
- Propriétés par étiquette : texte, taille en mm, position, police, forme, action (Découpe / Gravure), couleur de fond
- Disposition automatique **horizontale** ou **verticale**
- Grille d'alignement, ajout d'image, saut de ligne
- Sauvegarde / chargement de projet (JSON)
- Génération PDF avec **nom de fichier standardisé**

### Standardiser un PDF existant
- Import d'un PDF existant pour le reformater selon les standards FabLab

### Administration
- Gestion des styles de plaques (nom, épaisseur, autocollante, couleurs disponibles)
- Configuration du dossier de sortie des PDF par défaut
- Réinitialisation aux valeurs d'usine

---

## Format du nom de fichier généré

```
COULEUR_STYLE_xNB#NOM_SERVICE_NUMERO#TITRE_TEXTE.pdf
```

Exemple : `Blanc sur fond Noir_ETQ-SOUPLE_x3#DUPONT_MAINTENANCE_042#Armoire électrique_Disjoncteur principal.pdf`

---

## Prérequis

- Windows 10 ou supérieur (64 bits)
- **.NET 8** — inclus dans le setup si vous utilisez l'installeur

---

## Installation

### Option 1 — Installeur (recommandé)
1. Télécharger `FabLab_Etiquette_Setup.exe`
2. Lancer et suivre l'assistant (en français)
3. Un raccourci est créé dans le menu Démarrer ; option bureau disponible

### Option 2 — Exécutable portable
1. Télécharger `FabLab Etiquette.exe`
2. Double-cliquer pour lancer, aucune installation requise

> Si Windows affiche un avertissement SmartScreen, cliquer sur **"Informations complémentaires"** puis **"Exécuter quand même"**.

---

## Compilation depuis les sources

```bash
# Cloner le dépôt
git clone https://github.com/FonkyPeaky/FabLab-Etiquette.git
cd FabLab-Etiquette

# Build debug
dotnet build "FabLab Etiquette/FabLab Etiquette.csproj"

# Publier en .exe autonome
dotnet publish "FabLab Etiquette/FabLab Etiquette.csproj" ^
  -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

**Dépendances NuGet**

| Package | Version |
|---|---|
| MaterialDesignThemes | 5.2.1 |
| itext7 | 7.1.16 |
| PDFsharp | 6.1.1 |
| Newtonsoft.Json | 13.0.3 |

---

## Structure du projet

```
FabLab Etiquette/
├── Assets/             Logos et icône
├── Helpers/            RelayCommand (MVVM)
├── Models/             LabelModel, AppConfig, UserInfo, FabricationParams
├── Services/           ConfigService (lecture/écriture config JSON)
├── ViewModels/         CreatePdfViewModel, AdminViewModel, ...
├── Views/              Fenêtres et vues WPF
├── PdfService.cs       Génération PDF (iText7 + PDFsharp)
└── ColorUtils.cs       Utilitaires couleur
```

La configuration utilisateur est stockée dans `%AppData%\FabLabEtiquette\config.json`.

---

## Développé par

FonkyPeaky
Projet relancé en 2026 à partir de la base d'Arnaud !
