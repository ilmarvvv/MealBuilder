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

Create a system that stores ingredients, recipes, prepared meals, and daily plans in one place.

The user will be able to add ingredients with nutrition values: calories, protein, fat, carbohydrates, sugars, fiber, and salt. In the current React migration scope, values are stored per 100 g.

Based on saved ingredients, the user will be able to create recipes with ordered cooking steps. A recipe can include ingredients, and the user can add them, remove them, or change their quantities. Nested recipes are not part of the current REST API and React scope.

The system will automatically recalculate total recipe values after each ingredient change.

For recipes, the user will be able to specify the number of portions. The system will calculate values for the whole recipe and for one portion.

The user will be able to create a Prepared Meal snapshot from a Recipe. Portions are planned automatically by default, but they may remain available and be allocated, adjusted, or moved manually.

Daily Plans can contain individual Ingredients or Prepared Meal portions. The system will show daily and weekly nutrition values, compare daily calories with the user's saved target, and exclude empty or manually disabled days from weekly calculations.

The main idea is to save Ingredient and Recipe data once, then reuse it through Recipes, Prepared Meals, and Daily Plans without manual recalculation.

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

A reusable food or drink record with nutrition values. It can represent a basic cooking ingredient, a packaged product, a drink, or ready-to-eat purchased food. For example: chicken, rice, egg, olive oil, milk, Coca-Cola, or a store-bought burger.

Responsibility:
- store base nutrition values for a product
- provide reusable nutrition data for recipes and daily plans
- support direct use in daily plans without requiring a recipe
- provide shared built-in ingredients and private user-created ingredients

Main data:
- name
- calories per 100 g
- protein per 100 g
- fat per 100 g
- carbohydrates per 100 g
- sugars per 100 g
- fiber per 100 g
- salt per 100 g
- owner id for user-created ingredients
- built-in source name, source code, and source version

Relationships:
- can be used in many recipes through `RecipeIngredient`
- can be used directly in daily plans through `DailyPlanItem`
- a user-created ingredient belongs to one application user

Notes:
- values are stored per 100 g
- ingredient quantities in recipes and daily plans are entered in grams
- nutrition values are non-nullable and default to 0
- values cannot be negative
- calories per 100 g are limited to 0-900
- protein, fat, carbohydrates, sugars, fiber, and salt per 100 g are limited to 0-100
- sugars cannot exceed carbohydrates
- ingredient names do not have to be unique
- built-in ingredients are shared and read-only for regular users
- user-created ingredients are private to their owner

### Recipe

A dish, preparation, or reusable recipe that can be cooked and used again. For example: salad, sauce, chicken with rice, or filling.

Responsibility:
- store basic recipe information
- group ingredients into a reusable dish or preparation
- store ordered cooking steps
- provide a base for nutrition calculations, portions, and prepared meals

Main data:
- name
- description
- servings

Relationships:
- has many `RecipeIngredient` records
- has many `RecipeStep` records
- can be used to create many prepared meals

Notes:
- recipe nutrition values are calculated from its ingredients
- the current REST API and React scope does not support a recipe inside another recipe
- the completed Razor Pages prototype may retain its legacy `RecipeComponent` implementation as a reference
- a recipe must contain at least one non-empty cooking step
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
- all current REST API and React recipe calculations use grams
- unit support, piece-based products, and milliliters can be added later

### RecipeStep

One ordered cooking instruction inside a `Recipe`.

Responsibility:
- connect one cooking instruction to one recipe
- preserve the order in which instructions should be followed

Main data:
- recipe id
- sort order
- instruction

Relationships:
- belongs to one `Recipe`

Notes:
- every recipe must contain at least one step
- an instruction cannot be empty or contain only whitespace
- step ordering must remain deterministic after add, move, or remove operations
- cooking time, temperature, images, and timers are outside the current scope

### DailyPlan

A food plan for one specific date.

It allows the user to plan prepared recipe batches and individual ingredients for a day and see the total nutrition values for that date.

Responsibility:
- represent one daily food plan
- group planned food items for a specific date
- provide the source for daily nutrition totals
- support calendar, weekly, and future monthly views

Main data:
- name
- date
- description
- include in weekly summary

Relationships:
- has many `DailyPlanItem` records

Notes:
- one `DailyPlan` represents one specific date
- the same date can have only one saved daily plan
- an empty date can be viewed from Calendar without immediately saving a record
- a daily plan is created only after the user adds an item or saves changes
- nutrition totals are calculated from daily plan items and are not stored directly
- a non-empty daily plan is included in weekly calculations by default
- the user may exclude a non-empty daily plan from weekly calculations without deleting its data

### DailyPlanItem

A single planned food item inside a `DailyPlan`.

In the current UI, it represents either an individual ingredient measured in grams or servings from a prepared recipe batch.

Responsibility:
- connect a daily plan with one planned food item
- store the planned amount for that item
- contribute nutrition values to the daily total

Main data:
- daily plan id
- item type
- ingredient id
- prepared recipe batch id
- servings count
- grams

Relationships:
- belongs to one `DailyPlan`
- can reference one `Ingredient`
- can reference one `PreparedRecipeBatch`
- can temporarily reference one legacy `Recipe`

