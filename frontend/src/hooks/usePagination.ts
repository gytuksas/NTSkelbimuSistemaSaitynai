import { useCallback, useMemo, useState } from 'react'

export const PAGE_SIZE_OPTIONS = [10, 20, 30, 40, 50] as const
export type PageSizeOption = (typeof PAGE_SIZE_OPTIONS)[number]

export interface PaginationState<T> {
  page: number
  pageSize: PageSizeOption
  totalItems: number
  totalPages: number
  rangeStart: number
  rangeEnd: number
  pageItems: T[]
  pageSizeOptions: readonly number[]
  goToPage: (page: number) => void
  nextPage: () => void
  prevPage: () => void
  setPageSize: (size: number) => void
  reset: () => void
}

export const usePagination = <T,>(
  items: T[],
  initialPageSize: PageSizeOption = PAGE_SIZE_OPTIONS[0],
): PaginationState<T> => {
  const [pageSize, setPageSizeState] = useState<PageSizeOption>(initialPageSize)
  const [page, setPage] = useState(1)
  const totalItems = items.length
  const totalPages = Math.max(1, Math.ceil(Math.max(totalItems, 1) / pageSize))

  const clampPage = useCallback(
    (value: number) => {
      if (totalItems === 0) {
        return 1
      }
      return Math.min(Math.max(1, value), totalPages)
    },
    [totalItems, totalPages],
  )

  const setPageSize = useCallback((size: number) => {
    const normalized = (PAGE_SIZE_OPTIONS.find((option) => option === size) ?? PAGE_SIZE_OPTIONS[0]) as PageSizeOption
    setPageSizeState(normalized)
    setPage(1)
  }, [])

  const goToPage = useCallback(
    (nextPage: number) => {
      setPage(clampPage(nextPage))
    },
    [clampPage],
  )

  const nextPage = useCallback(() => {
    goToPage(page + 1)
  }, [goToPage, page])

  const prevPage = useCallback(() => {
    goToPage(page - 1)
  }, [goToPage, page])

  const reset = useCallback(() => {
    goToPage(1)
  }, [goToPage])

  const activePage = totalItems === 0 ? 1 : clampPage(page)

  const pageItems = useMemo(() => {
    if (totalItems === 0) {
      return []
    }
    const start = (activePage - 1) * pageSize
    return items.slice(start, start + pageSize)
  }, [items, activePage, pageSize, totalItems])
  const rangeStart = totalItems === 0 ? 0 : (activePage - 1) * pageSize + 1
  const rangeEnd = totalItems === 0 ? 0 : Math.min(totalItems, activePage * pageSize)

  return {
    page: activePage,
    pageSize,
    totalItems,
    totalPages,
    rangeStart,
    rangeEnd,
    pageItems,
    pageSizeOptions: PAGE_SIZE_OPTIONS,
    goToPage,
    nextPage,
    prevPage,
    setPageSize,
    reset,
  }
}
