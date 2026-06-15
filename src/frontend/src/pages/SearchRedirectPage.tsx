import { Navigate, useSearchParams } from "react-router-dom";

export default function SearchRedirectPage() {
  const [searchParams] = useSearchParams();
  const nextParams = new URLSearchParams(searchParams);
  const legacyQuery = nextParams.get("q");

  if (legacyQuery && !nextParams.has("search")) {
    nextParams.set("search", legacyQuery);
  }
  nextParams.delete("q");

  const queryString = nextParams.toString();
  return <Navigate to={`/products${queryString ? `?${queryString}` : ""}`} replace />;
}
