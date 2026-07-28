type ErrorListProps = {
  messages: string[]
}

export default function ErrorList({ messages }: ErrorListProps) {
  if (messages.length === 0) {
    return null
  }

  return (
    <div role="alert">
      <ul>
        {messages.map((message) => (
          <li key={message}>{message}</li>
        ))}
      </ul>
    </div>
  )
}
