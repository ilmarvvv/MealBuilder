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

Responsibility:
- store base nutrition values for a product
- provide reusable nutrition data for recipes and menus

Main data:
- name
- calories per 100 g
- protein per 100 g
- fiber per 100 g
- sugar per 100 g
- salt per 100 g

Relationships:
- can be used in many recipes through `RecipeIngredient`
- can be used directly in menus in the future

Notes:
- values are stored per 100 g
- values cannot be negative
- calories per 100 g are limited to 0-900
- protein, fiber, sugar, and salt per 100 g are limited to 0-100

### Recipe

A dish, preparation, or reusable recipe that can be cooked and used again. For example: salad, sauce, chicken with rice, or filling.

Responsibility:
- store basic recipe information
- group ingredients into a reusable dish or preparation
- provide a base for future recipe calculations, portions, and menus

Main data:
- name
- description
- servings

Relationships:
- has many `RecipeIngredient` records
- can be used in menus in the future
- can be used as a component of another recipe through `RecipeComponent`

Notes:
- recipe nutrition values are calculated from its ingredients and components
- an empty recipe can exist as a draft
- servings must be greater than 0 when portion calculations are used

### RecipeIngredient

A connection between a recipe and an ingredient. It describes which ingredient is used in a recipe and in what quantity.

Responsibility:
- connect one recipe with one ingredient
- store how much of the ingredient is used in the recipe
- provide the ingredient weight used for recipe nutrition calculations

Main data:
- recipe id
- ingredient id
- grams

Relationships:
- belongs to one `Recipe`
- references one `Ingredient`

Notes:
- `grams` must be greater than 0
- all first-version recipe calculations use grams
- unit support, piece-based products, and milliliters can be added later

### RecipeComponent

A connection between one recipe and another finished recipe.

It is needed when a finished recipe is used as part of another recipe. For example, a sauce can be created as a separate recipe and then added to a burger, salad, or another dish.

Responsibility:
- connect a parent recipe with another finished recipe
- store how much of the finished recipe is used
- allow finished recipes to be included in another recipe's nutrition calculations

Main data:
- parent recipe id
- component recipe id
- grams

Relationships:
- one parent `Recipe` can have many `RecipeComponent` records
- one component `Recipe` can be used in many `RecipeComponent` records
- the same component `Recipe` can be added only once inside the same parent `Recipe`

Notes:
- `parent recipe` is the recipe that contains the component
- `component recipe` is the finished recipe used as part of another recipe
- `grams` must be greater than 0
- a recipe must not directly contain itself
- a recipe must not indirectly contain itself through other recipes
- duplicate components should be prevented by a unique database index on `ParentRecipeId` and `ComponentRecipeId`
- the UI should not show the current recipe or already added component recipes in the add-component dropdown
- server-side validation should protect the same rules even if the UI is bypassed
- component recipe nutrition values should be included in parent recipe total values
- in the first UI version, recipe ingredients and recipe components will be shown as two separate tables

### Menu

A daily meal plan for a specific date.

It is needed so the user can plan recipes and individual ingredients for a specific day and see total nutrition values for that day.

Responsibility:
- represent one daily meal plan
- group recipes and individual ingredients planned for a specific date
- provide a base for calculating daily nutrition totals
- support future weekly and monthly planning views

Main data:
- name
- date
- description

Relationships:
- has many `MenuItem` records

Notes:
- one menu represents one specific day
- menus can be created for future dates to support planning ahead
- the same date should normally have only one menu
- menu totals are calculated from its menu items
- menu does not store nutrition totals directly in the first version
- weekly and monthly views can be built later by grouping daily menus by date

### MenuItem

A single food item inside a daily menu.

It can represent either a recipe or an individual ingredient planned for a specific day.

Responsibility:
- connect a menu with one planned food item
- allow recipes to be added to a daily menu
- allow individual ingredients to be added to a daily menu
- store the planned amount for that item

Main data:
- menu id
- item type
- recipe id
- ingredient id
- servings count
- grams

Relationships:
- belongs to one `Menu`
- can reference one `Recipe`
- can reference one `Ingredient`

Notes:
- `ItemType` defines whether the item is a recipe or an ingredient
- if `ItemType` is `Recipe`, the item uses `RecipeId` and `ServingsCount`
- if `ItemType` is `Ingredient`, the item uses `IngredientId` and `Grams`
- `ServingsCount` must be greater than 0 for recipe items
- `Grams` must be greater than 0 for ingredient items
- in the first version, one menu item should not reference both a recipe and an ingredient at the same time
- prepared recipe batch tracking can be added later

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

