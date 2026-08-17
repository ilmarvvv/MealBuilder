import { Navigate, Route, Routes } from 'react-router'
import AuthenticatedRoute from './auth/AuthenticatedRoute'
import AppLayout from './components/AppLayout'
import LibraryLayout from './components/LibraryLayout'
import CreateIngredientPage from './pages/CreateIngredientPage'
import CreateRecipePage from './pages/CreateRecipePage'
import EditIngredientPage from './pages/EditIngredientPage'
import EditRecipePage from './pages/EditRecipePage'
import HomePage from './pages/HomePage'
import IngredientDetailsPage from './pages/IngredientDetailsPage'
import IngredientListPage from './pages/IngredientListPage'
import LoginPage from './pages/LoginPage'
import NotFoundPage from './pages/NotFoundPage'
import OnboardingPage from './pages/OnboardingPage'
import RecipeDetailsPage from './pages/RecipeDetailsPage'
import RecipeListPage from './pages/RecipeListPage'
import RegisterPage from './pages/RegisterPage'
import AccountPage from './pages/AccountPage'

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route element={<AuthenticatedRoute allowIncompleteOnboarding />}>
          <Route path="/onboarding" element={<OnboardingPage />} />
        </Route>

        <Route element={<AuthenticatedRoute />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/account" element={<AccountPage />} />

          <Route path="/library" element={<LibraryLayout />}>
            <Route index element={<Navigate to="ingredients" replace />} />
            <Route path="ingredients" element={<IngredientListPage />} />
            <Route path="recipes" element={<RecipeListPage />} />
            <Route path="ingredients/new" element={<CreateIngredientPage />} />
            <Route path="recipes/new" element={<CreateRecipePage />} />
            <Route path="recipes/:recipeId/edit" element={<EditRecipePage />} />
            <Route path="recipes/:recipeId" element={<RecipeDetailsPage />} />
            <Route
              path="ingredients/:ingredientId/edit"
              element={<EditIngredientPage />}
            />
            <Route
              path="ingredients/:ingredientId"
              element={<IngredientDetailsPage />}
            />
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App
