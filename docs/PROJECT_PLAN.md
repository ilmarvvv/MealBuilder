# MealBuilder Project Plan

## Schema

Problem -> solution idea -> main entities -> main scenarios -> business rules -> implementation.

## 1. Problem: Why the Project Exists

I have many recipes, but they are inconvenient to change manually.

When I add, remove, or change ingredient quantities, I have to recalculate my nutrition table again: calories, protein, fiber, sugar, salt, and other nutrients. This takes a lot of time and can easily lead to mistakes.

Ingredient data also has to be searched for repeatedly: calories, protein, fiber, and other values per 100 g, per piece, or per milliliter. Because of this, creating or changing a recipe becomes slow.

Menu planning is another problem. I want to quickly build a menu from several recipes or individual products and immediately see total values for a day or for a selected period.

Portions and days are also inconvenient. If a dish is prepared for several portions or several days, I have to manually calculate values for one portion or one day.

In the future, I also want to track vitamins and other micronutrients, but storing and recalculating that data manually is even more difficult.

## 2. Solution Idea: How the Project Solves the Problem

Create a system that stores ingredients, recipes, and menus in one place.

The user will be able to add ingredients with nutrition values: calories, protein, fiber, sugar, salt, vitamins, and other nutrients. Values can be stored per 100 g, per one piece, or another unit of measurement.

Based on saved ingredients, the user will be able to create recipes. A recipe can include ingredients, and the user can add them, remove them, or change their quantities. A finished recipe can also be reused as part of another recipe, for example as a sauce, filling, or prepared component.

The system will automatically recalculate total recipe values after each change: when an ingredient quantity changes, a new ingredient is added, an ingredient is removed, or another finished recipe is used as part of a dish.

For recipes, the user will be able to specify the number of portions or the number of days the dish lasts. The system will calculate values for the whole recipe, for one portion, or for one day.

The user will also be able to create menus from several recipes or individual products. The system will show total menu values for a day or for a selected period.

The main idea is to save ingredient or finished recipe data once, then reuse it in recipes and menus without manual recalculation.

## 3. Main Entities: What Objects Exist in the System

This is the initial entity list. Some names and relationships may change as the project grows and the domain becomes clearer.

Entity details will be refined before implementing each feature area:

- [ ] Pass 2: define responsibilities for each entity
- [ ] Pass 3: define main fields and relationships before implementation

Entity refinement template:

```md
### EntityName

Short description.

Responsibility:
- ...

Main data:
- ...

Relationships:
- ...

Notes:
- ...
```

### Ingredient

A single product with nutrition values. For example: chicken, rice, egg, olive oil, or milk.

### Recipe

A dish, preparation, or reusable recipe that can be cooked and used again. For example: salad, sauce, chicken with rice, or filling.

### RecipeIngredient

A connection between a recipe and an ingredient. It describes which ingredient is used in a recipe and in what quantity.

### RecipeComponent

A connection between one recipe and another recipe. It is needed when a finished recipe is used as part of another recipe, for example a sauce in a burger or a filling in a pie.

### Menu

A set of products, recipes, or dishes that the user plans to eat during a day or selected period.

### MenuItem

A single item inside a menu. It can represent a recipe, an individual ingredient, or a prepared dish added to the menu.

## 4. Main Scenarios: What the User Can Do

This section describes the main user workflows. The exact UI and technical implementation may change later.

For the first version, the project is treated as a single-user local app: one user works with their own ingredients, recipes, and menus locally. User accounts, registration, and login are out of scope for the first version.

### Working With Ingredients

The user can create an ingredient and store nutrition values for it.

The user can edit ingredient data if the values were incorrect or changed.

The user can find a saved ingredient and reuse it in a recipe or menu.

### Working With Recipes

The user can create a recipe.

The user can add ingredients to a recipe and specify their quantities.

The user can change an ingredient quantity in a recipe or remove an ingredient from a recipe.

The system automatically recalculates recipe nutrition values after changes.

The user can save a recipe and reuse it later.

### Working With Recipe Components

The user can use a finished recipe as part of another recipe.

For example, the user can create a sauce as a separate recipe, then add that sauce to a burger, salad, or another dish.

### Working With Portions and Days

The user can specify how many portions a dish is split into.

The user can see nutrition values for one portion.

The user can specify how many days a dish lasts.

The user can see nutrition values for one day.

### Working With Menus

The user can create a menu for a day or selected period.

The user can add several recipes or individual products to a menu.

The user can see total calories, protein, fiber, sugar, salt, and other values for the menu.

The user can change menu contents or remove an item from the menu.

### Searching and Reusing Data

The user can search saved ingredients or recipes.

The user can reuse the same ingredients in different recipes.

The user can reuse finished recipes in menus or in other recipes.

## 5. Business Rules: What Must Always Be True

This section describes rules that the system must follow regardless of the UI or implementation details.

### Ingredient Rules

Each ingredient must have a name.

The ingredient name cannot be empty.

Ingredient nutrition values are stored as base values for future calculations.

Calories, protein, fiber, sugar, salt, and other nutrients cannot be less than 0.

Values measured per 100 g should not exceed physically possible limits. For example, calories per 100 g cannot be more than 900, and protein or fiber cannot be more than 100 g per 100 g of product.

### Unit and Input Rules

All calculations inside the system should be converted to grams.

If a product is entered in grams, the entered quantity is already the weight in grams.

If a product is entered in pieces, the system must know the weight of one piece in grams.

If a product is entered in pieces, the weight in grams is calculated as: weight in grams = number of pieces * grams per piece.

