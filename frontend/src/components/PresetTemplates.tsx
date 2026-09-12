'use client';

import React from 'react';
import { AlertTriangle, CreditCard, Lock, Zap, Sparkles } from 'lucide-react';

export interface Template {
  title: string;
  description: string;
  categoryHint: string;
  icon: React.ReactNode;
}

interface PresetTemplatesProps {
  onSelectTemplate: (template: Template) => void;
}

export const PRESET_TEMPLATES: Template[] = [
  {
    title: 'Database Timeout Error on Checkout',
    description: 'Users are experiencing 504 gateway timeout errors when clicking Place Order on the checkout page. Intermittent CPU spike detected on SQL server cluster.',
    categoryHint: 'Bug',
    icon: <AlertTriangle className="w-4 h-4 text-rose-400" />,
  },
  {
    title: 'Invoice Payment Double Charged',
    description: 'Customer #9412 reported that invoice INV-2024-889 was processed twice on their credit card ending in 4921. Requesting immediate refund of $249.00.',
    categoryHint: 'Billing',
    icon: <CreditCard className="w-4 h-4 text-purple-400" />,
  },
  {
    title: 'OAuth 2.0 Google Login Loop',
    description: 'Signing in via Google SSO redirects back to the login screen without error message. Affected browser: Chrome v128 on macOS Sonoma.',
    categoryHint: 'Login',
    icon: <Lock className="w-4 h-4 text-blue-400" />,
  },
  {
    title: 'Request Dark Mode Support for Analytics Dashboard',
    description: 'We would love to see a native high-contrast dark theme option added to the CSV report builder tab to reduce eye strain during late shift monitoring.',
    categoryHint: 'Feature Request',
    icon: <Zap className="w-4 h-4 text-teal-400" />,
  },
];

export const PresetTemplates: React.FC<PresetTemplatesProps> = ({ onSelectTemplate }) => {
  return (
    <div className="space-y-2">
      <div className="flex items-center gap-1.5 text-xs font-semibold text-slate-400 uppercase tracking-wider">
        <Sparkles className="w-3.5 h-3.5 text-indigo-400" />
        <span>Quick Test Prompts</span>
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
        {PRESET_TEMPLATES.map((tmpl, idx) => (
          <button
            key={idx}
            type="button"
            onClick={() => onSelectTemplate(tmpl)}
            className="flex items-start gap-3 p-3 rounded-xl bg-slate-900/50 hover:bg-slate-800/60 border border-slate-800/80 hover:border-indigo-500/40 text-left transition-all group"
          >
            <div className="p-2 rounded-lg bg-slate-800 group-hover:bg-indigo-950/60 transition-colors">
              {tmpl.icon}
            </div>
            <div className="flex-1 min-w-0">
              <h4 className="text-xs font-semibold text-slate-200 group-hover:text-indigo-300 truncate">
                {tmpl.title}
              </h4>
              <p className="text-[11px] text-slate-400 line-clamp-1 mt-0.5">
                {tmpl.description}
              </p>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
};
