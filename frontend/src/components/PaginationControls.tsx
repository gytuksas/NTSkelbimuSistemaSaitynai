import { type ChangeEvent } from 'react'
import type { PageSizeOption } from '../hooks/usePagination'

interface PaginationControlsProps {
  page: number
  pageSize: PageSizeOption
  totalItems: number
  totalPages: number
  rangeStart: number
  rangeEnd: number
  pageSizeOptions: readonly number[]
  onPageChange: (page: number) => void
  onPageSizeChange: (size: number) => void
  className?: string
}

export const PaginationControls = ({
  page,
  pageSize,
  totalItems,
  totalPages,
  rangeStart,
  rangeEnd,
  pageSizeOptions,
  onPageChange,
  onPageSizeChange,
  className,
}: PaginationControlsProps) => {
  const isPrevDisabled = page <= 1
  const isNextDisabled = page >= totalPages || totalItems === 0
  const handlePageSizeChange = (event: ChangeEvent<HTMLSelectElement>) => {
    onPageSizeChange(Number(event.target.value))
  }

  return (
    <div className={className ? `pagination ${className}` : 'pagination'}>
      <p className="pagination__summary">
        {totalItems === 0 ? 'Įrašų nerasta' : `Rodoma ${rangeStart}–${rangeEnd} iš ${totalItems}`}
      </p>
      <div className="pagination__actions">
        <label>
          Puslapyje
          <select value={pageSize} onChange={handlePageSizeChange}>
            {pageSizeOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
        <div className="pagination__buttons">
          <button type="button" className="btn btn--ghost" onClick={() => onPageChange(page - 1)} disabled={isPrevDisabled}>
            Ankstesnis
          </button>
          <span className="pagination__status">
            Puslapis {Math.min(page, totalPages)} / {totalPages}
          </span>
          <button type="button" className="btn" onClick={() => onPageChange(page + 1)} disabled={isNextDisabled}>
            Kitas
          </button>
        </div>
      </div>
    </div>
  )
}
