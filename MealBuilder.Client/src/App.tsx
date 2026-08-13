import { Navigate, Route, Routes } from 'react-router'
import AppLayout from './components/AppLayout'
import LibraryLayout from './components/LibraryLayout'
import HomePage from './pages/HomePage'
import IngredientDetailsPage from './pages/IngredientDetailsPage'
import IngredientListPage from './pages/IngredientListPage'
import LoginPage from './pages/LoginPage'
import NotFoundPage from './pages/NotFoundPage'
import RegisterPage from './pages/RegisterPage'
import CreateIngredientPage from './pages/CreateIngredientPage'
import EditIngredientPage from './pages/EditIngredientPage'
import RecipeListPage from './pages/RecipeListPage'
import CreateRecipePage from './pages/CreateRecipePage'

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route path="/library" element={<LibraryLayout />}>
          <Route
            index
            element={<Navigate to="ingredients" replace />}
          />
          <Route
            path="ingredients"
            element={<IngredientListPage />}
          />
          <Route
            path="recipes"
            element={<RecipeListPage />}
          />
          <Route
            path="ingredients/new"
            element={<CreateIngredientPage />}
          />
          <Route
            path="recipes/new"
            element={<CreateRecipePage />}
          />
          <Route
            path="ingredients/:ingredientId/edit"
            element={<EditIngredientPage />}
          />
          <Route
            path="ingredients/:ingredientId"
            element={<IngredientDetailsPage />}
          />
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App