- [x] Refine `Recipe` entity
- [x] Refine `RecipeIngredient` entity
- [x] Add `Recipe` model
- [x] Add `RecipeIngredient` model
- [x] Add EF Core relationships
- [x] Add migration for recipes

### Milestone 4: Recipes CRUD

Goal: allow the user to create and edit recipes.

- [x] Add Recipes list page
- [x] Add Create Recipe page
- [x] Add Edit Recipe page
- [x] Add Details Recipe page
- [x] Add Delete Recipe page
- [x] Allow adding ingredients to a recipe
- [x] Allow changing ingredient quantities in a recipe
- [x] Allow removing ingredients from a recipe

### Milestone 5: Recipe Components

Goal: allow a finished recipe to be used as part of another recipe.

- [x] Refine `RecipeComponent` entity
- [x] Add `RecipeComponent` model
- [x] Add EF Core relationship for recipe components
- [x] Add unique index for `ParentRecipeId` and `ComponentRecipeId`
- [x] Add migration for recipe components
- [x] Show recipe components on Recipe Details page
- [x] Allow adding a recipe as a component of another recipe
- [x] Do not show the current recipe in the add-component dropdown
- [x] Do not show already added component recipes in the add-component dropdown
- [x] Allow changing recipe component quantity
- [x] Allow removing a recipe component from a recipe
- [x] Prevent a recipe from directly containing itself

### Milestone 6: Recipe Calculations

Goal: automatically calculate total nutrition values for a recipe.

- [x] Create recipe nutrition totals model
- [x] Create recipe calculation service
- [x] Calculate nutrition values by grams
- [x] Include recipe ingredients in recipe totals
- [x] Include recipe components in recipe totals
- [x] Calculate total calories, protein, fiber, sugar, and salt
- [x] Show recipe totals on Recipe Details page

### Milestone 7: Portions and Days

Goal: calculate recipe nutrition values per serving and per day.

- [x] Use recipe servings for per-serving calculations
- [x] Add days count to recipes
- [x] Add migration for recipe days count
- [x] Allow editing days count in recipe forms
- [x] Calculate nutrition values per serving
- [x] Calculate nutrition values per day
- [x] Show servings per day on Recipe Details page
- [x] Show per-serving and per-day values on Recipe Details page

### Milestone 8: Daily Menus

Goal: allow the user to plan food for a specific day and see daily nutrition totals.

- [x] Refine `Menu` entity
- [x] Refine `MenuItem` entity
- [x] Add `Menu` model
- [x] Add `MenuItem` model
- [x] Add EF Core relationship for menus
- [x] Add migration for menus
- [x] Add Menus list page
- [x] Add Create Menu page
- [x] Add Menu Details page
- [x] Allow adding recipes to a menu
- [x] Allow adding ingredients to a menu
- [x] Allow changing menu item quantity
- [x] Allow removing items from a menu
- [x] Calculate daily menu totals
- [x] Show daily menu totals on Menu Details page

### Milestone 9: Menu Calendar

Goal: allow the user to view planned menus across multiple days.

- [ ] Add Menu Calendar page
- [ ] Show current week by date
- [ ] Show menus for each day of the week
- [ ] Show daily totals in weekly view
- [ ] Show empty days without a menu
- [ ] Add previous and next week navigation
- [ ] Link from calendar days to Menu Details
- [ ] Allow creating a menu for a selected date

### Milestone 10: Meal Prep Batches

Goal: track prepared recipe batches and remaining servings over time.

- [ ] Refine `PreparedRecipeBatch` entity
- [ ] Add `PreparedRecipeBatch` model
- [ ] Add migration for prepared recipe batches
- [ ] Allow marking a recipe as cooked
- [ ] Store cooked date and total prepared servings
- [ ] Track servings used in daily menus
- [ ] Calculate remaining servings
- [ ] Do not show a prepared batch as available after its servings run out
- [ ] Allow creating a new batch when the recipe is cooked again
- [ ] Show when a prepared batch will run out
- [ ] Show missing daily nutrition when prepared food runs out

### Milestone 11: Menu Planning Improvements

Goal: make menu planning easier and more flexible.

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

- [ ] Add recipe scaling
  - Allow scaling recipes up or down without changing the original recipe.

- [ ] Add editable final recipe weight
  - By default, the system can estimate recipe weight from all ingredients and components.
  - The default estimate does not account for trimming, peeling, evaporation, or other preparation and cooking weight changes.
  - Allow users to override the estimated weight with their own final recipe weight.
  - Use the final recipe weight for more accurate per-gram recipe component calculations.

- [ ] Add advanced recipe validation
  - Prevent the same ingredient from being added more than once inside the same recipe.
  - Prevent indirect recipe component cycles.

- [ ] Add user accounts and authentication

- [ ] Improve UI

- [ ] Add tests