Notes:
- `ItemType` determines which relationship and quantity field are used
- ingredient items use `IngredientId` and `Grams`
- prepared batch items use `PreparedRecipeBatchId` and `ServingsCount`
- the quantity must be greater than 0
- one item must reference only the entity required by its item type
- direct recipe items are no longer created by the UI and their legacy support should be removed later

### PreparedRecipeBatch

An editable snapshot of a recipe that represents one prepared or planned batch.

For example, the user can create a `Burger` recipe, create a batch with 8 servings, adjust its copied contents, and allocate those servings across consecutive daily plans.

Responsibility:
- represent a prepared copy of one recipe
- preserve copied recipe contents and nutrition values as snapshot data
- store the prepared date, optional allocation start date, total servings, and optional planned days
- track how many servings are allocated and how many remain

Main data:
- source recipe id
- recipe name snapshot
- cooked date
- allocation start date when automatic planning is used
- total servings
- planned days when automatic planning is used
- prepared batch snapshot items

Relationships:
- references one source `Recipe`
- has many `PreparedRecipeBatchItem` snapshot records
- can be referenced by many `DailyPlanItem` records

Notes:
- changing the source recipe does not silently change an existing batch snapshot
- the prepared date and allocation start date are separate concepts in the new workflow
- servings are distributed across consecutive dates only when automatic planning is enabled
- used servings are calculated from daily plan items that reference the batch
- remaining servings are calculated as total servings minus used servings
- nutrition values are calculated from stored batch snapshot items
- automatic portion planning is enabled by default but can be disabled
- servings may remain unallocated and be assigned to dates later
- allocated servings can be adjusted or moved between dates

## 4. Main Scenarios: What the User Can Do

This section describes the main user workflows. The exact UI and technical implementation may change later.

### Working With Ingredients

The user can view and reuse built-in ingredients.

The user can create, edit, and delete their own ingredients.

The user cannot edit or delete built-in ingredients or another user's ingredients.

The user can find built-in and personal ingredients and reuse them in a recipe or daily plan.

Different ingredients may have the same name because records are identified by id and ownership.

In the current React migration scope, all ingredient nutrition values are stored per 100 g and all ingredient quantities are entered in grams.

### Working With Recipes

The user can create a recipe.

The user can add ingredients to a recipe and specify their quantities.

The user can change an ingredient quantity in a recipe or remove an ingredient from a recipe.

The user can add, edit, reorder, and remove cooking steps.

Every recipe must contain at least one non-empty cooking step.

The system automatically recalculates recipe nutrition values after changes.

The user can save a recipe and reuse it later.

### Working With Prepared Meals

The user can create a prepared snapshot from a finished recipe.

Automatic portion planning is enabled by default. The user can disable it and keep all portions available for later allocation.

The user can adjust allocated portions, move full or partial amounts to another date, or return portions to the available amount.

### Working With Portions and Planning

The user can specify how many portions a dish is split into.

The user can see nutrition values for one portion.

When preparing a Recipe, the user can accept automatic planning across a selected number of days or keep the portions available.

Daily nutrition values are calculated from the portions and Ingredients allocated to that date.

### Working With Daily Plans

The user can open any date from Calendar and see an existing or empty daily plan.

The user can add prepared recipe batch servings or individual ingredients to the selected day.

The system creates a saved daily plan only after the user adds an item or saves changes.

The user can see total calories, protein, fat, carbohydrates, sugars, fiber, and salt for the day.

The user can change planned quantities or remove items from the daily plan.

The user can exclude a non-empty day from weekly calculations without deleting its data.

### Searching and Reusing Data

The user can search saved ingredients or recipes.

The user can reuse the same ingredients in different recipes.

The user can reuse finished recipes by creating prepared meals.

## 5. Business Rules: What Must Always Be True

This section describes rules that the system must follow regardless of the UI or implementation details.

### Ingredient Rules

Each ingredient must have a name.

The ingredient name cannot be empty.

Ingredient names do not have to be unique, including within one user's private ingredients.

Ingredient nutrition values are stored as base values for future calculations.

Calories, protein, fat, carbohydrates, sugars, fiber, and salt are non-nullable and default to 0.

Calories, protein, fat, carbohydrates, sugars, fiber, and salt cannot be less than 0.

Values measured per 100 g should not exceed physically possible limits. Calories per 100 g cannot be more than 900. Protein, fat, carbohydrates, sugars, fiber, and salt cannot be more than 100 g per 100 g of product.

Sugars per 100 g cannot exceed carbohydrates per 100 g.

Each ingredient is either built-in or user-created.

Built-in ingredients are available to every authenticated user and are read-only for regular users.

User-created ingredients belong to one user, and only their owner can view or change them.

Every built-in ingredient must preserve its external source name, source code, and source version.

A missing source nutrition value must not be silently converted to 0 when built-in data is prepared.

### Unit and Input Rules

All ingredient nutrition values are stored per 100 g in the current React migration scope.

All ingredient quantities in recipes and daily plans are entered and calculated in grams.

Milliliter, piece, slice, and custom measurement input are not supported in the current React migration scope.

The quantity of a product in a recipe or menu must be greater than 0.

### Calculations

