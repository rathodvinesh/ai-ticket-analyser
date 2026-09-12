'use client';

import React from 'react';
import { AnalyzedTicketItem } from '../types/ticket';
import { ShieldAlert, Bug, CreditCard, Layers } from 'lucide-react';

interface StatsOverviewProps {
  history: AnalyzedTicketItem[];
}

export const StatsOverview: React.FC<StatsOverviewProps> = ({ history }) => {
  const total = history.length;
  const highCount = history.filter((h) => h.priority?.toLowerCase() === 'high').length;
  const bugCount = history.filter((h) => h.category?.toLowerCase() === 'bug').length;
  const billingCount = history.filter((h) => h.category?.toLowerCase() === 'billing').length;

  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
      <div className="glass-card p-4 rounded-xl flex items-center gap-3">
        <div className="p-2.5 rounded-xl bg-indigo-500/10 border border-indigo-500/20 text-indigo-400">
          <Layers className="w-5 h-5" />
        </div>
        <div>
          <p className="text-[11px] font-medium text-slate-400 uppercase tracking-wider">Total Tickets</p>
          <p className="text-xl font-extrabold text-slate-100">{total}</p>
        </div>
      </div>

      <div className="glass-card p-4 rounded-xl flex items-center gap-3">
        <div className="p-2.5 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400">
          <ShieldAlert className="w-5 h-5" />
        </div>
        <div>
          <p className="text-[11px] font-medium text-slate-400 uppercase tracking-wider">High Priority</p>
          <p className="text-xl font-extrabold text-red-400">{highCount}</p>
        </div>
      </div>

      <div className="glass-card p-4 rounded-xl flex items-center gap-3">
        <div className="p-2.5 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400">
          <Bug className="w-5 h-5" />
        </div>
        <div>
          <p className="text-[11px] font-medium text-slate-400 uppercase tracking-wider">Bugs Detected</p>
          <p className="text-xl font-extrabold text-rose-300">{bugCount}</p>
        </div>
      </div>

      <div className="glass-card p-4 rounded-xl flex items-center gap-3">
        <div className="p-2.5 rounded-xl bg-purple-500/10 border border-purple-500/20 text-purple-400">
          <CreditCard className="w-5 h-5" />
        </div>
        <div>
          <p className="text-[11px] font-medium text-slate-400 uppercase tracking-wider">Billing Issues</p>
          <p className="text-xl font-extrabold text-purple-300">{billingCount}</p>
        </div>
      </div>
    </div>
  );
};
