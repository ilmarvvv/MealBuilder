# MealBuilder User Flows

## Purpose

This document defines the six core user-flow groups for the current MealBuilder design scope. It turns the Design Brief and Information Architecture into concrete sequences that can guide Wireframes, API planning, React implementation, and end-to-end verification.

It does not define the final visual layout or authorize source-code, database, or migration changes.

## Shared Flow Rules

- Keep the user's input after validation or network errors.
- Disable repeated submission while a request is in progress.
- Show field-level validation close to the relevant field.
- Ask before discarding a form only when unsaved changes exist.
- Do not create partial records when a multi-record operation fails.
- Reuse the same workflow when an action is opened from different entry points.
- Keep internal terms such as `PreparedRecipeBatch` out of user-facing text.

## Flow 1: Authentication and Onboarding

### Register

```text
Register
  -> Create Account
  -> Choose Target Setup
  -> Complete Onboarding
  -> Confirm Calorie Target
  -> Dashboard
```

Register collects:

- email;
- password;
- password confirmation.

The frontend validates the email, password requirements, and matching passwords before submission.

### Choose Target Setup

After registration, the user chooses one of two paths:

```text
Calculate for Me | Set Manually
```

`Calculate for Me` is the recommended default.

### Calculated Target

The calculation flow collects:

- date of birth;
- sex required by the selected calculation formula;
- height in centimeters;
- weight in kilograms;
- activity level;
- goal: Lose Weight, Maintain Weight, or Gain Weight.

The result shows a recommended daily calorie target. The user may accept it or enter a custom target.

The recommendation must be described as an estimate rather than medical advice. The exact formula and validation limits require a separate evidence-based technical review before implementation.

### Manual Target

The manual path asks only for a daily calorie target in kcal. Other profile data may be completed later in Account.

### Incomplete Onboarding

Onboarding is complete only when a calorie target has been saved. A user who leaves before completion returns to Onboarding after the next Login rather than entering the authenticated application without a target.

### Login

```text
Login
  -> Enter email and password
  -> Submit
  -> Return to requested protected page or Dashboard
```

An unsuccessful Login preserves the email and shows a general authentication error without revealing whether an account exists.

### Logout

```text
User Menu
  -> Logout
  -> End Session
  -> Login
```

Protected pages must no longer be accessible after Logout.

## Flow 2: Use or Create an Ingredient

### Add an Existing Ingredient to a Day

The flow may start from Dashboard or Planner:

```text
Dashboard -> Add Food
Planner -> Select Day -> Add Food
```

Both entry points open the same Planner flow:

```text
Add Food
  -> Ingredients
  -> Search or Browse
  -> Select Ingredient
  -> Enter Grams
  -> Review Nutrition Preview
  -> Add
  -> Update Daily Plan and Nutrition Totals
```

Quantity must be greater than zero. Adding the first item to an empty date creates its Daily Plan. Merely viewing an empty date does not create a record.

After success, the interface offers `Add Another` for fast repeated entry.

### Create a Personal Ingredient

The user may start from Library or from an unsuccessful Ingredient search:

```text
Library -> Ingredients -> Add Ingredient
```

```text
Add Food -> No Suitable Ingredient -> Create Personal Ingredient
```

The form collects:

- name;
- calories per 100 g;
- protein per 100 g;
- fat per 100 g;
- carbohydrates per 100 g;
- sugars per 100 g;
- fiber per 100 g;
- salt per 100 g.

Nutrition values default to zero and cannot be negative. Sugars cannot exceed carbohydrates. Duplicate names are allowed.

The created Ingredient is private, labeled `Mine`, and available to the owner in Library, Recipes, and Planner.

When creation starts from Add Food, success returns the user to that workflow with the new Ingredient selected.

## Flow 3: Create a Recipe

The current Recipe scope supports Ingredients and Cooking Steps. It does not support a Recipe inside another Recipe.

```text
Library
  -> Recipes
  -> Add Recipe
  -> Enter Details
  -> Add Ingredients
  -> Add Cooking Steps
  -> Review Nutrition
  -> Save
  -> Recipe Details
```

### Details

Collect:

- name;
- optional description;
- servings, defaulting to one.

### Ingredients

For each Ingredient:

```text
Search Library
  -> Select Ingredient
  -> Enter Grams
  -> Add to Recipe
```

The user may change the amount or remove the Ingredient. If no suitable Ingredient exists, the user may create a personal Ingredient without losing the Recipe form.

