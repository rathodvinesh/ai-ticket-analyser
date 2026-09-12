'use client';

import React from 'react';
import { Cpu, Sparkles, Activity, Layers } from 'lucide-react';

interface NavbarProps {
  analyzedCount: number;
}

export const Navbar: React.FC<NavbarProps> = ({ analyzedCount }) => {
  return (
    <header className="sticky top-0 z-50 glass-panel border-b border-slate-800/80 px-6 py-4">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        {/* Brand */}
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-gradient-to-tr from-indigo-600 via-indigo-500 to-purple-500 text-white shadow-lg shadow-indigo-500/20">
            <Cpu className="w-6 h-6 animate-pulse" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-xl font-bold bg-gradient-to-r from-white via-indigo-100 to-indigo-300 bg-clip-text text-transparent">
                TicketAI
              </h1>
              <span className="px-2 py-0.5 text-xs font-semibold rounded-full bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
                Llama 3.1 8B
              </span>
            </div>
            <p className="text-xs text-slate-400 hidden sm:block">
              Intelligent Support Ticket Classifier & Summarizer
            </p>
          </div>
        </div>

        {/* Right Badges */}
        <div className="flex items-center gap-4">
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-900/60 border border-slate-800 text-xs text-slate-300">
            <Activity className="w-4 h-4 text-emerald-400 animate-pulse" />
            <span>API Online</span>
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-400"></span>
          </div>

          <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-indigo-950/40 border border-indigo-800/40 text-xs font-medium text-indigo-300">
            <Layers className="w-4 h-4 text-indigo-400" />
            <span>Analyzed: </span>
            <span className="font-bold text-white">{analyzedCount}</span>
          </div>
        </div>
      </div>
    </header>
  );
};
