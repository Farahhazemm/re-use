import { ArrowLeft, Home, SearchX } from "lucide-react";
import { useNavigate } from "react-router-dom";
import SystemPageShell from "./SystemPageShell";

export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <SystemPageShell
      eyebrow="404"
      title="Page not found"
      description="The page you requested does not exist, may have moved, or is no longer available."
      icon={<SearchX className="h-7 w-7" />}
      actions={[
        { label: "Go home", to: "/", icon: <Home className="h-4 w-4" /> },
        {
          label: "Go back",
          onClick: () => navigate(-1),
          variant: "secondary",
          icon: <ArrowLeft className="h-4 w-4" />,
        },
      ]}
    />
  );
}
