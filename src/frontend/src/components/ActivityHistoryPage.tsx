import { useEffect, useState, useCallback } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  Package,
  Heart,
  UserPlus,
  Clock,
  History,
  AlertCircle,
  Eye,
  Search,
  MapPin,
} from "lucide-react";
import { getActivityHistory } from "../services/activityService";
import type { ActivityEventDto } from "../services/activityService";

const DATE_RANGES = [
  { key: "7d", label: "Last 7 days" },
  { key: "30d", label: "Last 30 days" },
  { key: "3m", label: "Last 3 months" },
  { key: "1y", label: "Last year" },
  { key: "all", label: "All time" },
] as const;

const TYPE_FILTERS = [
  { key: "all", label: "All" },
  { key: "product", label: "Products" },
  { key: "product.viewed", label: "Views" },
  { key: "favorite", label: "Favorites" },
  { key: "user", label: "Follows" },
  { key: "searched", label: "Searches" },
] as const;

const MAX_ITEMS = 100;

function getDateRange(dateRange: string): { from?: string; to?: string } {
  const now = new Date();
  const to = now.toISOString();

  switch (dateRange) {
    case "7d":
      return { from: new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString(), to };
    case "30d":
      return { from: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(), to };
    case "3m":
      return { from: new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000).toISOString(), to };
    case "1y":
      return { from: new Date(now.getTime() - 365 * 24 * 60 * 60 * 1000).toISOString(), to };
    default:
      return {};
  }
}