The main formula for calculating a nutrition value for a specific product weight is: calculated value = value per 100 g * actual weight in grams / 100.

For example, if 100 g of chicken contains 23 g of protein and the recipe uses 250 g of chicken, the calculation is: protein = 23 * 250 / 100 = 57.5 g.

Total recipe nutrition values are the sum of all ingredient values.

The system should not change base ingredient values while calculating recipes or daily plans.

### Recipes

Each recipe must have a name.

A recipe can contain many ingredients.

A recipe must contain at least one ordered cooking step.

Each cooking-step instruction must contain non-whitespace text.

The quantity of each ingredient in a recipe must be greater than 0.

The current REST API and React scope does not support nested recipes or `RecipeComponent` operations.

### Portion and Planning Rules

The number of portions must be greater than 0.

The number of planned days must be greater than 0 when automatic portion planning is enabled.

Values for one portion are calculated as total recipe values divided by the number of portions.

Daily values for a prepared meal are calculated from the portions allocated to that date.

### Daily Plans

One saved `DailyPlan` can exist for each date.

A daily plan can contain individual ingredients or prepared recipe batches.

Direct recipes must be converted into prepared batches before they are added through the current UI.

The quantity of each daily plan item must be greater than 0.

A prepared batch item cannot use more servings than remain available in that batch.

Total daily plan values are the sum of all daily plan item values.

Opening an empty calendar date must not create a database record until the user saves a change or adds an item.

### Weekly Summary Rules

A non-empty daily plan is included in weekly calculations by default.

The user can exclude a non-empty daily plan without deleting or changing its daily data.

Empty and manually excluded days do not contribute to weekly totals or averages.

The weekly average for each nutrition value is the included-day total divided by the number of included non-empty days.

The weekly summary must show how many of the seven days are included.

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

- [x] Review Create/Edit/Details ingredient flow with conversion fields and notes
- [x] Review adding ingredients to recipes with grams
- [x] Review adding recipe components to recipes with grams
- [x] Review recipe totals and per-serving values
- [x] Review creating prepared batches from Recipe Details
- [x] Review prepared batch creation with preselected recipe, start date, planned days, and snapshot values
- [x] Review daily planning as one `DailyPlan` per date
- [x] Review adding direct ingredients to daily plans with grams
- [x] Review adding prepared batch servings to daily plans
- [x] Review calendar day creation and daily average behavior
- [x] Review safe delete behavior for used ingredients, recipes, and prepared batches
- [x] Update entity descriptions after domain changes
- [x] Update business rules after domain changes
- [x] Update implementation plan after each completed domain area
- [x] Review README and keep its current project title

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

- [x] Keep consecutive-day batch allocation for the current version
  - Use `CookedDate` as the allocation start date.
  - Distribute servings across consecutive days using `PlannedDays` and servings per day.
  - Keep selecting individual meal dates as a future improvement.

- [x] Add on-demand daily plan workflow from Calendar
  - Remove the separate Daily Plan column and use the date as the navigation link.
  - Open an existing daily plan by ID or show a virtual empty daily plan by date.
  - Do not save an empty daily plan when the user only views it.
  - Create the daily plan after the user successfully adds an ingredient or prepared batch, or saves other changes.
  - Adjust Edit and Delete actions for virtual daily plans that have not been saved yet.

### Milestone 13: Authentication and Data Ownership

Goal: make the Razor Pages application safe for multiple authenticated users with isolated private data.

#### 13.1 Authentication Foundation

- [x] Add ASP.NET Core Identity
- [x] Add user registration
- [x] Add login and logout
- [x] Add authenticated user navigation
- [x] Protect private application pages
- [x] Add an initial administrator account safely without hardcoded credentials

#### 13.2 Data Ownership and Authorization

- [x] Add ownership to user-created ingredients and recipes
- [x] Make prepared batches and daily plans belong to their creator
- [x] Reset the local database before applying the final ownership schema
- [x] Allow users to view and edit their own private data
- [x] Prevent users from viewing or changing another user's private data
- [x] Enforce ownership rules on the server, not only in the UI

#### 13.3 Verification and Documentation

- [x] Verify registration, login, logout, and protected page behavior
- [x] Verify ownership isolation between at least two users
- [x] Verify the clean database initialization workflow
- [x] Confirm that the project builds without warnings or errors
- [x] Update project documentation for the completed authentication and ownership scope

> [!NOTE]
> The Razor Pages prototype is complete and remains as a working reference. Further development will continue in the REST API and React application.

### Milestone 14: API and React Foundation

### Goal

Create the isolated foundation for the new REST API and React application without changing the completed Razor Pages prototype.

### Architecture Decisions

- Keep all projects in the existing `MealBuilder` solution and Git repository.
- Keep `MealBuilder.Web` unchanged as a working reference with its own database.
- Do not create project references between `MealBuilder.Web` and the new application projects.
- Use a separate SQLite database at `data/mealbuilder.db` for the new application.
- Build each feature completely through Domain, API, tests, and React before moving to the next feature.

### Target Structure

```text
MealBuilder/
|-- MealBuilder.slnx
|-- MealBuilder.Domain/
|-- MealBuilder.Infrastructure/
|-- MealBuilder.Api/
|-- MealBuilder.Api.Tests/
|-- MealBuilder.Client/
`-- data/
    `-- mealbuilder.db
```

