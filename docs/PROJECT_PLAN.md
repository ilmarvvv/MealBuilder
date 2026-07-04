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

### PreparedRecipeBatch

A specific cooked batch of a recipe.

For example, the user can create a `Burger` recipe and then cook one batch of it on 21/05/2026 with 8 servings. This cooked batch can be used in menus across multiple days until the servings run out.

Responsibility:
- represent a cooked batch of one recipe
- store when the recipe was cooked
- store how many servings were prepared
- allow menu planning based on available prepared food
- help calculate how many servings are already used and how many remain

Main data:
- recipe id
- cooked date
- total servings

Relationships:
- belongs to one `Recipe`
- can be used by many `MenuItem` records

Notes:
- `TotalServings` must be greater than 0
- used servings are calculated from menu items that reference this batch
- remaining servings are calculated as: total servings - used servings
- a prepared batch should not be shown as available when remaining servings are 0
- in the first version, batch nutrition values come from the related recipe
- if the recipe changes later, we may need to decide whether old batches keep old nutrition values or use updated recipe values

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

- [x] Add Menu Calendar page
- [x] Show current week by date
- [x] Show menus for each day of the week
- [x] Show daily totals in weekly view
- [x] Show empty days without a menu
- [x] Add previous and next week navigation
- [x] Link from calendar days to Menu Details
- [x] Allow creating a menu for a selected date

### Milestone 10: Meal Prep Batches

Goal: track cooked recipe batches and remaining servings over time.

- [x] Refine `PreparedRecipeBatch` entity
- [x] Add `PreparedRecipeBatch` model
- [x] Add `PreparedRecipeBatch` relationship to `Recipe`
- [x] Add optional `PreparedRecipeBatch` relationship to `MenuItem`
- [x] Add migration for prepared recipe batches
- [x] Add Prepared Batches list page
- [x] Add Create Prepared Batch page
- [x] Allow selecting a recipe when creating a prepared batch
- [x] Store cooked date and total prepared servings
- [x] Show used and remaining servings for each prepared batch
- [x] Allow adding a prepared batch serving to a menu
- [x] Prevent using more servings than remain in the prepared batch
- [x] Do not show a prepared batch as available after its servings run out
- [x] Allow creating a new batch when the recipe is cooked again

### Milestone 11: Menu Planning Improvements

Goal: make menu planning easier and more understandable.

- [x] Improve prepared batch dropdown labels
- [x] Show recipe name, cooked date, and remaining servings in prepared batch dropdown
- [x] Show prepared batch details in Menu Details
- [x] Show daily average nutrition in Menu Calendar
- [x] Add Edit Menu page
- [x] Add Delete Menu page
- [x] Add Details Prepared Batch page
- [x] Add Delete Prepared Batch page
- [x] Prevent deleting a prepared batch that is already used in menus

### Milestone 12: Core Domain Refinement

Goal: review and improve the core domain model, data structure, business rules, calculations, and user flows before moving to larger features.

This milestone goes through the existing system from ingredients to menus and refines it closer to the real product vision.

#### 12.1 Ingredient Refinement

- [x] Add optional grams-per-piece value
  - Show on Create, Edit, and Details pages.
  - Do not show on the Index page in the first version.
  - If entered, value must be between 0.01 and 10000.
- [x] Add optional grams-per-milliliter value
  - Show on Create, Edit, and Details pages.
  - Do not show on the Index page in the first version.
  - If entered, value must be between 0.01 and 10000.
- [x] Add optional notes field
  - Show on Create, Edit, and Details pages.
  - Do not show on the Index page in the first version.
- [x] Keep ingredient nutrition values per 100g
- [x] Keep all calculations gram-based
- [x] Treat ingredient name as the product and food state description
- [x] Do not require globally unique ingredient names in the first version
- [x] Keep raw/cooked state out of the Ingredient model for now
- [x] Review unknown nutrition values versus real zero values
  - For now, unknown nutrition values are stored as 0.
  - This is a simplification for the first version.
- [x] Review ingredient deletion rules when ingredient is already used in recipes or menus
  - If an ingredient is used in recipes or menus, do not delete it.
  - Archive or deactivate workflow can be added later.

#### 12.2 Recipe Refinement

- [x] Decide whether recipes should use live ingredient values or ingredient snapshots
  - For now, recipes use live ingredient values.
  - Ingredient snapshots can be added later for published or shared recipes.
