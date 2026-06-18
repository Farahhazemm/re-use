const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ProductBriefDto {
  id: string;
  title: string;
  coverImageUrl: string;
  price: number | null;
  type: string;
  condition: string | null;
  locationCity: string | null;
  locationCountry: string | null;
  sellerName: string;
}

export interface ActivityEventDto {
  id: string;
  userId: string;
  productId: string | null;
  type: string;
  description: string | null;
  timestamp: string;
  createdAt: string;
  product: ProductBriefDto | null;
}

export interface ActivityHistoryResponse {
  items: ActivityEventDto[];
  nextCursor: string | null;
  hasMore: boolean;
}

export interface ActivityHistoryQuery {
  limit?: number;
  before?: string;
  from?: string;
  to?: string;
  type?: string;
}

export interface TrackActivityRequest {
  productId?: string;
  type: string;
  description?: string;
  metadata?: string;
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const errorData = await res.json().catch(() => ({ message: "Request failed" }));
    throw new Error(errorData.message || "Request failed");
  }
  return res.json() as Promise<T>;
}

function buildQuery(query: ActivityHistoryQuery): string {
  const params = new URLSearchParams();
  if (query.limit !== undefined) params.set("limit", String(query.limit));
  if (query.before) params.set("before", query.before);
  if (query.from) params.set("from", query.from);
  if (query.to) params.set("to", query.to);
  if (query.type) params.set("type", query.type);
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export async function getActivityHistory(
  query: ActivityHistoryQuery = {}
): Promise<ActivityHistoryResponse> {
  const res = await fetch(`${BASE_URL}/activity/me/history${buildQuery(query)}`, {
    method: "GET",
    credentials: "include",
  });
  return handleResponse<ActivityHistoryResponse>(res);
}

export async function trackActivity(req: TrackActivityRequest): Promise<void> {
  await fetch(`${BASE_URL}/activity/track`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req),
  });
}
