'use client';

import React, { useState } from 'react';
import { Send, RotateCcw, AlertCircle, Sparkles } from 'lucide-react';
import { TicketRequest } from '../types/ticket';

interface TicketFormProps {
  onSubmit: (data: TicketRequest) => void;
  isLoading: boolean;
  title: string;
  setTitle: (val: string) => void;
  description: string;
  setDescription: (val: string) => void;
  onClear: () => void;
}

export const TicketForm: React.FC<TicketFormProps> = ({
  onSubmit,
  isLoading,
  title,
  setTitle,
  description,
  setDescription,
  onClear,
}) => {
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('Please enter a ticket title.');
      return;
    }
    if (!description.trim()) {
      setError('Please enter a ticket description.');
      return;
    }
    setError(null);
    onSubmit({ title: title.trim(), description: description.trim() });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <div className="flex items-center gap-2 p-3 rounded-lg bg-rose-500/10 border border-rose-500/30 text-rose-300 text-xs">
          <AlertCircle className="w-4 h-4 text-rose-400 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Ticket Title */}
      <div>
        <div className="flex justify-between items-center mb-1.5">
          <label className="text-xs font-semibold text-slate-300">
            Ticket Title <span className="text-indigo-400">*</span>
          </label>
          <span className="text-[11px] text-slate-500">{title.length}/150</span>
        </div>
        <input
          type="text"
          maxLength={150}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="e.g., Cannot reset password via email link"
          className="w-full px-4 py-3 rounded-xl bg-slate-900/90 border border-slate-700/80 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 text-slate-100 placeholder-slate-500 text-sm outline-none transition-all"
        />
      </div>

      {/* Ticket Description */}
      <div>
        <div className="flex justify-between items-center mb-1.5">
          <label className="text-xs font-semibold text-slate-300">
            Problem Description <span className="text-indigo-400">*</span>
          </label>
          <span className="text-[11px] text-slate-500">{description.length}/1000</span>
        </div>
        <textarea
          rows={5}
          maxLength={1000}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Describe the issue in detail, steps to reproduce, error codes, affected users..."
          className="w-full px-4 py-3 rounded-xl bg-slate-900/90 border border-slate-700/80 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 text-slate-100 placeholder-slate-500 text-sm outline-none transition-all resize-none"
        />
      </div>

      {/* Actions */}
      <div className="flex items-center gap-3 pt-2">
        <button
          type="submit"
          disabled={isLoading}
          className="flex-1 py-3 px-6 rounded-xl font-semibold text-sm text-white shimmer-button hover:opacity-95 shadow-lg shadow-indigo-500/25 flex items-center justify-center gap-2 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <Sparkles className="w-4 h-4 animate-spin text-white" />
              <span>Analyzing with Groq AI...</span>
            </>
          ) : (
            <>
              <Send className="w-4 h-4" />
              <span>Analyze Ticket</span>
            </>
          )}
        </button>

        <button
          type="button"
          onClick={onClear}
          disabled={isLoading || (!title && !description)}
          className="p-3 rounded-xl bg-slate-900 border border-slate-800 text-slate-400 hover:text-slate-200 hover:bg-slate-800 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
          title="Clear form"
        >
          <RotateCcw className="w-4 h-4" />
        </button>
      </div>
    </form>
  );
};
