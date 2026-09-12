import { TicketRequest, TicketResponse } from '../types/ticket';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || '';

export async function analyzeTicket(request: TicketRequest): Promise<TicketResponse> {
  const response = await fetch(`${API_BASE_URL}/api/Ticket/analyze`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `API Error: ${response.status} ${response.statusText}`);
  }

  return response.json();
}

export async function fetchDbTickets(): Promise<any[]> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Ticket`);
    if (!response.ok) return [];
    return await response.json();
  } catch (err) {
    console.warn('Could not fetch DB tickets:', err);
    return [];
  }
}
