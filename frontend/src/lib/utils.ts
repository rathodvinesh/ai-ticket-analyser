import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function getPriorityBadgeColor(priority: string) {
  switch (priority?.toLowerCase()) {
    case 'high':
      return 'bg-red-500/10 text-red-400 border-red-500/30 glow-red';
    case 'medium':
      return 'bg-amber-500/10 text-amber-400 border-amber-500/30 glow-amber';
    case 'low':
      return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30 glow-emerald';
    default:
      return 'bg-slate-500/10 text-slate-400 border-slate-500/30';
  }
}

export function getCategoryBadgeColor(category: string) {
  switch (category?.toLowerCase()) {
    case 'bug':
      return 'bg-rose-500/10 text-rose-300 border-rose-500/30';
    case 'billing':
      return 'bg-purple-500/10 text-purple-300 border-purple-500/30';
    case 'login':
      return 'bg-blue-500/10 text-blue-300 border-blue-500/30';
    case 'feature request':
    case 'feature':
      return 'bg-teal-500/10 text-teal-300 border-teal-500/30';
    default:
      return 'bg-indigo-500/10 text-indigo-300 border-indigo-500/30';
  }
}