function formatRelativeTime(timestamp: string): string {
  const date = new Date(timestamp);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHr = Math.floor(diffMin / 60);
  const diffDays = Math.floor(diffHr / 24);

  if (diffSec < 60) return "Just now";
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHr < 24) return `${diffHr}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  if (diffDays < 365) {
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
  }
  return date.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
}

function getActivityInfo(type: string): { icon: typeof Package; label: string } {
  if (type.startsWith("product.view")) return { icon: Eye, label: "Viewed" };
  if (type.startsWith("product.")) return { icon: Package, label: "Product" };
  if (type.startsWith("favorite.")) return { icon: Heart, label: "Favorite" };
  if (type.startsWith("user.")) return { icon: UserPlus, label: "Follow" };
  if (type === "searched") return { icon: Search, label: "Search" };
  return { icon: Clock, label: "Activity" };
}

function formatDescription(type: string, description: string | null): string {
  if (description) return description;

  switch (type) {
    case "product.created":
      return "Created a product";
    case "product.updated":
      return "Updated a product";
    case "product.deleted":
      return "Deleted a product";
    case "product.viewed":
      return "Viewed a product";
    case "favorite.added":
      return "Added item to favorites";
    case "favorite.removed":
      return "Removed item from favorites";
    case "user.followed":
      return "Followed a user";
    case "user.unfollowed":
      return "Unfollowed a user";
    case "searched":
      return "Performed a search";
    default:
      return type;
  }
}

export function ActivityHistoryPage() {
  const [items, setItems] = useState<ActivityEventDto[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(false);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [dateRange, setDateRange] = useState<string>("all");
  const [typeFilter, setTypeFilter] = useState<string>("all");

  const fetchActivities = useCallback(
    async (before?: string) => {
      const dateParams = getDateRange(dateRange);
      const type = typeFilter === "all" ? undefined : typeFilter;

      return getActivityHistory({
        limit: 20,
        before,
        ...dateParams,
        type,
      });
    },
    [dateRange, typeFilter]
  );

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setItems([]);
    setNextCursor(null);
    setHasMore(false);

    fetchActivities()
      .then((res) => {
        if (!cancelled) {
          setItems(res.items);
          setNextCursor(res.nextCursor);
          setHasMore(res.hasMore);
          setError(null);
          setLoading(false);
        }
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setError(err.message);
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [fetchActivities]);

  const handleLoadMore = async () => {
    if (loadingMore || !nextCursor) return;
    setLoadingMore(true);

    try {
      const res = await fetchActivities(nextCursor);
      setItems((prev) => [...prev, ...res.items]);
      setNextCursor(res.nextCursor);
      setHasMore(res.hasMore);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load more");
    } finally {
      setLoadingMore(false);
    }
  };

  const reachedCap = items.length >= MAX_ITEMS;

  return (
    <div className="min-h-screen bg-gradient-to-br from-[#FAFAFA] to-[#F3F4F6]">
      {/* Header */}
      <div className="bg-gradient-to-r from-[#3d2e7c] to-[#4a3689] py-8 sm:py-10 md:py-12">
        <div className="max-w-[1000px] mx-auto px-4 sm:px-6 md:px-8">
          <div className="flex items-center gap-2 sm:gap-3 mb-2">
            <h1 className="text-white text-2xl sm:text-3xl md:text-4xl font-semibold">
              Activity History
            </h1>
            <History className="w-6 h-6 sm:w-8 sm:h-8 text-indigo-300" />
          </div>
          <p className="text-white/90 text-sm sm:text-base">Track your activity on ReUse.</p>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-[1000px] mx-auto px-4 sm:px-6 md:px-8 py-4 space-y-3">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide mr-1">
              Period
            </span>
            {DATE_RANGES.map(({ key, label }) => (
              <button
                key={key}
                type="button"
                onClick={() => setDateRange(key)}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-all ${
                  dateRange === key
                    ? "bg-[#3d2e7c] text-white shadow-sm"
                    : "bg-gray-100 text-gray-600 hover:bg-gray-200"
                }`}
              >
                {label}
              </button>
            ))}
          </div>

          <div className="flex items-center gap-2 flex-wrap">
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide mr-1">
              Type
            </span>
            {TYPE_FILTERS.map(({ key, label }) => (
              <button
                key={key}
                type="button"
                onClick={() => setTypeFilter(key)}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-all ${
                  typeFilter === key
                    ? "bg-[#3d2e7c] text-white shadow-sm"
                    : "bg-gray-100 text-gray-600 hover:bg-gray-200"
                }`}
              >
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-[1000px] mx-auto px-4 sm:px-6 md:px-8 py-6 sm:py-8">
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <div className="w-10 h-10 border-4 border-[#7C3AED] border-t-transparent rounded-full animate-spin" />
          </div>
        ) : error ? (
          <div className="text-center py-20">
            <AlertCircle className="w-12 h-12 text-red-400 mx-auto mb-4" />
            <p className="text-red-500 mb-4">{error}</p>
            <button
              type="button"
              onClick={() => window.location.reload()}
              className="px-5 py-2.5 bg-gradient-to-r from-[#4B0082] to-[#3d2e7c] text-white rounded-lg font-semibold hover:opacity-90 transition-opacity text-sm"
            >
              Try Again
            </button>
          </div>
        ) : items.length === 0 ? (
          <div className="bg-white rounded-xl sm:rounded-2xl p-8 sm:p-12 lg:p-16 text-center border border-gray-200">
            <div className="w-20 h-20 sm:w-24 sm:h-24 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4 sm:mb-6">
              <History className="w-10 h-10 sm:w-12 sm:h-12 text-gray-400" />
            </div>
            <h2 className="text-2xl sm:text-3xl font-bold text-gray-900 mb-2 sm:mb-3">
              No Activity Yet
            </h2>
            <p className="text-gray-600 text-sm sm:text-base max-w-md mx-auto">
              {typeFilter !== "all" || dateRange !== "all"
                ? "No activity found for this period. Try changing the filters."
                : "Your activity will appear here as you use ReUse."}
            </p>
          </div>
        ) : (
          <>
            <div className="space-y-3">
              {items.map((activity) => (
                <div key={activity.id}>
                  {activity.type === "product.viewed" && activity.product ? (
                    <ViewedProductCard activity={activity} />
                  ) : activity.type === "searched" ? (
                    <SearchQueryCard activity={activity} />
                  ) : (
                    <ActivityRow activity={activity} />
                  )}
                </div>
              ))}
            </div>

            <div className="mt-6 text-center">
              {reachedCap ? (
                <p className="text-sm text-gray-500">
                  Showing {MAX_ITEMS} most recent activities.{" "}
                  {dateRange === "all" ? (
                    <>
                      Use the{" "}
                      <button
                        type="button"
                        onClick={() => setDateRange("1y")}
                        className="text-[#3d2e7c] hover:underline font-medium"
                      >
                        date filter
                      </button>{" "}
                      to browse older activity.
                    </>
                  ) : (
                    "Try a broader date range to see more."
                  )}
                </p>
              ) : hasMore ? (
                <button
                  type="button"
                  onClick={handleLoadMore}
                  disabled={loadingMore}
                  className="px-6 py-3 bg-white border border-gray-300 text-gray-700 rounded-lg font-semibold text-sm hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {loadingMore ? (
                    <span className="flex items-center justify-center gap-2">
                      <div className="w-4 h-4 border-2 border-gray-400 border-t-transparent rounded-full animate-spin" />
                      Loading...
                    </span>
                  ) : (
                    "Load More"
                  )}
                </button>
              ) : null}
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function ViewedProductCard({ activity }: { activity: ActivityEventDto }) {
  const navigate = useNavigate();
  const p = activity.product!;

  return (
    <div
      className="bg-white rounded-lg border border-gray-100 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
      onClick={() => navigate(`/product/${p.id}`)}
    >
      <div className="flex items-start gap-3 sm:gap-4 p-3 sm:p-4">
        <div className="shrink-0 w-16 h-16 sm:w-20 sm:h-20 rounded-lg bg-gray-100 overflow-hidden">
          <img src={p.coverImageUrl} alt={p.title} className="w-full h-full object-cover" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <p className="text-sm sm:text-base font-medium text-gray-900 truncate">{p.title}</p>
              <div className="flex items-center gap-2 mt-0.5">
                <Eye className="w-3 h-3 text-[#3d2e7c]" />
                <span className="text-xs text-[#3d2e7c]">Viewed</span>
              </div>
            </div>
            <span className="shrink-0 text-xs text-gray-400 whitespace-nowrap">
              {formatRelativeTime(activity.timestamp)}
            </span>
          </div>
          <div className="flex items-center gap-2 mt-1.5 text-xs text-gray-500">
            {p.price != null && p.type === "Regular" && (
              <span className="font-semibold text-gray-900">${p.price}</span>
            )}
            {p.condition && (
              <span className="bg-gray-100 px-1.5 py-0.5 rounded text-xs">{p.condition}</span>
            )}
            {(p.locationCity || p.locationCountry) && (
              <span className="flex items-center gap-0.5">
                <MapPin className="w-3 h-3" />
                {[p.locationCity, p.locationCountry].filter(Boolean).join(", ")}
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function SearchQueryCard({ activity }: { activity: ActivityEventDto }) {
  const navigate = useNavigate();
  const query =
    activity.description?.replace(/^searched for\s+"|"$/g, "") ||
    activity.description ||
    "Searched";

  return (
    <div
      className="bg-white rounded-lg border border-gray-100 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
      onClick={() => navigate(`/products?search=${encodeURIComponent(query)}`)}
    >
      <div className="flex items-start gap-3 sm:gap-4 px-4 py-3 sm:px-5 sm:py-4">
        <div className="shrink-0 w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-[#f0edf8] flex items-center justify-center">
          <Search className="w-4 h-4 sm:w-5 sm:h-5 text-[#3d2e7c]" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-baseline justify-between gap-2">
            <p className="text-sm sm:text-base text-gray-900 truncate">
              {activity.description || "Performed a search"}
            </p>
            <span className="shrink-0 text-xs text-gray-400 whitespace-nowrap">
              {formatRelativeTime(activity.timestamp)}
            </span>
          </div>
          <p className="mt-0.5 text-xs text-[#3d2e7c] hover:underline">Search again →</p>
        </div>
      </div>
    </div>
  );
}

function ActivityRow({ activity }: { activity: ActivityEventDto }) {
  const { icon: Icon } = getActivityInfo(activity.type);
  const desc = formatDescription(activity.type, activity.description);

  return (
    <div className="bg-white rounded-lg border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-start gap-3 sm:gap-4 px-4 py-3 sm:px-5 sm:py-4">
        <div className="shrink-0 w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-[#f0edf8] flex items-center justify-center">
          <Icon className="w-4 h-4 sm:w-5 sm:h-5 text-[#3d2e7c]" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-baseline justify-between gap-2">
            <p className="text-sm sm:text-base text-gray-900 truncate">{desc}</p>
            <span className="shrink-0 text-xs text-gray-400 whitespace-nowrap">
              {formatRelativeTime(activity.timestamp)}
            </span>
          </div>
          {activity.type === "favorite.added" ? (
            <Link
              to="/favorites"
              className="mt-0.5 text-xs text-[#3d2e7c] hover:underline inline-block"
              onClick={(e) => e.stopPropagation()}
            >
              View favorites →
            </Link>
          ) : activity.productId ? (
            <Link
              to={`/product/${activity.productId}`}
              className="mt-0.5 text-xs text-[#3d2e7c] hover:underline inline-block"
              onClick={(e) => e.stopPropagation()}
            >
              View product →
            </Link>
          ) : null}
        </div>
      </div>
    </div>
  );
}
