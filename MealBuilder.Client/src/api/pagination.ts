export const defaultPageSize = 24

export type PaginationQuery = {
  search?: string
  page?: number
  pageSize?: number
}

export type PagedResponse<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function createPaginationSearchParams(query: PaginationQuery) {
  const searchParams = new URLSearchParams()

  const normalizedSearch = query.search?.trim()

  if (normalizedSearch) {
    searchParams.set('search', normalizedSearch)
  }

  searchParams.set('page', String(query.page ?? 1))
  searchParams.set('pageSize', String(query.pageSize ?? defaultPageSize))

  return searchParams
}
