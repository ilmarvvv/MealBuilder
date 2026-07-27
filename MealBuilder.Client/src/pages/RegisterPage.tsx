import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth'

export default function RegisterPage() {
  const { user, isLoading, register } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setError(null)

    try {
      await register({ email, password })
      navigate('/')
    } catch {
      setError('Unable to create the account. Check the entered information.')
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <p>Loading...</p>
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return (
    <section>
      <h1>Register</h1>

      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div>
          <label htmlFor="password">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="new-password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {error && <p role="alert">{error}</p>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Creating account...' : 'Register'}
        </button>
      </form>

      <p>
        Already have an account? <Link to="/login">Login</Link>
      </p>
    </section>
  )
}