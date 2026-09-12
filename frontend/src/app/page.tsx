'use client';

import React, { useState, useEffect } from 'react';
import { Navbar } from '../components/Navbar';
import { PresetTemplates, Template } from '../components/PresetTemplates';
import { TicketForm } from '../components/TicketForm';
import { AnalysisResult } from '../components/AnalysisResult';
import { HistoryDrawer } from '../components/HistoryDrawer';
import { StatsOverview } from '../components/StatsOverview';
import { TicketRequest, TicketResponse, AnalyzedTicketItem } from '../types/ticket';
import { analyzeTicket, fetchDbTickets } from '../lib/api';
import { Sparkles, Terminal, Info } from 'lucide-react';

export default function Home() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [result, setResult] = useState<TicketResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [history, setHistory] = useState<AnalyzedTicketItem[]>([]);

  const mapDbTicket = (t: any): AnalyzedTicketItem => ({
    id: (t.ticketId ?? t.TicketId ?? Math.random()).toString(),
    title: t.rawTitle ?? t.RawTitle ?? 'Untitled Ticket',
    description: t.rawDescription ?? t.RawDescription ?? '',
    summary: t.aisummary ?? t.Aisummary ?? t.aiSummary ?? t.AISummary ?? 'No summary available',
    category: t.aicategory ?? t.Aicategory ?? t.aiCategory ?? t.AICategory ?? 'Other',
    priority: t.aipriority ?? t.Aipriority ?? t.aiPriority ?? t.AIPriority ?? 'Low',
    timestamp: (t.createdAt ?? t.CreatedAt)
      ? new Date(t.createdAt ?? t.CreatedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      : 'Just now',
  });

  const loadDbTickets = async () => {
    try {
      const dbTickets = await fetchDbTickets();
      if (dbTickets && dbTickets.length > 0) {
        const formatted = dbTickets.map(mapDbTicket);
        setHistory(formatted);
        return true;
      }
    } catch (err) {
      console.warn('DB ticket fetch error:', err);
    }
    return false;
  };

  // Load history from DB and localStorage on mount
  useEffect(() => {
    async function init() {
      const loadedFromDb = await loadDbTickets();
      if (!loadedFromDb) {
        try {
          const saved = localStorage.getItem('ticket_ai_history');
          if (saved) setHistory(JSON.parse(saved));
        } catch (err) {
          console.error('Failed to parse history from localStorage', err);
        }
      }
    }
    init();
  }, []);

  // Save history to localStorage
  const saveHistoryItem = (req: TicketRequest, res: TicketResponse) => {
    const newItem: AnalyzedTicketItem = {
      id: Date.now().toString(),
      title: req.title,
      description: req.description,
      summary: res.summary,
      category: res.category,
      priority: res.priority,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setHistory((prev) => {
      const updated = [newItem, ...prev.slice(0, 19)];
      try {
        localStorage.setItem('ticket_ai_history', JSON.stringify(updated));
      } catch (err) {
        console.error('Failed to save history', err);
      }
      return updated;
    });
  };

  const handleSelectTemplate = (template: Template) => {
    setTitle(template.title);
    setDescription(template.description);
  };

  const handleClearForm = () => {
    setTitle('');
    setDescription('');
    setResult(null);
  };

  const handleClearHistory = () => {
    setHistory([]);
    localStorage.removeItem('ticket_ai_history');
  };

  const handleSelectHistoryItem = (item: AnalyzedTicketItem) => {
    setTitle(item.title);
    setDescription(item.description);
    setResult({
      summary: item.summary,
      category: item.category,
      priority: item.priority,
    });
  };

  const handleSubmit = async (request: TicketRequest) => {
    setIsLoading(true);
    setResult(null);

    try {
      const res = await analyzeTicket(request);
      setResult(res);
      saveHistoryItem(request, res);
      await loadDbTickets();
    } catch (err: any) {
      console.error('Error analyzing ticket:', err);
      alert(`Failed to analyze ticket: ${err?.message || 'Check server connection'}`);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col bg-[#080c14] text-slate-100 selection:bg-indigo-500 selection:text-white">
      {/* Top Ambient Glow */}
      <div className="fixed top-0 left-1/2 -translate-x-1/2 w-[800px] h-[300px] bg-gradient-to-b from-indigo-600/15 via-purple-600/10 to-transparent blur-3xl pointer-events-none z-0" />

      {/* Navbar */}
      <Navbar analyzedCount={history.length} />

      {/* Main Content Container */}
      <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 py-8 space-y-8 relative z-10">
        {/* Banner */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between p-4 rounded-2xl glass-panel border border-indigo-500/20 bg-gradient-to-r from-indigo-950/40 via-slate-900/60 to-purple-950/40 gap-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-xl bg-indigo-500/20 border border-indigo-500/30 text-indigo-300">
              <Sparkles className="w-5 h-5 text-indigo-400" />
            </div>
            <div>
              <h2 className="text-sm font-semibold text-slate-100">
                Groq AI Llama-3.1 Automated Classification Engine
              </h2>
              <p className="text-xs text-slate-400">
                Instant priority triage, automated categorization, and executive summarization in under 500ms.
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2 text-xs text-slate-400 bg-slate-900/80 px-3 py-1.5 rounded-xl border border-slate-800 shrink-0">
            <Terminal className="w-4 h-4 text-emerald-400" />
            <span>API endpoint: <code className="text-indigo-300 font-mono">/api/Ticket/analyze</code></span>
          </div>
        </div>

        {/* Stats Overview */}
        <StatsOverview history={history} />

        {/* Workspace Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
          {/* Left Column: Ticket Input Workspace */}
          <div className="lg:col-span-7 space-y-6">
            <div className="glass-panel p-6 rounded-2xl border border-slate-800 space-y-6">
              <PresetTemplates onSelectTemplate={handleSelectTemplate} />

              <div className="border-t border-slate-800/80 pt-5">
                <TicketForm
                  onSubmit={handleSubmit}
                  isLoading={isLoading}
                  title={title}
                  setTitle={setTitle}
                  description={description}
                  setDescription={setDescription}
                  onClear={handleClearForm}
                />
              </div>
            </div>
          </div>

          {/* Right Column: AI Analysis Display */}
          <div className="lg:col-span-5">
            <div className="glass-panel p-6 rounded-2xl border border-slate-800 min-h-[460px] flex flex-col justify-between">
              <AnalysisResult result={result} isLoading={isLoading} />

              <div className="mt-6 pt-4 border-t border-slate-800/80 flex items-center justify-between text-[11px] text-slate-500">
                <div className="flex items-center gap-1">
                  <Info className="w-3.5 h-3.5 text-indigo-400" />
                  <span>Powered by Groq Cloud Platform</span>
                </div>
                <span>JSON Schema v1.0</span>
              </div>
            </div>
          </div>
        </div>

        {/* History Stream */}
        <HistoryDrawer
          history={history}
          onSelect={handleSelectHistoryItem}
          onClearHistory={handleClearHistory}
        />
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-800/80 py-6 text-center text-xs text-slate-500 relative z-10">
        <div className="max-w-7xl mx-auto px-4 flex flex-col sm:flex-row items-center justify-between gap-2">
          <span>AI Ticket Analyzer & Classification System</span>
          <span>Ready for 1-Click Render Deployment</span>
        </div>
      </footer>
    </div>
  );
}