Project responsibilities:

- `MealBuilder.slnx`
  - Combines all .NET projects in one solution.

- `MealBuilder.Domain`
  - Contains models and business rules.

- `MealBuilder.Infrastructure`
  - Contains database access, Entity Framework Core, Identity, and migrations.

- `MealBuilder.Api`
  - Contains the REST API used by the frontend.

- `MealBuilder.Api.Tests`
  - Contains automated tests for the API.

- `MealBuilder.Client`
  - Contains the React, TypeScript, and Vite frontend.

- `data/mealbuilder.db`
  - Stores the local SQLite database and must not be committed to Git.

### Required Sub-milestones

#### Milestone 14.1: Backend Project Structure

- [x] Create the `MealBuilder.Domain` class library
- [x] Create the `MealBuilder.Infrastructure` class library
- [x] Create the `MealBuilder.Api` ASP.NET Core Web API project
- [x] Create the `MealBuilder.Api.Tests` test project
- [x] Configure the required project references
- [x] Keep `MealBuilder.Web` isolated from the new projects

#### Milestone 14.2: Database and Identity Foundation

- [x] Configure the new `AppDbContext` in `MealBuilder.Infrastructure`
- [x] Configure ASP.NET Core Identity for the new application
- [x] Configure the separate `data/mealbuilder.db` SQLite database
- [x] Create and apply the initial migration
- [x] Keep the prototype database unchanged

#### Milestone 14.3: API Foundation

- [x] Configure API controllers, routing, and JSON responses
- [x] Use ASP.NET Core Identity cookie authentication for React-to-API requests
- [x] Configure authentication and authorization
- [x] Configure CORS with explicit frontend origins and credentials
- [x] Protect authenticated state-changing requests against CSRF
- [x] Resolve the current authenticated user in API requests
- [x] Add consistent validation and error responses
- [x] Add OpenAPI for development and manual API testing
- [x] Protect private endpoints from unauthenticated users

#### Milestone 14.4: React Foundation

- [x] Create `MealBuilder.Client` with React, TypeScript, and Vite
- [x] Configure routing and environment settings
- [x] Configure the API client for the chosen authentication approach
- [x] Implement registration, login, logout, and authenticated navigation
- [x] Add consistent loading, validation, and error states

#### Milestone 14.5: Foundation Verification

- [x] Configure the API integration test infrastructure
- [x] Add authentication and authorization integration tests
- [x] Confirm that the API and React client run independently
- [x] Confirm that authentication works through React
- [x] Confirm that `MealBuilder.Web` still works separately
- [x] Confirm that the solution builds without warnings or errors

### React UX/UI Design Phase

- [x] Define the visual direction as 70% Warm Minimal, 25% Fitness Energy, and 5% Nutrition Analytics
- [x] Define light, dark, and system themes with orange, black, white, and gray as the core palette
- [x] Define the desktop top navigation and mobile bottom navigation for Dashboard, Library, and Planner
- [x] Design the shared Library flow for Ingredients and Recipes
- [x] Design Dashboard, Planner, Add Food, and Weekly Summary layouts
- [x] Define the daily and weekly nutrition hierarchy for calories, protein, carbohydrates and sugars, fiber, fat, and salt
- [x] Design Ingredient and Recipe create and details workflows, including ordered Cooking Steps
- [x] Define the initial Cooked Recipe flow with a shared remaining amount and optional portion planning
- [x] Design Login, Register, guided and manual Onboarding, and Account Settings flows
- [x] Define shared loading, empty, validation, error, and success states
- [x] Define responsive behavior for desktop and mobile layouts

### Milestone 15: Ingredients Vertical Slice

### Goal

Complete the Ingredient workflow through Domain, persistence, REST API, automated tests, and React.

### Required Sub-milestones

#### Milestone 15.1: Ingredient Domain

- [x] Review the Ingredient fields and business rules before migration
- [x] Add the Ingredient model to `MealBuilder.Domain`
- [x] Add calories, protein, fat, carbohydrates, sugars, fiber, and salt per 100 g
- [x] Make nutrition values non-nullable with a default value of 0
- [x] Allow duplicate Ingredient names
- [x] Distinguish built-in Ingredients from user-created Ingredients
- [x] Keep all current Ingredient quantities grams-only
- [x] Add Ingredient business validation rules

#### Milestone 15.2: Ingredient Persistence

- [x] Add Ingredient Entity Framework Core configuration
- [x] Add Ingredients to `AppDbContext`
- [x] Configure built-in Ingredient source metadata and user-created Ingredient ownership
- [x] Preserve BLS source code and version for every seeded Ingredient
- [x] Seed 20 read-only Ingredients from BLS 4.0
  1. Chicken breast
  2. Ground beef
  3. Salmon
  4. Egg
  5. Milk
  6. Natural yogurt
  7. Quark
  8. White rice
  9. Oats
  10. Pasta
  11. Wheat bread
  12. Potato
  13. Wheat flour
  14. Lentils
  15. Olive oil
  16. Butter
  17. Tomato
  18. Onion
  19. Apple
  20. Banana
