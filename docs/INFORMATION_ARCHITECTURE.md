# MealBuilder Information Architecture

## Purpose

This document defines how the current MealBuilder experience is organized. It describes the main sections, their responsibilities, the relationships between screens, and the agreed user-facing Prepared Meal workflow.

It complements `DESIGN_BRIEF.md`. It does not define database schema or authorize implementation changes.

## Primary Navigation

The authenticated application has three primary destinations:

1. Dashboard
2. Planner
3. Library

Account actions remain in the user menu rather than occupying primary navigation space.

```text
MealBuilder
|-- Dashboard
|-- Planner
|-- Library
|   |-- Ingredients
|   `-- Recipes
`-- User Menu
    |-- Account
    `-- Logout
```

### Section Responsibilities

| Section | Responsibility |
| --- | --- |
| Dashboard | Summarize today and the current week, then direct the user to the appropriate workflow. |
| Planner | Select dates, manage Daily Plans, allocate food, and review detailed daily or weekly results. |
| Library | Find, review, create, and reuse Ingredients and Recipes. |
| Account | Display account information and manage appearance preferences. |

Dashboard answers, "How am I doing today and this week?"

Planner answers, "What is planned for a date, and how do I change it?"

## Application Shell

### Desktop

Desktop uses a persistent top navigation bar.

```text
MealBuilder    Dashboard    Planner    Library    Theme    User Menu
```

The brand appears on the left, primary navigation appears in the main header area, and theme and account actions appear on the right.

### Mobile

Mobile uses a compact top bar and persistent bottom navigation.

The top bar contains:

- MealBuilder brand;
- theme action;
- user menu action.

The bottom navigation contains labeled destinations:

- Dashboard;
- Planner;
- Library.

Icons must include text labels. The active destination must use more than color alone to communicate its state.

### Authentication

Login and Register use a separate minimal layout without the authenticated application shell.

After Register, the user completes Onboarding and confirms a calorie target before entering Dashboard. A returning user with incomplete Onboarding resumes it after Login.

## Dashboard

Dashboard is an overview and entry point. It must not duplicate the complete Planner editor.

### Today's Nutrition

Show:

- calories and progress toward the current goal;
- protein;
- fat;
- carbohydrates;
- sugars;
- fiber;
- salt.

Calories and the three primary macronutrients receive the strongest visual emphasis. Sugars, fiber, and salt remain visible in a more compact presentation.

### Today's Plan Preview

Show:

- the number of planned items;
- a short list of today's Ingredients or Prepared Meals;
- grams or portions;
- a compact nutrition summary.

Primary actions:

- Add Food;
- Open Planner.

Add Food opens the Planner workflow for today rather than creating a separate Dashboard editor.

### Weekly Summary

Show:

- all seven days of the current week;
- which days are included, excluded, or empty;
- a compact nutrition result;
- how many days are included;
- an action to open detailed weekly information in Planner.

An incomplete week must be labeled clearly, for example, `4 of 7 days included`.

## Planner

Planner combines Calendar navigation with the selected Daily Plan.

### Week Navigation

Show:

- previous week;
- current week range;
- next week;
- seven selectable days.

Selecting a date opens its existing or empty Daily Plan. Viewing an empty date does not create a stored Daily Plan. It is created only after the user adds an item or explicitly saves a change.

### Selected Day

Show:

- date;
- optional Daily Plan name;
- optional description;
- compact nutrition summary;
- `Include this day in weekly summary` for a non-empty day;
- planned food items;
- Add Food action.

The weekly-summary setting is enabled by default. Empty days are excluded automatically. A manually excluded day remains visible with all its data but does not affect weekly totals or averages.

### Daily Plan Items

A Daily Plan may contain:

- an individual Ingredient measured in grams;
- portions from a Prepared Meal.

Each item shows:

- name;
- item type;
- grams or portions;
- nutrition contribution;
- Change Amount action;
- Move action;
- Remove action.

Move transfers the item to another date without requiring the user to remove and recreate it.

Reducing or removing Prepared Meal portions returns them to the available amount. Increasing the amount uses available portions. The system must not silently create portions when the available amount is insufficient.

### Add Food

Add Food provides two sources:

```text
Ingredients | Available Portions
```

For an Ingredient, the user selects a record and enters grams.

For a Prepared Meal, the user selects an available preparation and enters portions.

The flow shows a nutrition preview before the item is added.

### Weekly Details

Planner provides the detailed weekly view, including:

- included, excluded, and empty day states;
- nutrition totals by included day;
- totals and averages calculated from included non-empty days only;
- partial-week results;
- planned and empty days;
- navigation between dates.

Dashboard contains only the compact weekly preview.

## Prepared Meal Workflow

`PreparedRecipeBatch` remains the internal technical term. The UI uses user-facing language:

- Prepare Recipe;
- Prepared Meal;
- Available Portions;
- portions left.

### Default Automatic Planning

When preparing a Recipe, the form asks for:

- total portions;
- prepared date;
- start date;
- number of planned days.

`Automatically plan portions` is enabled by default. The system previews the proposed distribution before the user confirms it.

### Optional Flexible Planning

The user may disable automatic planning. All portions then remain available and may be assigned to dates later.

Automatic planning remains flexible after confirmation. The user can:

- increase or decrease the amount on a day;
- move portions to another day;
- remove portions from a day;
- leave portions without an assigned date.

### Technical Follow-up

Before implementation, the project plan and domain rules must be reviewed for:

- allocated and unallocated portions;
- optional automatic planning;
- movement between dates;
- fractional portion support;
- the role of `PlannedDays`;
- protection against allocating more portions than are available.

This review may require behavior or database changes and must be handled separately from this UX decision.

## Library

Library provides two tabs:

```text
Ingredients | Recipes
```

Search and primary actions apply to the selected tab. Ingredients and Recipes remain separate domain concepts and workflows.

### Ingredients

Filters:

```text
All | Built-in | Mine
```

Ingredient screens include:

- list;
- search;
- details;
- create;
- edit.

List items show the name, ownership type, calories per 100 g, and primary macronutrients. Details show calories, protein, fat, carbohydrates, sugars, fiber, and salt.

Edit and Delete actions appear only for Ingredients owned by the current user.

### Recipes

Recipe screens include:

- list;
- search;
- details;
- create;
- edit.

List items show the name, optional description, calories, primary macronutrients, servings, and Ingredient count.

Recipe creation and editing contain Details, Ingredients, and Cooking Steps. A Recipe contains Ingredients only and must have at least one non-empty ordered Cooking Step. Recipes inside other Recipes remain outside the current scope.

Recipe details show Ingredients and quantities, ordered Cooking Steps, total nutrition, nutrition per serving, and the Prepare Recipe action.

Prepared Meals do not become a third Library tab. A Prepared Meal is created from Recipe details and then managed through Planner.

```text
Library -> Recipe Details -> Prepare Recipe -> Planner
```

## Account

Profile and Settings are combined into one Account destination.

### Account Information

The current scope includes:

- email;
- account status;
- onboarding profile data when the calculated-target path was used;
- activity level and selected goal when available;
- current daily calorie target;
- actions to recalculate or manually change the calorie target;
- Logout action.

Changing profile data must not silently replace the saved calorie target. The user reviews and confirms any recalculated target.

Profile photos, public usernames, biographies, email changes, and account deletion remain outside the current scope.

### Appearance

The appearance control provides:

```text
System | Light | Dark
```

System follows the device preference and selects one of the two supported themes.

## Login

Login contains:

- email;
- password;
- Login action;
- link to Register;
- loading, validation, authentication, and network error states.

Password recovery remains outside the current scope.

Successful Login returns a fully onboarded user to the originally requested protected page or Dashboard. A user with incomplete Onboarding returns to Onboarding.

## Register

Register contains:

- email;
- password;
- password confirmation;
- Create Account action;
- link to Login;
- visible password requirements;
- loading, validation, and network error states.

Password confirmation is a client-side validation field and does not have to be sent to the API.

Successful Register creates the account and opens Onboarding rather than Dashboard.

## Onboarding

Onboarding is completed after the user saves a daily calorie target.

The first choice is:

```text
Calculate for Me | Set Manually
```

The calculated path collects date of birth, the sex required by the selected formula, height in centimeters, weight in kilograms, activity level, and goal. The result is an estimated calorie target that the user may accept or replace with a custom value.

The manual path collects only a daily calorie target in kcal. Other profile data may be completed later in Account.

The exact calculation formula and safe validation limits require an evidence-based technical review before implementation. The result must be presented as an estimate rather than medical advice.

## Complete Structure

```text
MealBuilder
|-- Authentication
|   |-- Login
|   |-- Register
|   `-- Onboarding
|-- Dashboard
|   |-- Today's Nutrition
|   |-- Today's Plan Preview
|   `-- Weekly Summary
|-- Planner
|   |-- Week Navigation
|   |-- Selected Daily Plan
|   |-- Available Portions
|   `-- Weekly Details
|-- Library
|   |-- Ingredients
|   |   |-- List
|   |   |-- Details
|   |   `-- Create or Edit
|   `-- Recipes
|       |-- List
|       |-- Details
|       |-- Create or Edit
|       |   |-- Details
|       |   |-- Ingredients
|       |   `-- Cooking Steps
|       `-- Prepare Recipe
`-- Account
    |-- Account Information
    |-- Calorie Target
    `-- Appearance
```

## Completion Criteria

Information Architecture is complete when every screen in the current design scope has one clear location, Onboarding establishes the calorie target used by Dashboard, Dashboard and Planner do not duplicate responsibilities, Ingredients and Recipes remain easy to find within Library, and the Prepared Meal workflow supports both automatic and flexible planning without exposing internal technical terminology to the user.