- [x] Keep recipe description as simple optional text in the first version
- [x] Move structured recipe instructions to Future Ideas
- [x] Move recipe images to Future Ideas
- [x] Move recipe storage and expiration information to Future Ideas
- [x] Do not require globally unique recipe names in the first version
- [x] Prevent deleting a recipe if it is used in recipe components, menus, or prepared batches
- [x] Keep recipe servings as default output servings
- [x] Move recipe days from Recipe to PreparedRecipeBatch
- [x] Use menu items and prepared batches for actual user-specific consumption
- [x] Move recipe final weight to Future Ideas

#### 12.3 Recipe Ingredient Units

- [x] Keep recipe ingredient input grams-only in this milestone
- [x] Move piece and milliliter input support to Future Ideas

#### 12.4 Recipe Components

- [x] Keep recipe components grams-only in the first version
- [x] Keep recipe component calculations approximate in this milestone
- [x] Prevent deleting a recipe if it is used as a component in another recipe
- [x] Keep recipe components based on live recipe values in the first version
- [x] Move recipe component snapshots to Future Ideas

#### 12.5 Prepared Batches

- [x] Treat PreparedRecipeBatch as cooked food inventory
- [x] Use cooked date as the start date for prepared batch planning
- [x] Add planned days to prepared batches
- [x] Calculate servings per day from prepared batch total servings / planned days
- [x] Automatically create menu items across planned days when a batch is created
- [x] Add Create Batch action from Recipe Details
- [x] Preselect recipe when creating a prepared batch from Recipe Details
- [x] Store recipe name snapshot when a prepared batch is created
- [x] Store recipe nutrition totals snapshot when a prepared batch is created
- [x] Use prepared batch snapshot values for menu calculations
- [x] Allow different batches of the same recipe to have different nutrition values
- [x] Move full ingredient-level batch snapshots to Future Ideas
- [x] Move recipe versioning to Future Ideas

#### 12.6 Daily Plans and Menu Items

- [x] Treat each Menu as one daily food plan
- [x] Keep one menu per date
- [x] Allow ingredients and prepared batch servings to be added to any day
- [x] Keep direct ingredient menu items grams-only in this milestone
- [x] Move menu item piece and milliliter input support to Future Ideas
- [x] Stop creating direct recipe menu items from the UI
  - Recipes should be used through prepared batches in the main flow.
- [x] Keep legacy direct recipe menu item display support temporarily
  - Old recipe menu items may still be shown if they already exist in the database.
  - Full removal can be done later after it is safe.

#### 12.7 Calculations and Validation

- [x] Keep ingredient calculation formula: value per 100g * grams / 100
- [x] Use grams for recipe ingredient calculations
- [x] Use grams for direct ingredient menu item calculations
- [x] Calculate prepared batch nutrition from stored batch snapshot values
- [x] Calculate prepared batch per-serving values from snapshot totals / total servings
- [x] Calculate daily menu totals as the sum of menu item values
- [x] Calculate calendar daily average from seven daily totals
- [x] Validate ingredient name is required
- [x] Validate ingredient nutrition value ranges
- [x] Validate ingredient grams-per-piece and grams-per-milliliter are positive when entered
- [x] Prevent deleting ingredients that are already used in recipes or menus
- [x] Validate recipe name is required
- [x] Validate recipe servings are greater than 0
- [x] Prevent deleting recipes that are used in components, menus, or prepared batches
- [x] Validate recipe ingredient quantity is greater than 0
- [x] Validate recipe component grams are greater than 0
- [x] Prevent recipe from directly containing itself as a component
- [x] Prevent duplicate recipe components
- [x] Move indirect recipe component cycle validation to Future Ideas
- [x] Validate prepared batch recipe is required
- [x] Validate prepared batch total servings are greater than 0
- [x] Validate prepared batch planned days are greater than 0
- [x] Use cooked date as the prepared batch planning start date
- [x] Prevent using more prepared batch servings than remain
- [x] Keep one menu per date
- [x] Validate menu item quantity is greater than 0
- [x] Validate prepared batch menu item does not exceed remaining servings

#### 12.8 User Flows and Documentation