### Cooking Steps

The form begins with one empty Cooking Step. The user may:

- enter its instruction;
- add more steps;
- edit steps;
- reorder steps;
- delete a step when more than one exists.

A saved Recipe must contain at least one non-empty Cooking Step. Cooking time, temperature, images, and timers remain outside the current scope.

### Nutrition

The form shows live total and per-serving values for calories, protein, fat, carbohydrates, sugars, fiber, and salt.

### Result

Recipe Details provides:

- Details;
- Ingredients;
- ordered Cooking Steps;
- total nutrition;
- nutrition per serving;
- Edit;
- Prepare Recipe;
- Delete.

The Recipe remains private. Public submission and moderation remain future work.

## Flow 4: Prepare a Recipe

The user starts from Recipe Details:

```text
Recipe Details
  -> Prepare Recipe
  -> Review Preparation and Planning
  -> Prepare
  -> Planner
```

Prepare Recipe is available only when the Recipe has a valid name, servings greater than zero, at least one Ingredient, and at least one Cooking Step.

### Form Defaults

The form provides:

- prepared date, defaulting to today;
- total portions, defaulting to Recipe servings;
- `Automatically Plan Portions`, enabled by default;
- start date, defaulting to prepared date;
- number of days, defaulting to one.

### Automatic Planning

The system previews the proposed portion distribution before confirmation. When portions do not divide evenly, whole portions are distributed as evenly as possible, and the preview remains editable.

### Flexible Planning

The user may disable automatic planning. All portions then remain available and may be assigned to dates later.

### Result

Confirmation creates a Prepared Meal snapshot. Later Recipe changes do not silently change it.

Allocated portions appear in the selected Daily Plans. Unallocated portions remain available. If any part of the operation fails, the system must not leave a partially created Prepared Meal or partial allocations.

`PreparedRecipeBatch` remains the internal technical term. The UI uses `Prepared Meal`, `Available Portions`, and `portions left`.

## Flow 5: Adjust, Move, or Remove Food

The current scope uses one editable amount rather than separate Planned and Eaten values. The user may adjust it to match what was actually consumed.

### Change Amount

Ingredient amounts are changed in grams.

Prepared Meal amounts are changed in portions:

- increasing an amount consumes available portions;
- decreasing an amount returns portions to the available amount;
- an increase fails clearly when insufficient portions remain.

### Move

```text
Move
  -> Select Destination Date
  -> Select Full or Partial Amount
  -> Confirm
```

Moving a Prepared Meal allocation changes its date without changing the available amount. When the same source item already exists on the destination date, amounts are combined while meal categories remain outside the scope.

Move must behave as one complete operation. A failure leaves the original item unchanged.

### Remove

Removing an Ingredient deletes its Daily Plan Item. Removing Prepared Meal portions returns them to the available amount.

The interface provides a short Undo action instead of interrupting every removal with a confirmation dialog.

Every successful adjustment updates the selected Daily Plan, daily nutrition, available portions, Dashboard preview, and weekly summary.

## Flow 6: Review Daily and Weekly Results

### Daily Result

Dashboard and the selected Planner day show:

- calories and progress toward the saved calorie target;
- protein;
- fat;
- carbohydrates;
- sugars;
- fiber;
- salt.

An empty day displays `No food added for this day` and an Add Food action. It must not be presented as a tracked day with zero calories.

### Weekly Inclusion

Each non-empty Daily Plan provides one setting:

```text
Include this day in weekly summary
```

The setting is enabled by default. Empty days are excluded automatically. A manually excluded day remains visible with all its data and is labeled `Excluded from weekly summary`.

Estimated calories and separate Include and Exclude checkboxes remain outside the current scope.

### Weekly Calculations

Weekly total and average use only included non-empty days.

```text
Weekly average =
nutrition total of included non-empty days
/
number of included non-empty days
```

The summary shows:

- all seven days;
- included, excluded, and empty states;
- weekly nutrition total;
- average per included day;
- included day count, for example `4 of 7 days included`.

Dashboard shows a compact preview. Planner provides details and allows the user to open any day.

## Completion Criteria

User Flows are complete when Wireframes can represent all six groups without inventing new pages, when every core action has a clear success and failure destination, and when future capabilities such as public publishing, AI assistance, nested Recipes, recipe-derived Ingredients, estimated calorie days, and detailed nutrition targets remain outside the current implementation path.
