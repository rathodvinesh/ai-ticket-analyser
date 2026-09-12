'use client';

import React, { useState } from 'react';
import { TicketResponse } from '../types/ticket';
import { getPriorityBadgeColor, getCategoryBadgeColor } from '../lib/utils';
import {
  Check,
  Copy,
  AlertOctagon,
  Tag,
  FileText,
  ShieldAlert,
  ShieldCheck,
  ShieldCheck as ShieldMedium,
  CheckCircle2,
  Sparkles,
} from 'lucide-react';

interface AnalysisResultProps {
  result: TicketResponse | null;
  isLoading: boolean;
}

export const AnalysisResult: React.FC<AnalysisResultProps> = ({ result, isLoading }) => {
  const [copied, setCopied] = useState(false);

  if (isLoading) {
    return (
      <div className="h-full flex flex-col items-center justify-center p-8 text-center space-y-4">
        <div className="relative">
          <div className="w-16 h-16 rounded-full border-4 border-indigo-500/20 border-t-indigo-500 animate-spin"></div>
          <Sparkles className="w-6 h-6 text-indigo-400 absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 animate-pulse" />
        </div>
        <div>
          <h3 className="text-base font-semibold text-slate-200">
            Processing Neural Classification
          </h3>
          <p className="text-xs text-slate-400 mt-1 max-w-xs">
            Querying Groq Llama-3.1 model to extract priority, domain category, and summary...
          </p>
        </div>
      </div>
    );
  }

  if (!result) {
    return (
      <div className="h-full flex flex-col items-center justify-center p-8 text-center border-2 border-dashed border-slate-800/80 rounded-2xl bg-slate-900/20">
        <div className="p-4 rounded-2xl bg-slate-900 border border-slate-800 text-slate-500 mb-3">
          <FileText className="w-8 h-8 stroke-1" />
        </div>
        <h3 className="text-sm font-medium text-slate-300">Ready for Analysis</h3>
        <p className="text-xs text-slate-500 mt-1 max-w-xs">
          Submit a ticket or choose a sample prompt to see AI classification insights in real-time.
        </p>
      </div>
    );
  }

  const handleCopy = () => {
    const textToCopy = `[AI Analysis] Category: ${result.category} | Priority: ${result.priority}\nSummary: ${result.summary}`;
    navigator.clipboard.writeText(textToCopy);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const getPriorityIcon = (priority: string) => {
    switch (priority?.toLowerCase()) {
      case 'high':
        return <ShieldAlert className="w-5 h-5 text-red-400" />;
      case 'medium':
        return <ShieldMedium className="w-5 h-5 text-amber-400" />;
      default:
        return <ShieldCheck className="w-5 h-5 text-emerald-400" />;
    }
  };

  return (
    <div className="space-y-4 animate-in fade-in duration-300">
      {/* Top Action Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 text-emerald-400" />
          <span className="text-xs font-semibold uppercase tracking-wider text-slate-300">
            Analysis Complete
          </span>
        </div>
        <button
          onClick={handleCopy}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-xs font-medium text-slate-300 transition-colors"
        >
          {copied ? (
            <>
              <Check className="w-3.5 h-3.5 text-emerald-400" />
              <span className="text-emerald-400">Copied!</span>
            </>
          ) : (
            <>
              <Copy className="w-3.5 h-3.5" />
              <span>Copy Analysis</span>
            </>
          )}
        </button>
      </div>

      {/* Grid Badges */}
      <div className="grid grid-cols-2 gap-3">
        {/* Priority Badge */}
        <div
          className={`p-4 rounded-xl border flex flex-col justify-between ${getPriorityBadgeColor(
            result.priority
          )}`}
        >
          <div className="flex items-center justify-between mb-2">
            <span className="text-[11px] font-bold uppercase tracking-wider opacity-80">
              Priority Level
            </span>
            {getPriorityIcon(result.priority)}
          </div>
          <div className="text-xl font-extrabold capitalize">{result.priority}</div>
        </div>

        {/* Category Badge */}
        <div
          className={`p-4 rounded-xl border flex flex-col justify-between ${getCategoryBadgeColor(
            result.category
          )}`}
        >
          <div className="flex items-center justify-between mb-2">
            <span className="text-[11px] font-bold uppercase tracking-wider opacity-80">
              Category
            </span>
            <Tag className="w-4 h-4 opacity-80" />
          </div>
          <div className="text-lg font-bold truncate capitalize">{result.category}</div>
        </div>
      </div>

      {/* Summary Box */}
      <div className="p-4 rounded-xl bg-slate-900/90 border border-slate-800 space-y-2">
        <div className="flex items-center gap-2 text-xs font-semibold text-indigo-400">
          <FileText className="w-4 h-4" />
          <span>Executive AI Summary</span>
        </div>
        <p className="text-sm text-slate-200 leading-relaxed bg-slate-950/60 p-3 rounded-lg border border-slate-800/80">
          "{result.summary}"
        </p>
      </div>

      {/* Recommended Next Action */}
      <div className="p-4 rounded-xl bg-gradient-to-br from-indigo-950/30 to-purple-950/20 border border-indigo-800/40 space-y-2">
        <div className="flex items-center gap-2 text-xs font-semibold text-purple-300">
          <AlertOctagon className="w-4 h-4 text-purple-400" />
          <span>Recommended Routing</span>
        </div>
        <p className="text-xs text-slate-300">
          {result.priority?.toLowerCase() === 'high'
            ? '⚠️ Route directly to Tier 3 Escalation Team & notify On-Call Engineer.'
            : result.priority?.toLowerCase() === 'medium'
            ? '⚡ Assign to Tier 2 Technical Support Queue (SLA target: 4 hours).'
            : '✅ Route to General Support Queue (Standard SLA: 24 hours).'}
        </p>
      </div>
    </div>
  );
};
