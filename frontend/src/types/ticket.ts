export interface TicketRequest {
  title: string;
  description: string;
}

export interface TicketResponse {
  summary: string;
  category: 'Billing' | 'Login' | 'Bug' | 'Feature Request' | 'Other' | string;
  priority: 'High' | 'Medium' | 'Low' | string;
}

export interface AnalyzedTicketItem extends TicketResponse {
  id: string;
  title: string;
  description: string;
  timestamp: string;
}
