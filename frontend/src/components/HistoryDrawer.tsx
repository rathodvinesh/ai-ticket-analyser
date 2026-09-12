'use client';

import React from 'react';
import { AnalyzedTicketItem } from '../types/ticket';
import { getPriorityBadgeColor, getCategoryBadgeColor } from '../lib/utils';
import { Clock, Trash2, ExternalLink, Download } from 'lucide-react';

interface HistoryDrawerProps {
  history: AnalyzedTicketItem[];
  onSelect: (item: AnalyzedTicketItem) => void;
  onClearHistory: () => void;
}

export const HistoryDrawer: React.FC<HistoryDrawerProps> = ({
  history,
  onSelect,
  onClearHistory,
}) => {
  const exportJSON = () => {
    const blob = new Blob([JSON.stringify(history, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `ticket_history_${new Date().toISOString().slice(0, 10)}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  if (history.length === 0) {
    return null;
  }

  return (
    <div className="glass-panel rounded-2xl p-6 border border-slate-800 space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Clock className="w-4 h-4 text-indigo-400" />
          <h3 className="text-sm font-semibold text-slate-200">Recent Analysis Stream</h3>
          <span className="px-2 py-0.5 rounded-full bg-slate-800 text-[11px] font-bold text-slate-400">
            {history.length}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={exportJSON}
            className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-slate-900 border border-slate-800 hover:bg-slate-800 text-slate-300 text-xs transition-colors"
            title="Export history to JSON"
          >
            <Download className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Export</span>
          </button>
          <button
            onClick={onClearHistory}
            className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-rose-950/40 border border-rose-900/40 hover:bg-rose-900/60 text-rose-300 text-xs transition-colors"
            title="Clear all history"
          >
            <Trash2 className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Clear</span>
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
        {history.map((item) => (
          <div
            key={item.id}
            onClick={() => onSelect(item)}
            className="glass-card p-4 rounded-xl space-y-3 cursor-pointer group"
          >
            <div className="flex items-start justify-between gap-2">
              <h4 className="text-xs font-semibold text-slate-200 group-hover:text-indigo-300 line-clamp-1">
                {item.title}
              </h4>
              <ExternalLink className="w-3.5 h-3.5 text-slate-500 group-hover:text-indigo-400 shrink-0" />
            </div>

            <p className="text-[11px] text-slate-400 line-clamp-2 leading-relaxed">
              "{item.summary}"
            </p>

            <div className="flex items-center justify-between text-[10px] pt-1">
              <span className={`px-2 py-0.5 rounded border ${getPriorityBadgeColor(item.priority)}`}>
                {item.priority}
              </span>
              <span className={`px-2 py-0.5 rounded border ${getCategoryBadgeColor(item.category)}`}>
                {item.category}
              </span>
              <span className="text-slate-500">{item.timestamp}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
