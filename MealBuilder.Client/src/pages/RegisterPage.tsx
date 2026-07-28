import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'

export default function RegisterPage() {
  const { user, isLoading, register } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrors([])

    try {
      await register({ email, password })
      navigate('/')
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to create the account. Check the entered information.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator />
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

        <ErrorList messages={errors} />

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
