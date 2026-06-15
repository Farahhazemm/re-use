import { AlertTriangle, Home, RotateCcw } from "lucide-react";
import { isRouteErrorResponse, useRouteError } from "react-router-dom";
import SystemPageShell from "./SystemPageShell";

function getErrorMessage(error: unknown) {
  if (isRouteErrorResponse(error)) {
    return error.statusText || error.data?.message || `Request failed with status ${error.status}.`;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Something went wrong while loading this page.";
}

export default function GlobalErrorPage() {
  const routeError = useRouteError();
  const message = getErrorMessage(routeError);

  return (
    <SystemPageShell
      eyebrow="Application error"
      title="Something went wrong"
      description="The app hit an unexpected problem. You can retry the current page or return home."
      icon={<AlertTriangle className="h-7 w-7" />}
      actions={[
        {
          label: "Try again",
          onClick: () => window.location.reload(),
          icon: <RotateCcw className="h-4 w-4" />,
        },
        { label: "Go home", to: "/", variant: "secondary", icon: <Home className="h-4 w-4" /> },
      ]}
    >
      <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-amber-900">
        <p className="font-semibold">Error details</p>
        <p className="mt-1 break-words">{message}</p>
      </div>
    </SystemPageShell>
  );
}
