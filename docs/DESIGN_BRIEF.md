# MealBuilder Design Brief

## Purpose

This document defines the product direction, target user, design principles, and boundaries for the current MealBuilder design work. It keeps the design process focused and provides a shared reference for later UX, UI, and implementation decisions.

## Product Purpose

MealBuilder helps people plan meals, create reusable ingredients and recipes, and understand daily and weekly nutrition totals without complex tables.

The product combines:

- a built-in database of common ingredients;
- private user-created ingredients and recipes;
- flexible meal planning;
- a personal calorie target selected during onboarding;
- clear daily and weekly nutrition summaries;
- future AI assistance that reduces manual work.

The core product must remain useful without AI. Once the core workflows work reliably, AI may assist with barcode scanning, nutrition-label recognition, product creation, data validation, and content review.

The primary product promise is:

> Add food quickly and immediately understand the result.

## Primary Target User

The primary user is an everyday person who:

- plans meals at home;
- wants to monitor calories and essential nutrition values;
- uses built-in ingredients and creates personal ingredients and recipes;
- wants flexibility without professional nutrition software complexity;
- values simplicity and expects AI assistance when a task would otherwise require significant manual work.

Fitness-focused users and home meal planners are included within this broader audience, but MealBuilder should not look or behave exclusively like a bodybuilding tracker.

## UX Priorities

1. Keep the product simple to understand.
2. Make common meal-planning actions fast.
3. Present the daily result clearly.
4. Provide a useful weekly summary.
5. Show detailed analytics only when the user asks for them.

## Visual Direction

The visual direction combines:

- 70% Warm Minimal;
- 25% Fitness Energy;
- 5% Nutrition Analytics.

The interface should feel clean, warm, focused, motivating, and trustworthy.

Warm Minimal provides the spacious layout, neutral surfaces, clear typography, and restrained decoration. Fitness Energy appears through orange accents, primary actions, progress indicators, goals, and active states. Nutrition Analytics appears through small charts, weekly summaries, and optional details rather than dominating the interface.

MealBuilder must support both light and dark themes. The primary color families are orange, black, white, and gray. Exact design tokens will be selected during UI Design System work.

## Design Principles

### 1. Simple by Default

Show only what is needed for the primary action. Keep one clear primary action, reduce unnecessary text, and prevent advanced options from interrupting the main workflow.

Evaluation question:

> Can the user understand what to do next without instructions?

### 2. Fast for Everyday Actions

Make repeated actions efficient, including finding an ingredient, adding it to a recipe or daily plan, reusing a meal, changing a quantity, and checking the daily result.

Evaluation question:

> Can the user complete a common action without unnecessary navigation?

### 3. Details on Demand

Prioritize calories, protein, fat, carbohydrates, sugars, fiber, and salt. Keep advanced charts, trends, and analytics available but collapsed or secondary until the user requests them.

Evaluation question:

> Can these details be hidden without losing the main meaning?

### 4. Flexible, but Not Complicated

Allow people to use built-in data, create personal ingredients and recipes, plan meals in their own way, and later use AI assistance. Additional capabilities must not complicate the basic workflow for everyone.

Evaluation question:

> Does this capability provide freedom without requiring every user to configure it?

### 5. Clear and Trustworthy

Clearly distinguish private and public content, built-in and user-created ingredients, verified and unverified data, manual and AI-assisted data, validation errors, and saved or unsaved changes. Color must not be the only way to communicate meaning.

Evaluation question:

> Can the user understand where the data came from and what is happening to it?

## Current Design Scope

The current design scope includes:

- light and dark themes;
- responsive navigation and application shell;
- Login and Register;
- required onboarding with calculated or manually selected calorie target;
- editable profile and calorie-target settings;
- Dashboard;
- daily and weekly nutrition summaries;
- the ability to exclude a non-empty day from weekly calculations;
- Ingredient list, search, details, and form;
- built-in and private user-created Ingredients;
- Recipe list, details, and form;
- adding Ingredients to a Recipe;
- ordered Cooking Steps with at least one step per Recipe;
- Calendar and Daily Plan;
- automatic or flexible Prepared Meal planning;
- adding an Ingredient or Prepared Meal to a day;
- basic Settings, including theme selection;
- loading, empty, error, validation, and success states;
- reusable buttons, inputs, cards, dialogs, and nutrition indicators.

## Outside the Current Design Scope

The following capabilities remain future work:

- barcode scanning;
- AI nutrition-label recognition;
- AI meal-photo recognition;
- user submissions to the public database;
- AI validation and Admin moderation workflows;
- Admin interfaces;
- Premium subscriptions and payments;
- blog and short nutrition content;
- dietary preferences and filtering;
- advanced analytics;
- detailed fat breakdown;
- advanced measurement units and conversions;
- Recipes inside other Recipes;
- converting a Recipe into a derived Ingredient;
- manually estimated calories for untracked days;
- personal nutrition targets beyond calories;
- social features.

Built-in shared Ingredients are part of the current scope. User submission, validation, and publication workflows are not.

Future capabilities should influence extensibility decisions, but they do not require dedicated screens during the current design work.

## Completion Criteria

This Design Brief is complete when it provides enough direction to create the Information Architecture, User Flows, Wireframes, UI Design System, high-fidelity mockups, responsive layouts, interface states, and final development handoff without expanding the agreed scope.
