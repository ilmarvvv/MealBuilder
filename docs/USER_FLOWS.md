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
- Use `PreparedRecipe` in code and `Prepared Recipe` in user-facing text.
- Derive ownership from the authenticated user; never accept `OwnerId` from client input.
- Treat another user's private entity id as `404 Not Found` without revealing that it exists.
- Allow Daily Plans to use only built-in Ingredients and private Ingredients or Prepared Recipes owned by the current user.

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

Quantity must be greater than zero. Adding the first item to an empty date creates its Daily Plan. Merely viewing an empty date does not create a record, and removing the final item removes the empty saved plan.

Direct Ingredient items use current live Ingredient nutrition values in the first version. Preserving historical values with an Ingredient snapshot remains a future improvement.

Planned time is optional. Daily Plan Items are ordered by planned time, items without a time appear last, and fixed meal categories are outside the current scope.

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

Prepared date may be in the past, today, or the future. It defines the earliest allowed allocation date. Start date cannot be earlier than prepared date.

The user may create and allocate a future Prepared Recipe immediately. Add Food and Move reject any destination date before its prepared date.

### Automatic Planning

The system previews the proposed portion distribution before confirmation. Portions are distributed as evenly as possible with at most two decimal places. Any rounding remainder is assigned deterministically so the allocations sum exactly to the total portions, and the preview remains editable.

### Flexible Planning

The user may disable automatic planning. All portions then remain available and may be assigned to dates later.

### Result

Confirmation creates a Prepared Recipe snapshot. Later Recipe changes do not silently change it.

The copied contents may be reviewed before confirmation. After creation, the Prepared Recipe snapshot is immutable; only its portion allocations may be adjusted, moved, or removed.

Start date and number of planned days are creation inputs only. The resulting allocations are stored through Daily Plans and Daily Plan Items.

Allocated portions appear in the selected Daily Plans. Unallocated portions remain available. If any part of the operation fails, the system must not leave a partially created Prepared Recipe or partial allocations.

The Domain, API, and UI use the same `PreparedRecipe` / `Prepared Recipe` concept. The UI also uses `Available Portions` and `portions left`.

### Delete Prepared Recipe

```text
Prepared Recipe
  -> Delete
  -> Review Permanent Deletion Warning
  -> Cancel or Confirm Delete
```

The warning shows the Prepared Recipe name, the number of affected Daily Plan Items and dates, and explains that affected daily and weekly nutrition totals will change.

Confirmation permanently deletes the Prepared Recipe, its snapshot ingredients, and all Daily Plan Items that reference it. Daily Plans left empty by the cascade are removed, while plans with other items remain. The source Recipe remains unchanged. The complete deletion is atomic.

## Flow 5: Adjust, Move, or Remove Food

The current scope uses one editable amount rather than separate Planned and Eaten values. The user may adjust it to match what was actually consumed.

### Change Amount

Ingredient amounts are changed in grams.

Prepared Recipe amounts are changed in portions. Fractional portions use at most two decimal places:

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

Moving a Prepared Recipe allocation changes its date without changing the total allocated or available portions. Full and partial moves preserve the original planned time. When the same source item with the same planned time already exists on the destination date, amounts are combined. Items with different planned times remain separate.

Move must behave as one complete operation. A failure leaves the original item unchanged. If a full move leaves the source Daily Plan empty, that empty plan is removed.

### Remove

Removing an Ingredient deletes its Daily Plan Item. Removing Prepared Recipe portions returns them to the available amount.

Removal is persisted immediately. The interface keeps the removed item details and provides Undo for 5 seconds instead of interrupting every removal with a confirmation dialog.

Undo re-adds the item with its original amount and planned time. Restoring Prepared Recipe portions requires enough available portions at that moment; otherwise Undo fails with a clear message. If removal deleted an empty Daily Plan, successful Undo recreates the plan with the restored item.

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