The quantity of a product in a recipe or menu must be greater than 0.

### Calculations

The main formula for calculating a nutrition value for a specific product weight is: calculated value = value per 100 g * actual weight in grams / 100.

For example, if 100 g of chicken contains 23 g of protein and the recipe uses 250 g of chicken, the calculation is: protein = 23 * 250 / 100 = 57.5 g.

Total recipe nutrition values are the sum of all ingredient and component values.

If a recipe uses another finished recipe, that recipe's values are included as part of the total recipe values.

The system should not change base ingredient values while calculating recipes or menus.

### Recipes

Each recipe must have a name.

A recipe can contain many ingredients.

A recipe can contain another finished recipe as a component.

The quantity of each ingredient or component in a recipe must be greater than 0.

An empty recipe can exist as a draft, but its nutrition values are 0 until ingredients or components are added.

### Recipe Component Rules

A finished recipe can be used as part of another recipe.

A recipe must not directly or indirectly contain itself to avoid endless calculations.

For example, `Sauce` can be part of `Burger`, but `Burger` cannot be part of `Sauce` if `Sauce` is already used in `Burger`.

### Portion and Day Rules

The number of portions must be greater than 0.

The number of days must be greater than 0.

Values for one portion are calculated as total recipe values divided by the number of portions.

Values for one day are calculated as total recipe values divided by the number of days.

### Menus

A menu can contain recipes, individual ingredients, or prepared dishes.

The quantity of each menu item must be greater than 0.

Total menu values are the sum of all menu item values.

### First Version Scope

The first version does not include user accounts, registration, or login.

The first version is treated as a single-user local app.

Complex measurement units, vitamins, and micronutrients can be refined later.

## 6. Implementation Plan: In What Order the Project Will Be Built

This section describes the development order. The plan can change as the project grows and better solutions become clearer.

Each milestone should produce a small but complete result. After completing a milestone or a logical step, the changes can be committed.

### Milestone 1: Project Setup and Database

Goal: create the basic project structure and connect the database.

- [x] Create ASP.NET Core Razor Pages project
- [x] Add `Ingredient` model
- [x] Add EF Core packages
- [x] Add `AppDbContext`
- [x] Configure SQLite connection
- [x] Add initial migration
- [x] Apply migration to create local database

### Milestone 2: Ingredients CRUD

Goal: allow the user to manage ingredients through the UI.

- [x] Add Ingredients list page
- [x] Add Create Ingredient page
- [x] Add Edit Ingredient page
- [x] Add Details Ingredient page
- [x] Add Delete Ingredient page
- [x] Show validation messages
- [x] Add navigation link to Ingredients
- [x] Add sugar and salt fields to Ingredient

### Milestone 3: Recipe Models

Goal: create the basic recipe model and the relationship between recipes and ingredients.

- [ ] Refine `Recipe` entity
- [ ] Refine `RecipeIngredient` entity
- [ ] Add `Recipe` model
- [ ] Add `RecipeIngredient` model
- [ ] Add EF Core relationships
- [ ] Add migration for recipes

### Milestone 4: Recipes CRUD

Goal: allow the user to create and edit recipes.

- [ ] Add Recipes list page
- [ ] Add Create Recipe page
- [ ] Add Edit Recipe page
- [ ] Add Details Recipe page
- [ ] Add Delete Recipe page
- [ ] Allow adding ingredients to a recipe
- [ ] Allow changing ingredient quantities in a recipe
- [ ] Allow removing ingredients from a recipe

### Milestone 5: Recipe Components

Goal: allow a finished recipe to be used as part of another recipe.

- [ ] Refine `RecipeComponent` entity
- [ ] Add `RecipeComponent` model
- [ ] Add EF Core relationship for recipe components
- [ ] Allow adding a recipe as a component of another recipe
- [ ] Prevent a recipe from containing itself
- [ ] Include recipe components in nutrition calculations

### Milestone 6: Recipe Calculations

Goal: automatically calculate recipe nutrition values.

- [ ] Calculate total calories
- [ ] Calculate total protein
- [ ] Calculate total fiber
- [ ] Calculate total sugar
- [ ] Calculate total salt
- [ ] Show recipe totals in the UI

### Milestone 7: Portions and Days

Goal: calculate values for one portion or one day.

- [ ] Add portions count to recipes
- [ ] Add days count to recipes
- [ ] Calculate values per portion
- [ ] Calculate values per day
- [ ] Show portion and day values in the UI

### Milestone 8: Menus

Goal: allow the user to build menus and see total values.

- [ ] Refine `Menu` entity
- [ ] Refine `MenuItem` entity
- [ ] Add `Menu` model
- [ ] Add `MenuItem` model
- [ ] Add Menus list page
- [ ] Add Create Menu page
- [ ] Allow adding recipes or products to a menu
- [ ] Calculate total menu values

## 7. Future Ideas

This section contains ideas that may be useful for the project in the future, but are not part of the nearest implementation plan.

- [ ] Add image support for ingredients
  - Decide later how images should be stored.

- [ ] Add AI-assisted ingredient nutrition autofill
  - AI can suggest initial nutrition values, but the user should review and correct them before saving.

- [ ] Add AI-assisted nutrition label photo import
  - AI can analyze a photo of a nutrition label, extract available values, and suggest missing values when possible.

- [ ] Add vitamins and micronutrients
  - Decide later which vitamins and micronutrients should be tracked first.

- [ ] Add search and filters

- [ ] Support piece-based products

- [ ] Support milliliters or other units

- [ ] Add user accounts and authentication

- [ ] Improve UI

- [ ] Add tests
