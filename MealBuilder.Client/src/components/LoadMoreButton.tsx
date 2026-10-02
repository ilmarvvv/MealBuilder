import './LoadMoreButton.css'

type LoadMoreButtonProps = {
  isLoading: boolean
  onClick: () => void
}

export default function LoadMoreButton({
  isLoading,
  onClick,
}: LoadMoreButtonProps) {
  return (
    <div className="load-more">
      <button type="button" disabled={isLoading} onClick={onClick}>
        {isLoading ? 'Loading...' : 'Load More'}
      </button>
    </div>
  )
}