- [ ] Review Create/Edit/Details ingredient flow with conversion fields and notes
- [ ] Review adding ingredients to recipes with grams
- [ ] Review adding recipe components to recipes with grams
- [ ] Review recipe totals and per-serving values
- [ ] Review creating prepared batches from Recipe Details
- [ ] Review prepared batch creation with preselected recipe, start date, planned days, and snapshot values
- [ ] Review daily menu planning as one daily food plan per date
- [ ] Review adding direct ingredients to daily plans with grams
- [ ] Review adding prepared batch servings to daily plans
- [ ] Review calendar day creation and daily average behavior
- [ ] Review safe delete behavior for used ingredients, recipes, and prepared batches
- [ ] Update entity descriptions after domain changes
- [ ] Update business rules after domain changes
- [ ] Update implementation plan after each completed domain area
- [ ] Update README if project description becomes outdated

#### 12.9 Recipe Flow Refinement

##### 12.9.1 Recipe Core and Summary Refinement

- [x] Refine recipe core
  - Add default planned days.
  - Add default servings per day.
  - Keep total servings calculated from default planned days * default servings per day.
  - Add prep time.
  - Add cook time.
  - Add optional final weight.
  - Keep recipe categories out of the core model for now.
- [x] Refine Recipe Create page
  - Structure the page as a reusable recipe form.
  - Add a Basic Information section.
  - Show category as a placeholder only.
- [x] Refine Recipe Edit page
  - Reuse the same form structure as Recipe Create.
  - Keep editable recipe core fields in one clear place.
- [x] Refine Recipe Details summary
  - Show recipe totals, per-day values, and per-serving values clearly.
  - Show recipe weight using manual final weight when available, otherwise estimated weight.
  - Show ingredients and recipe components as one recipe contents summary.
- [x] Add mixed recipe weight handling
  - Use manual final weight when set.
  - Otherwise use estimated weight from recipe contents.
- [x] Combine ingredient and recipe component display
  - Use one recipe contents summary for Ingredients and Recipes.
- [x] Add recipe contents ordering
  - Add position numbers for ingredients and recipe components inside a recipe.
  - Show recipe contents in position order.
  - Keep positions continuous without duplicates or gaps.
  - Allow changing recipe content position by entering a position number.

##### 12.9.2 Recipe Edit Flow Refinement

- [x] Make Recipe Details read-only
  - Remove add, edit, remove, and position editing controls from Recipe Details.
  - Keep Recipe Details focused on review, nutrition summary, recipe contents, and prepared batch creation.
- [x] Use Recipe Edit as the main place for editing recipe contents
  - Move add ingredient and add recipe component actions to Recipe Edit.
  - Move edit, remove, and position controls to Recipe Edit.
- [x] Redirect Recipe Create to Recipe Edit after saving
  - Allow the user to create basic recipe information first.
  - Continue filling recipe contents in Recipe Edit.
- [x] Show nutrition summary on Recipe Edit
  - Show total recipe, per-day, and per-serving values.
  - Update values after recipe contents are changed and the page reloads.

#### 12.10 Editable Prepared Batch Snapshot

Goal: treat a prepared batch as an editable cooked copy of a recipe that can be planned across specific days.

- [x] Refine prepared batch snapshot concept
  - `Recipe` is the reusable template.
  - `PreparedRecipeBatch` is the cooked/planned copy created from a recipe.
  - Editing a prepared batch must not change the original recipe.
  - Later changes to the original recipe must not silently change already created prepared batches.
- [x] Add prepared batch item snapshot model
  - Add one snapshot item model for both copied ingredients and copied recipe components.
  - Store item type, source ingredient or source recipe reference, name snapshot, grams, nutrition snapshot values, and position.
  - Use copied snapshot values for batch calculations instead of live recipe values.
- [x] Copy recipe contents into prepared batch items when a batch is created
  - Copy recipe ingredients.
  - Copy recipe components as recipe-type snapshot items.
  - Keep item positions in the same order as the source recipe.
- [x] Show prepared batch items on Prepared Batch Details
  - Show item type, name, grams, calories, protein, fiber, sugar, and salt.
  - Show batch nutrition summary from prepared batch items.
- [x] Separate Prepared Batch Details and Edit pages
  - Keep Details as a read-only page.
  - Move add, edit, remove, and reorder actions to Edit.
- [x] Allow editing prepared batch items before final planning
  - Allow adding an ingredient snapshot item.
  - Allow adding a recipe snapshot item.
  - Allow changing grams.
  - Allow removing items.
  - Allow changing item position.
- [x] Allow changing batch total servings and planned days before saving the plan
  - Use recipe defaults as starting values.
  - Allow user to override total servings and planned days for the cooked batch.
