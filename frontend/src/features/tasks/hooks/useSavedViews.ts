import { useState, useCallback } from 'react';
import type { SavedView, TaskFilter } from '../types/savedView.types';

const STORAGE_KEY = 'taskflow:saved-views';

function loadFromStorage(): SavedView[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as SavedView[]) : [];
  } catch {
    return [];
  }
}

function saveToStorage(views: SavedView[]): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(views));
}

export function useSavedViews() {
  const [savedViews, setSavedViews] = useState<SavedView[]>(loadFromStorage);

  const persist = useCallback((next: SavedView[]) => {
    saveToStorage(next);
    setSavedViews(next);
  }, []);

  const saveView = useCallback(
    (name: string, filter: TaskFilter) => {
      const view: SavedView = {
        id: `${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
        name,
        filter,
        isPinned: false,
        createdAt: new Date().toISOString(),
      };
      persist([...loadFromStorage(), view]);
    },
    [persist],
  );

  const deleteView = useCallback(
    (id: string) => {
      persist(loadFromStorage().filter(v => v.id !== id));
    },
    [persist],
  );

  const pinView = useCallback(
    (id: string) => {
      persist(loadFromStorage().map(v => (v.id === id ? { ...v, isPinned: !v.isPinned } : v)));
    },
    [persist],
  );

  return { savedViews, saveView, deleteView, pinView };
}
