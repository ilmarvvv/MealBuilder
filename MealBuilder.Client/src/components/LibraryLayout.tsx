import { NavLink, Outlet } from 'react-router'
import './LibraryLayout.css'

export default function LibraryLayout() {
  return (
    <section className="library-layout">
      <header className="library-layout__header">
        <div>
          <p className="library-layout__eyebrow">
            Your food collection
          </p>

          <h1>Library</h1>

          <p className="library-layout__description">
            Find and manage your Ingredients and Recipes.
          </p>
        </div>

        <nav
          className="library-tabs"
          aria-label="Library sections"
        >
          <NavLink
            className="library-tabs__item"
            to="/library/ingredients"
          >
            Ingredients
          </NavLink>

          <NavLink
            className="library-tabs__item"
            to="/library/recipes"
          >
            Recipes
          </NavLink>
        </nav>
      </header>

      <div className="library-layout__content">
        <Outlet />
      </div>
    </section>
  )
}