- [x] Save the prepared batch into daily menus after confirming the plan
  - Split servings across planned days.
  - Create or reuse one menu per date.
  - Add prepared batch menu items for each planned day.
- [x] Allow editing daily servings after the batch is planned
  - Allow changing how much of the prepared batch is eaten on a specific day.
  - Recalculate remaining future servings when daily servings change.

#### 12.11 Core Workflow Validation and Stabilization

Goal: review and stabilize the current single-user core functionality before planning Milestone 13.

#### 12.11.1 Ingredients and Daily Planning Review

Goal: review the current ingredient and daily planning workflows before continuing stabilization.

- [x] Review the Ingredients workflow
  - Ingredient CRUD works well enough for the current version.

- [x] Review the Menu and Calendar concept
  - `Menu` currently represents one daily food plan.
  - `Calendar` displays multiple daily food plans across dates.
  - The current model works, but the name `Menu` may be confusing later.

#### 12.11.2 Days and Calendar Review

Goal: review and adjust days, calendar behavior, and related workflow issues found during testing.

- [x] Rename `Menu` to `DailyPlan`
  - Rename related models, pages, service, routes, and database tables.
  - Preserve existing data through a rename-only migration.

- [ ] Remove direct `Recipe` support from `DailyPlanItem`
  - A daily plan should contain only prepared recipe batches and individual ingredients.
  - A recipe must be converted into a prepared batch before it can be allocated to daily plans.
  - Check existing data before removing the recipe relationship and enum value.

### Milestone 13: Users and Authentication

Goal: make the app ready for multiple users.

### Milestone 14: Existing Feature Improvements

Goal: improve the features that already exist.

- [ ] Add ingredient descriptions
  - Store a longer user-facing description for ingredients.
  - Keep technical or personal notes separate from the public description.

- [ ] Add ingredient image support
  - Allow up to 3 images per ingredient.
  - Private draft ingredients may have 0-3 images.
  - Published or shared ingredients should have 1-3 images.
  - The first image should be treated as the main image.

### Milestone 15: API and Frontend Planning

Goal: plan the transition from Razor Pages to an API-based application and choose the future frontend technology.

- [ ] Design full recipe creation flow for the API/frontend version
  - Allow creating a recipe with ingredients and recipe components in one flow.
  - Avoid the temporary Razor Pages two-step flow where the basic recipe is created first and contents are added later.
  - Decide how the frontend should manage unsaved recipe contents before the recipe is saved.

## 7. Future Ideas

This section contains ideas that may be useful for the project in the future, but are not part of the nearest implementation plan.

### Ingredient Improvements

- [ ] Add ingredient brand

- [ ] Add ingredient category

- [ ] Add image support for ingredients
  - Planned for Milestone 14.
  - Allow up to 3 images per ingredient.
  - Private draft ingredients may have 0-3 images.
  - Published or shared ingredients should have 1-3 images.
  - The first image should be treated as the main image.
  - Decide later how images should be stored.

- [ ] Add ingredient data source
  - Decide later how to store whether values came from manual input, package labels, AI, or external databases.

- [ ] Add advanced ingredient state handling
  - For example, raw, cooked, peeled, or trimmed product states.

- [ ] Add package-label-based nutrition input
  - Allow entering nutrition values for a custom reference amount and convert them to values per 100g.

- [ ] Improve ingredient search and display

- [ ] Allow creating a new ingredient from recipe ingredient selection
  - When adding an ingredient to a recipe and the needed ingredient does not exist, allow creating it without leaving the recipe flow.
  - After the ingredient is created, return to the recipe ingredient add flow with the new ingredient available for selection.

- [ ] Use ingredient conversion fields in recipe and menu input
  - Allow users to enter ingredients by pieces when `GramsPerPiece` is available.
  - Allow users to enter ingredients by milliliters when `GramsPerMilliliter` is available.
  - Convert entered quantity to grams for all calculations.
  - Store or display the original quantity and calculated grams when useful.

- [ ] Add ingredient archive or deactivate workflow
  - Used ingredients should probably be hidden instead of deleted.

- [ ] Add AI-assisted ingredient nutrition autofill
  - AI can suggest initial nutrition values, but the user should review and correct them before saving.

- [ ] Add AI-assisted nutrition label photo import
  - AI can analyze a photo of a nutrition label, extract available values, and suggest missing values when possible.

### Nutrients and Health Data

