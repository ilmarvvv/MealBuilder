type LoadingIndicatorProps = {
  message?: string
}

export default function LoadingIndicator({
  message = 'Loading...',
}: LoadingIndicatorProps) {
  return (
    <p role="status" aria-live="polite">
      {message}
    </p>
  )
}
