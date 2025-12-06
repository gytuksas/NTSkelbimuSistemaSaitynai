import { useEffect, useMemo, useState } from 'react'
import { createPortal } from 'react-dom'
import type { MouseEvent, ReactNode } from 'react'

export type ConfirmIllustration = 'trash' | 'building' | 'clock'

export type ConfirmModalProps = {
  open: boolean
  title: string
  description: ReactNode
  onConfirm: () => void | Promise<void>
  onCancel: () => void
  confirmLabel?: string
  cancelLabel?: string
  busy?: boolean
  tone?: 'default' | 'danger'
  illustration?: ConfirmIllustration
}

const illustrationMap: Record<ConfirmIllustration, ReactNode> = {
  trash: (
    <svg viewBox="0 0 160 160" role="presentation" aria-hidden="true">
      <defs>
        <linearGradient id="trashGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#f97316" />
          <stop offset="100%" stopColor="#ea580c" />
        </linearGradient>
      </defs>
      <path
        d="M100 44h24a4 4 0 0 1 0 8h-4.3l-6.4 76.5A12 12 0 0 1 101.3 140H58.7a12 12 0 0 1 -12-11.5L40.3 52H36a4 4 0 0 1 0-8h24l5.7-11.5A12 12 0 0 1 76.6 28h6.8a12 12 0 0 1 10.9 6.5z"
        fill="url(#trashGradient)"
        opacity="0.3"
      />
      <rect x="50" y="52" width="60" height="72" rx="12" fill="url(#trashGradient)" opacity="0.8" />
      <rect x="46" y="44" width="68" height="12" rx="6" fill="#fff" opacity="0.9" />
      <circle cx="80" cy="20" r="10" fill="#f97316" opacity="0.25">
        <animateTransform attributeName="transform" attributeType="XML" type="scale" from="0.9" to="1.05" dur="2.8s" repeatCount="indefinite" />
      </circle>
      <line x1="70" y1="64" x2="70" y2="116" stroke="#fff" strokeWidth="6" strokeLinecap="round" opacity="0.9" />
      <line x1="90" y1="64" x2="90" y2="116" stroke="#fff" strokeWidth="6" strokeLinecap="round" opacity="0.9" />
    </svg>
  ),
  building: (
    <svg viewBox="0 0 160 160" role="presentation" aria-hidden="true">
      <defs>
        <linearGradient id="buildingGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#38bdf8" />
          <stop offset="100%" stopColor="#6366f1" />
        </linearGradient>
      </defs>
      <rect x="30" y="28" width="52" height="104" rx="12" fill="url(#buildingGradient)" opacity="0.6" />
      <rect x="78" y="52" width="52" height="80" rx="12" fill="url(#buildingGradient)" opacity="0.9" />
      <rect x="44" y="44" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="66" y="44" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="44" y="66" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="66" y="66" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="92" y="66" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="114" y="66" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="92" y="88" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="114" y="88" width="12" height="12" rx="2" fill="#fff" opacity="0.85" />
      <rect x="101" y="108" width="14" height="24" rx="2" fill="#fff" opacity="0.95" />
      <circle cx="100" cy="30" r="12" fill="#38bdf8" opacity="0.3">
        <animateTransform attributeName="transform" type="translate" values="0 0; 0 -5; 0 0" dur="3.5s" repeatCount="indefinite" />
      </circle>
    </svg>
  ),
  clock: (
    <svg viewBox="0 0 160 160" role="presentation" aria-hidden="true">
      <defs>
        <linearGradient id="clockGradient" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stopColor="#f97316" />
          <stop offset="100%" stopColor="#ec4899" />
        </linearGradient>
      </defs>
      <circle cx="80" cy="80" r="54" fill="url(#clockGradient)" opacity="0.85" />
      <circle cx="80" cy="80" r="64" stroke="url(#clockGradient)" strokeWidth="4" fill="none" opacity="0.4" />
      <line x1="80" y1="80" x2="80" y2="42" stroke="#fff" strokeWidth="6" strokeLinecap="round">
        <animateTransform attributeName="transform" type="rotate" from="0 80 80" to="360 80 80" dur="10s" repeatCount="indefinite" />
      </line>
      <line x1="80" y1="80" x2="114" y2="80" stroke="#fff" strokeWidth="6" strokeLinecap="round">
        <animateTransform attributeName="transform" type="rotate" from="0 80 80" to="360 80 80" dur="60s" repeatCount="indefinite" />
      </line>
      <circle cx="80" cy="80" r="6" fill="#fff" />
    </svg>
  ),
}

export const ConfirmModal = ({
  open,
  title,
  description,
  onCancel,
  onConfirm,
  confirmLabel = 'Patvirtinti',
  cancelLabel = 'Atšaukti',
  busy = false,
  tone = 'danger',
  illustration = 'trash',
}: ConfirmModalProps) => {
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    setMounted(true)
    return () => setMounted(false)
  }, [])

  useEffect(() => {
    if (!open) return
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !busy) {
        onCancel()
      }
    }
    window.addEventListener('keydown', handleKeyDown)
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      window.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = previousOverflow
    }
  }, [open, busy, onCancel])

  const illustrationNode = useMemo(() => illustrationMap[illustration], [illustration])

  if (!open || !mounted || typeof document === 'undefined') {
    return null
  }

  const handleBackdropClick = (event: MouseEvent<HTMLDivElement>) => {
    if (event.target === event.currentTarget && !busy) {
      onCancel()
    }
  }

  const modal = (
    <div className="confirm-modal__backdrop" role="presentation" onMouseDown={handleBackdropClick}>
      <div className={`confirm-modal__card confirm-modal__card--${tone}`} role="dialog" aria-modal="true">
        <div className="confirm-modal__illustration">{illustrationNode}</div>
        <div className="confirm-modal__content">
          <p className="confirm-modal__eyebrow">Patvirtinkite veiksmą</p>
          <h3>{title}</h3>
          <div className="confirm-modal__description">{description}</div>
        </div>
        <div className="confirm-modal__actions">
          <button type="button" className="btn btn--ghost" onClick={onCancel} disabled={busy}>
            {cancelLabel}
          </button>
          <button type="button" className="btn" onClick={onConfirm} disabled={busy}>
            {busy ? 'Vykdoma...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )

  return createPortal(modal, document.body)
}
