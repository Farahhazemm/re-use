import type { ReactNode } from "react";
import { Link } from "react-router-dom";

interface SystemPageAction {
  label: string;
  to?: string;
  onClick?: () => void;
  variant?: "primary" | "secondary";
  icon?: ReactNode;
}

interface SystemPageShellProps {
  eyebrow?: string;
  title: string;
  description: string;
  icon: ReactNode;
  children?: ReactNode;
  actions?: SystemPageAction[];
}

export default function SystemPageShell({
  eyebrow,
  title,
  description,
  icon,
  children,
  actions = [],
}: SystemPageShellProps) {
  return (
    <main className="min-h-screen bg-gray-50 px-4 py-10 text-gray-900 sm:px-6">
      <section className="mx-auto flex min-h-[calc(100vh-5rem)] w-full max-w-3xl items-center justify-center">
        <div className="w-full rounded-lg border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
          <div className="mb-5 flex h-14 w-14 items-center justify-center rounded-lg bg-[#f0edf8] text-[#3d2e7c]">
            {icon}
          </div>
          {eyebrow ? (
            <p className="mb-2 text-sm font-semibold uppercase tracking-wide text-[#3d2e7c]">
              {eyebrow}
            </p>
          ) : null}
          <h1 className="text-3xl font-bold tracking-normal text-gray-950 sm:text-4xl">{title}</h1>
          <p className="mt-3 text-base leading-7 text-gray-600">{description}</p>
          {children ? (
            <div className="mt-6 space-y-5 text-sm leading-6 text-gray-700">{children}</div>
          ) : null}
          {actions.length > 0 ? (
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              {actions.map((action) => {
                const className =
                  action.variant === "secondary"
                    ? "inline-flex items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-semibold text-gray-800 transition-colors hover:bg-gray-50"
                    : "inline-flex items-center justify-center gap-2 rounded-lg bg-[#3d2e7c] px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-[#4a3689]";

                if (action.to) {
                  return (
                    <Link key={action.label} to={action.to} className={className}>
                      {action.icon}
                      {action.label}
                    </Link>
                  );
                }

                return (
                  <button
                    key={action.label}
                    type="button"
                    onClick={action.onClick}
                    className={className}
                  >
                    {action.icon}
                    {action.label}
                  </button>
                );
              })}
            </div>
          ) : null}
        </div>
      </section>
    </main>
  );
}