- [ ] Add a BMI calculator
  - Step 1: create a standalone tool that calculates BMI from height and weight.
  - Step 2: integrate BMI tracking with the menu calendar.

- [ ] Add vitamins and micronutrients
  - Decide later which vitamins and micronutrients should be tracked first.

- [ ] Add additional macronutrients
  - For example, fat, carbohydrates, and saturated fat.

- [ ] Add optional harmful substances or food safety notes
  - For example, warnings or tracked substances for products like tuna.

### Recipe and Calculation Improvements

- [ ] Add recipe categories
  - Categories are not needed in the current milestone.
  - For now, recipe pages may show a placeholder such as `Category: Future idea`.

- [ ] Rename recipe `Servings` to `TotalServings`
  - `Servings` currently means the total number of servings in a recipe.
  - A clearer name can be introduced later when the recipe core model is cleaned up further.

- [ ] Add recipe versioning
  - Preserve recipe versions so prepared batches and published recipes can point to a stable recipe version.

- [ ] Add optional recipe ingredients
  - Allow a recipe to include ingredients that are not required.

- [ ] Add recipe ingredient choice groups
  - Allow choosing one ingredient from several alternatives.
  - Example: raisins, cranberries, or dried apricots.

- [ ] Improve recipe contents ordering UI
  - Allow moving recipe contents up and down with a better interface.
  - Consider drag-and-drop reordering.

- [ ] Replace recipe content type strings with safer values
  - Avoid comparing string values such as `Ingredient` and `Recipe` directly in Razor Pages.
  - Use an enum or shared constants when the flow becomes more complex.

- [ ] Add structured recipe instructions
  - Support multiple instruction blocks or ordered steps.

- [ ] Add recipe storage instructions
  - For example, how to store the prepared food.

- [ ] Add recipe expiration or best-before information
  - For example, how many days the prepared dish can be safely stored.

- [ ] Add recipe images

- [ ] Add recipe draft or publish workflow

- [ ] Add recipe type
  - For example, meal, sauce, preparation, component, or snack.

- [ ] Add recipe component snapshots
  - Preserve component recipe values so later edits do not silently change recipes that already use the component.

- [ ] Support serving-based recipe components
  - Allow adding another recipe as a component by servings instead of grams.

- [ ] Add recipe scaling
  - Allow scaling recipes up or down without changing the original recipe.
  - Support multipliers such as 1.5x or 2x.
  - Decide whether a recipe can be safely scaled.
  - In the future, some ingredients may need custom scaling behavior instead of simple multiplication.

- [ ] Add editable final recipe weight
  - By default, the system can estimate recipe weight from all ingredients and components.
  - The default estimate does not account for trimming, peeling, evaporation, or other preparation and cooking weight changes.
  - Allow users to override the estimated weight with their own final recipe weight.
  - Use the final recipe weight for more accurate per-gram recipe component calculations.

- [ ] Add advanced recipe validation
  - Prevent the same ingredient from being added more than once inside the same recipe.
  - Prevent indirect recipe component cycles.

### Menu Planning Improvements

- [ ] Add advanced prepared batch snapshot history
  - Preserve deeper source details, edit history, and recipe version references if the basic prepared batch snapshot model is not enough.

- [ ] Allow selecting specific meal dates for prepared batches
  - Open a calendar when creating a prepared batch.
  - Preselect meal dates from the recipe planning defaults.
  - Allow users to add or remove individual eating days.
  - Set planned days from the number of selected dates.
  - Calculate total servings from selected dates and servings per day.
  - Create or reuse daily menus only for the selected dates.

- [ ] Plan meals for families or multiple people
  - Review how servings, prepared batches, and daily plans should work for several people.

- [ ] Show when a prepared recipe batch will run out

- [ ] Show missing daily nutrition when prepared food runs out

### Sharing and Users

- [ ] Add private user-owned ingredients and recipes

- [ ] Add public ingredient catalog

- [ ] Allow users to request publishing private ingredients and recipes

- [ ] Add admin approval workflow for shared ingredients and recipes

- [ ] Preserve published recipe ingredients so later edits do not silently change published recipes

### General Improvements

- [ ] Add search and filters

- [ ] Improve UI

- [ ] Add automated tests for calculation logic
  - Test `RecipeCalculationService`.
  - Test `MenuCalculationService`.
  - Test prepared batch nutrition calculations.
  - Test daily menu totals.
  - Test edge cases such as empty recipes, zero values, recipe components, and prepared batch servings.