- [x] Match the starter names to exact BLS records and import only records with all seven required nutrition values
- [x] Add BLS 4.0 attribution under its CC BY 4.0 license
- [x] Create and apply the Ingredient migration

#### Milestone 15.3: Ingredient API

- [x] Add Ingredient request and response contracts
- [x] Return built-in Ingredients and the current user's Ingredients from list and details endpoints
- [x] Add create, update, and delete endpoints for user-created Ingredients
- [x] Prevent regular users from changing built-in Ingredients
- [x] Return appropriate validation errors and HTTP status codes
- [x] Enforce authentication and ownership in every endpoint

#### Milestone 15.4: Ingredient API Tests

- [x] Test successful Ingredient CRUD operations
- [x] Test default nutrition values and duplicate Ingredient names
- [x] Test that authenticated users can read built-in Ingredients
- [x] Test that regular users cannot update or delete built-in Ingredients
- [x] Test Ingredient validation errors
- [x] Test unauthenticated access
- [x] Test ownership isolation between two users

#### Milestone 15.5: Ingredient React Frontend

- [x] Add Ingredient frontend types and API functions
- [x] Implement Ingredient list, details, create, edit, and delete workflows in the shared Library layout
- [x] Distinguish built-in Ingredients from personal Ingredients in the UI
- [x] Show edit and delete actions only for the current user's Ingredients
- [x] Add loading, validation, empty, and error states
- [x] Confirm the complete Ingredient workflow works end to end

### Milestone 16: Recipes Vertical Slice

### Goal

Complete the Recipe workflow through Domain, persistence, REST API, automated tests, and React.

### Required Sub-milestones

#### Milestone 16.1: Recipe Domain

- [x] Review Recipe, RecipeIngredient, and RecipeStep rules before migration
- [x] Add Recipe, RecipeIngredient, and RecipeStep models to `MealBuilder.Domain`
- [x] Keep nested Recipes and `RecipeComponent` outside the current REST API and React scope
- [x] Add Ingredient and Cooking Step ordering rules
- [x] Add recipe nutrition calculations
- [x] Add Recipe business validation rules

#### Milestone 16.2: Recipe Persistence

- [x] Add Recipe Entity Framework Core configurations
- [x] Add Recipe data to `AppDbContext`
- [x] Create and apply the Recipe migration

#### Milestone 16.3: Recipe API

- [x] Add Recipe request, response, and nutrition contracts
- [x] Add Recipe CRUD endpoints
- [x] Add RecipeIngredient operations
- [x] Add ordered RecipeStep operations
- [x] Support creating and updating a complete recipe in one request
- [x] Enforce validation, dependency, and ownership rules

#### Milestone 16.4: Recipe API Tests

- [x] Test Recipe CRUD and complete recipe requests
- [x] Test Ingredient and Cooking Step operations
- [x] Test Cooking Step ordering and nutrition calculations
- [x] Test the minimum-one-step rule
- [x] Test validation and ownership isolation

#### Milestone 16.5: Recipe React Frontend

- [x] Add Recipe frontend types and API functions
- [x] Implement the full single-flow Recipe form
- [x] Add Details, Ingredients, and Cooking Steps sections
- [x] Support Ingredient and Cooking Step ordering and live nutrition summaries
- [x] Implement Recipe list, details, edit, and delete workflows
- [x] Integrate Recipe results into the shared Library search and type filters
- [x] Confirm the complete Recipe workflow works end to end

### Milestone 17: User Profile and Calorie Target

### Goal

Complete post-registration onboarding, user nutrition profile data, and a confirmed daily calorie target through persistence, REST API, automated tests, and React.

### Required Sub-milestones

#### Milestone 17.1: Profile and Calculation Rules

- [x] Review the profile storage model and privacy requirements before migration
  - Store one private `UserNutritionProfile` per user, separate from `ApplicationUser`.
  - Allow only the current user to access the profile, do not accept `UserId` from the client, and do not persist calculation previews.
- [x] Define required and optional data for calculated and manual target setup
  - Require `UserId` and `DailyCalorieTarget` in the saved profile.
  - Keep `BirthDate`, `SexForCalculation`, `HeightCm`, `WeightKg`, `ActivityLevel`, and `WeightGoal` optional in persistence so manual setup can save only a target.
  - Require all calculation inputs when the calculated setup path is used.
- [x] Limit the initial guided setup to adults aged 18 and over with metric age, height, and weight inputs
  - Support ages from 18 through 100, heights from 100 through 250 cm, and weights from 30 through 400 kg.
- [x] Define the initial activity-level and lose, maintain, or gain goal options
  - Use `LowActive` (1.4), `ModeratelyActive` (1.6), `Active` (1.8), and `VeryActive` (2.0).
  - Adjust maintenance calories by -10% for `LoseWeight`, 0% for `MaintainWeight`, and +10% for `GainWeight`.
- [x] Keep first-version user targets calories-only
  - Do not add saved targets for protein, fat, carbohydrates, sugars, fiber, or salt.
- [x] Select and document an evidence-based calorie calculation formula
  - Use the simplified Mifflin-St Jeor formula: `10 * weightKg + 6.25 * heightCm - 5 * age + 5` for male calculation and `10 * weightKg + 6.25 * heightCm - 5 * age - 161` for female calculation.
  - Calculate maintenance calories as resting energy expenditure multiplied by the selected EFSA physical activity level, then apply the selected weight-goal adjustment and round to the nearest whole kcal.
  - Sources: [Mifflin-St Jeor study](https://ajcn.nutrition.org/article/S0002-9165%2823%2916698-6/pdf) and [EFSA Dietary Reference Values for energy](https://www.efsa.europa.eu/sites/default/files/assets/DRV_Summary_tables_jan_17.pdf).
- [x] Define safe input validation limits and describe calculated results as estimates
  - Accept saved targets from 1,000 through 10,000 kcal and reject out-of-range calculated results instead of silently clamping them.
  - Present calculated results as estimates rather than medical advice.
  - Minimum-target reference: [NIDDK Body Weight Planner](https://www.niddk.nih.gov/bwp).
- [x] Require a confirmed daily calorie target before onboarding is complete
  - Treat the existence of a saved profile with a confirmed target as completed onboarding instead of persisting a separate completion flag.
- [x] Prevent profile changes from silently replacing the saved calorie target
  - Recalculation produces a preview, and the saved target changes only after explicit confirmation.

#### Milestone 17.2: Profile Persistence

- [ ] Add the chosen user profile, calorie target, and onboarding status persistence model
- [ ] Add Entity Framework Core configuration
- [ ] Create and apply the profile and calorie-target migration

#### Milestone 17.3: Profile API

- [ ] Add profile, target calculation, and saved-target contracts
- [ ] Add endpoints to read and update the current user's profile and calorie target
- [ ] Add a calculation-preview endpoint that does not silently save a target
- [ ] Return onboarding completion state with the authenticated user response
- [ ] Enforce authentication, validation, and ownership rules

#### Milestone 17.4: Profile API Tests

- [ ] Test calculated and manual target setup
- [ ] Test incomplete onboarding behavior
- [ ] Test calculation and input validation boundaries
- [ ] Test that profile changes do not silently change the saved target
- [ ] Test authentication and ownership isolation

#### Milestone 17.5: Profile React Frontend

- [ ] Add the calculated and manual onboarding paths after Register
- [ ] Implement the three-step guided flow: Body Information, Activity and Goal, and Daily Target
- [ ] Resume incomplete Onboarding after Login
- [ ] Add profile and calorie-target management to Account
- [ ] Add System, Light, and Dark appearance selection
- [ ] Allow the user to review and confirm a recalculated target
- [ ] Show the saved calorie target in Dashboard nutrition progress
- [ ] Confirm onboarding and target workflows work end to end

### Milestone 18: Meal Planning Vertical Slice

### Goal

Complete prepared batches, daily plans, and the calendar through Domain, persistence, REST API, automated tests, and React.

### Required Sub-milestones

#### Milestone 18.1: Meal Planning Domain

- [ ] Review PreparedRecipeBatch and DailyPlan rules before migration
- [ ] Add PreparedRecipeBatch and snapshot models to `MealBuilder.Domain`
- [ ] Add DailyPlan and DailyPlanItem models to `MealBuilder.Domain`
- [ ] Exclude the legacy direct Recipe relationship from the new `DailyPlanItem` model
- [ ] Add allocated and unallocated portion rules
- [ ] Keep the prepared amount available by default and support optional portion planning
- [ ] Support full and partial moves between dates
- [ ] Add daily-plan weekly-summary inclusion rules
- [ ] Add allocation, nutrition, and date validation rules

#### Milestone 18.2: Meal Planning Persistence

- [ ] Add Meal Planning Entity Framework Core configurations
- [ ] Add Meal Planning data to `AppDbContext`
- [ ] Create and apply the Meal Planning migration

#### Milestone 18.3: Meal Planning API

- [ ] Add PreparedRecipeBatch contracts and endpoints
- [ ] Add batch snapshot item operations
- [ ] Add DailyPlan and DailyPlanItem contracts and endpoints
- [ ] Create a prepared recipe batch and any optional planned allocations atomically
- [ ] Add full and partial move operations that preserve the original item on failure
- [ ] Return portions to the available amount after reduction or removal
- [ ] Add weekly Calendar and nutrition endpoints
- [ ] Exclude empty and manually disabled days from weekly totals and averages
- [ ] Enforce allocation, validation, and ownership rules

#### Milestone 18.4: Meal Planning API Tests

- [ ] Test prepared batch snapshot operations
- [ ] Test available-amount and optional planned-allocation operations
- [ ] Test adjust, move, remove, and insufficient-portion behavior
- [ ] Test atomic failure behavior for preparation and move operations
- [ ] Test daily-plan inclusion and weekly nutrition calculations
- [ ] Test validation and ownership isolation

#### Milestone 18.5: Meal Planning React Frontend

- [ ] Add Meal Planning frontend types and API functions
- [ ] Implement the user-facing Cooked Recipe and Available Amount workflows
- [ ] Keep the cooked amount available by default and provide an optional Plan Portions flow
- [ ] Implement the time-sorted Daily Plan without fixed meal sections, placing items without a time last
- [ ] Implement the two-step Add Food modal with combined Ingredient and Recipe search
- [ ] Implement change amount, full or partial move, remove, and Undo interactions
- [ ] Implement Dashboard daily and weekly previews
- [ ] Implement the weekly Planner and included-day nutrition summaries with one active calorie-target line
- [ ] Confirm the complete Meal Planning workflow works end to end

### Milestone 19: Final Transition

### Goal

Verify the new application, retire the completed Razor Pages prototype, and document the final project structure.

### Required Sub-milestones

#### Milestone 19.1: Final Application Verification

- [ ] Run all backend automated tests
- [ ] Verify authentication and ownership behavior end to end
- [ ] Verify Onboarding, calorie target, Ingredient, Recipe, Prepared Meal, Daily Plan, and Calendar workflows
- [ ] Verify responsive desktop and mobile layouts
- [ ] Confirm that the frontend and backend production builds succeed

#### Milestone 19.2: Razor Pages Prototype Retirement

- [ ] Confirm that the React application covers all required prototype workflows
- [ ] Confirm that no required code or data remains only in `MealBuilder.Web`
- [ ] Remove the `MealBuilder.Web` project from the solution and repository after explicit confirmation
- [ ] Make `MealBuilder.Api` the primary backend host

#### Milestone 19.3: Final Documentation

- [ ] Update the README and project setup instructions
- [ ] Document the final architecture and project responsibilities
- [ ] Update deployment configuration and documentation
- [ ] Update `docs/PROJECT_PLAN.md`

## 7. Future Ideas

This section contains ideas that may be useful for the project in the future, but are not part of the nearest implementation plan.

### Ingredient Improvements

- [ ] Add ingredient brand

- [ ] Add ingredient category

- [ ] Add ingredient descriptions
  - Store a longer user-facing description for ingredients.
  - Keep technical or personal notes separate from the public description.

- [ ] Add image support for ingredients
  - Allow up to 3 images per ingredient.
  - Private draft ingredients may have 0-3 images.
  - Published or shared ingredients should have 1-3 images.
  - The first image should be treated as the main image.
  - Decide later how images should be stored.

- [ ] Add advanced ingredient data provenance
  - Keep the required BLS source metadata for built-in Ingredients in the current scope.
  - Decide later how to represent manual input, package labels, AI suggestions, and additional external databases.

- [ ] Add advanced ingredient state handling
  - For example, raw, cooked, peeled, or trimmed product states.

- [ ] Add package-label-based nutrition input
  - Allow entering nutrition values for a custom reference amount and convert them to values per 100g.

- [ ] Improve ingredient search and display

- [ ] Allow creating a new ingredient from recipe ingredient selection
  - When adding an ingredient to a recipe and the needed ingredient does not exist, allow creating it without leaving the recipe flow.
  - After the ingredient is created, return to the recipe ingredient add flow with the new ingredient available for selection.

- [ ] Add fixed ingredient measurement conversions
  - Allow ingredient nutrition values to use either a per 100 g or per 100 ml basis.
  - Support gram, milliliter, piece, and slice input.
  - Add an optional grams-per-milliliter conversion when both mass and volume input are needed.
  - Add one optional piece conversion and one optional slice conversion per ingredient.
  - Convert every entered quantity to the ingredient nutrition basis before calculating nutrition values.

- [ ] Replace fixed ingredient conversions with flexible measurements
  - Introduce `IngredientMeasurement` records only when one piece and one slice conversion per ingredient are no longer sufficient.
  - Allow multiple named measurements such as piece, slice, bottle, can, cup, teaspoon, and tablespoon.
  - Map every measurement to the ingredient nutrition basis in grams or milliliters.
  - Support multiple package and portion sizes without duplicating ingredient nutrition values.

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

- [ ] Add goal pace to guided calorie target setup
  - Allow users with a `Lose weight` or `Gain weight` goal to choose a pace.
  - Adjust the suggested daily calorie target based on the selected pace.
  - Define safe pace options and limits before implementation.

- [ ] Add vitamins and micronutrients
  - Decide later which vitamins and micronutrients should be tracked first.

- [ ] Add detailed fat breakdown
  - Add saturated fat and unsaturated fat nutrition values.
  - Keep both values within the total fat value.

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

- [ ] Add recipe storage instructions
  - For example, how to store the prepared food.

- [ ] Add recipe expiration or best-before information
  - For example, how many days the prepared dish can be safely stored.

- [ ] Add recipe images

- [ ] Add recipe draft or publish workflow

- [ ] Add recipe type
  - For example, meal, sauce, preparation, component, or snack.

- [ ] Create a private Ingredient from a Recipe
  - Require a final prepared weight before calculating values per 100 g.
  - Create a recipe-derived Ingredient snapshot rather than a live nested Recipe relationship.
  - Preserve the source Recipe id and the source values used for conversion.
  - Do not silently update the derived Ingredient after the source Recipe changes.
  - Let the user explicitly refresh or recreate the derived Ingredient.

- [ ] Add recipe scaling
  - Allow scaling recipes up or down without changing the original recipe.
  - Support multipliers such as 1.5x or 2x.
  - Decide whether a recipe can be safely scaled.
  - In the future, some ingredients may need custom scaling behavior instead of simple multiplication.

- [ ] Add editable final recipe weight
  - By default, the system can estimate recipe weight from all ingredients.
  - The default estimate does not account for trimming, peeling, evaporation, or other preparation and cooking weight changes.
  - Allow users to override the estimated weight with their own final recipe weight.
  - Use the final recipe weight when creating a recipe-derived Ingredient with values per 100 g.

- [ ] Add advanced recipe validation
  - Prevent the same ingredient from being added more than once inside the same recipe.

### Menu Planning Improvements

- [ ] Add advanced prepared batch snapshot history
  - Preserve deeper source details, edit history, and recipe version references if the basic prepared batch snapshot model is not enough.

- [ ] Allow selecting specific meal dates for prepared batches
  - Open a calendar when creating a prepared batch.
  - Preselect meal dates from the recipe planning defaults.
  - Allow users to add or remove individual eating days.
  - Set planned days from the number of selected dates.
  - Calculate total servings from selected dates and servings per day.
  - Create or reuse daily plans only for the selected dates.

- [ ] Plan meals for families or multiple people
  - Review how servings, prepared batches, and daily plans should work for several people.

- [ ] Show when a prepared recipe batch will run out

- [ ] Show missing daily nutrition when prepared food runs out

### Advanced Sharing and Publishing

- [ ] Design direct public Ingredient and Recipe publishing after the core REST API and React frontend are complete
  - Start with `Private` and `Published` states.
  - Allow owners to publish valid content directly without administrator approval.
  - Make published content read-only.
  - Allow guests and authenticated users to view published content.

- [ ] Add automatic validation before publication
  - Require a valid name and nutrition values.
  - Require a published recipe to contain at least one ingredient and one non-empty cooking step.
  - Require valid quantities, servings, and nutrition calculations.
  - Require every public ingredient dependency to be published and immutable.

- [ ] Add safe unpublishing rules
  - Allow an owner to unpublish content only when it is not used by other published content.
  - Prevent unpublishing a dependency that is still used by a published recipe.
  - Decide whether unused content should return to `Private` or use a separate publication state.

- [ ] Allow users to save private copies of published recipes
  - Copy recipe core fields, ingredient rows, cooking steps, quantities, and positions.
  - Assign the copied recipe to the current user.
  - Ensure later edits to the private copy do not change the published recipe.
  - Decide when referenced ingredients should remain shared or receive private copies.

- [ ] Add reactive administrator controls for public content
  - Allow administrators to inspect public content and its dependencies.
  - Allow administrators to unpublish problematic ingredients or recipes.
  - Restrict administrative actions to the `Admin` role.

- [ ] Decide whether publication versioning is needed
  - Prefer the simpler `Private` and `Published` workflow first.
  - Add archived versions and publication lineage only if immutable version history becomes necessary.
  - Avoid duplicating complete records unless preserving an older public version is required.

- [ ] Add AI-assisted publication formatting
  - Let AI prepare a recipe publication draft in the required public format.
  - Require the user to review, edit, and confirm the result before direct publication.
  - Do not let AI silently change ingredients, quantities, servings, or nutrition values.

- [ ] Add reporting and automatic abuse detection for public content

- [ ] Add trusted-user or reputation-based publishing controls if direct publishing needs additional protection

- [ ] Add groups and group-specific administrators

- [ ] Add publication version history and audit information

### Content and Engagement

- [ ] Add a public blog

- [ ] Add short verified nutrition facts or tips
  - Show concise content without distracting from the main planning workflow.
  - Require reliable sources and review before publishing nutrition or health information.

### Food Capture and AI

- [ ] Add barcode scanning
  - Use a scanned barcode to find a matching product in a trusted internal or external database.
  - Let the user create or complete a product when no reliable match is found.

- [ ] Add AI-assisted meal photo recognition
  - Let AI suggest detected foods, portions, and estimated nutrition values from a meal photo.
  - Require the user to review and confirm all suggestions before saving them.
  - Treat photo-based nutrition values as estimates rather than verified data.

### Dietary Preferences and Restrictions

- [ ] Add dietary preferences such as vegan and vegetarian

- [ ] Add food and recipe filtering by dietary preference

- [ ] Consider personalized diet planning later
  - Keep simple preferences and filters separate from automatic diet-plan generation.

### Premium and Monetization

- [ ] Define free and Premium feature boundaries
  - Keep the core meal-planning workflow useful without a subscription.
  - Consider placing costly AI features and advanced analytics in Premium.

- [ ] Research subscription and payment implementation
  - Review payment fees, taxes, platform commissions, trial rules, and recurring billing requirements before implementation.

- [ ] Validate the initial pricing hypothesis
  - Possible monthly price: EUR 1.99.
  - Possible annual price: EUR 14.99.
  - Consider a three-month introductory free trial.
  - Treat these values as hypotheses until operating costs and user demand are understood.

### General Improvements

- [ ] Add search and filters

- [ ] Improve UI

- [ ] Add automated tests for calculation logic
  - Test `RecipeCalculationService`.
  - Test `MenuCalculationService`.
  - Test prepared batch nutrition calculations.
  - Test daily menu totals.
  - Test edge cases such as zero values, missing cooking steps, and prepared batch servings.
