import { FileText } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import SystemPageShell from "./SystemPageShell";

type Tab = "terms" | "privacy";

const TABS: { key: Tab; label: string }[] = [
  { key: "terms", label: "Terms & Conditions" },
  { key: "privacy", label: "Privacy Policy" },
];

const FAQ_LINK = (
  <p className="text-sm leading-6 text-gray-600">
    <strong>Questions?</strong>
    <br />
    If you have any questions about these Terms or our Privacy Policy, please contact us:
  </p>
);

const CONTACT_DETAILS = (
  <ul className="list-none space-y-1">
    <li>
      <strong>Email:</strong>{" "}
      <a href="mailto:legal@reuse.com" className="text-[#3d2e7c] hover:underline">
        legal@reuse.com
      </a>
    </li>
    <li>
      <strong>Address:</strong> 123 Market Street, San Francisco, CA 94103
    </li>
    <li>
      <strong>Phone:</strong> (555) 123-4567
    </li>
  </ul>
);

function ExpandableSection({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <details className="group rounded-lg border border-gray-200 [&:not(:first-child)]:mt-3">
      <summary className="flex cursor-pointer items-center justify-between px-4 py-3 text-sm font-semibold text-gray-900 [&::-webkit-details-marker]:hidden">
        {title}
        <svg
          className="h-4 w-4 shrink-0 text-gray-500 transition group-open:rotate-180"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path d="m6 9 6 6 6-6" />
        </svg>
      </summary>
      <div className="border-t border-gray-200 px-4 py-3 text-sm leading-6 text-gray-700">
        {children}
      </div>
    </details>
  );
}

function ShadedBox({ children }: { children: React.ReactNode }) {
  return (
    <div className="rounded-lg bg-blue-50 border border-blue-100 px-4 py-4 text-sm leading-6 text-gray-700">
      {children}
    </div>
  );
}

function LastUpdated() {
  return (
    <p className="mb-2 text-xs font-medium uppercase tracking-wide text-gray-400">
      Last Updated: February 17, 2024
    </p>
  );
}

function TermsTab() {
  return (
    <>
      <ShadedBox>
        <LastUpdated />
        <p className="text-sm leading-6 text-gray-700">
          By using ReUse, you agree to these terms. Please read them carefully before using our
          services.
        </p>
      </ShadedBox>

      <div className="mt-3" />

      <ExpandableSection title="1. Introduction">
        <p>
          Welcome to ReUse ("we," "us," or "our"). These Terms and Conditions ("Terms") govern your
          access to and use of our website, mobile application, and services (collectively, the
          "Services").
        </p>
        <p className="mt-2">
          By accessing or using our Services, you agree to be bound by these Terms and our Privacy
          Policy. If you do not agree to these Terms, you may not access or use our Services.
        </p>
      </ExpandableSection>

      <ExpandableSection title="2. User Accounts">
        <p className="font-semibold">Account Creation</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>You must be at least 18 years old to create an account</li>
          <li>You must provide accurate and complete information</li>
          <li>You are responsible for maintaining the security of your account</li>
          <li>You must not share your account credentials with others</li>
        </ul>
        <p className="mt-3 font-semibold">Account Termination</p>
        <p className="mt-1">
          We reserve the right to suspend or terminate your account at any time for violation of
          these Terms or for any other reason at our sole discretion.
        </p>
      </ExpandableSection>

      <ExpandableSection title="3. Buying & Selling">
        <p className="font-semibold">For Sellers</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>You must accurately describe items and provide clear photos</li>
          <li>You are responsible for shipping items promptly and securely</li>
          <li>You must comply with all applicable laws regarding the sale of goods</li>
          <li>
            Prohibited items include counterfeit goods, hazardous materials, and illegal items
          </li>
        </ul>
        <p className="mt-3 font-semibold">For Buyers</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>You agree to pay for items you purchase</li>
          <li>You must confirm delivery within a reasonable timeframe</li>
          <li>You should inspect items upon receipt and report issues promptly</li>
        </ul>
      </ExpandableSection>

      <ExpandableSection title="4. Payment & Escrow">
        <p>
          All payments are processed through our secure payment system and held in escrow until
          delivery is confirmed.
        </p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>Payment is held in escrow from purchase until delivery confirmation</li>
          <li>Sellers receive payment within 3-5 business days after confirmation</li>
          <li>Buyers have 10 days to confirm delivery or open a dispute</li>
          <li>ReUse charges a service fee of 5% on all transactions</li>
        </ul>
      </ExpandableSection>

      <ExpandableSection title="5. Disputes & Refunds">
        <p>
          If you have an issue with a transaction, you may open a dispute within 10 days of
          delivery.
        </p>
        <p className="mt-2 font-semibold">Valid Reasons for Disputes:</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>Item not as described</li>
          <li>Item damaged or defective</li>
          <li>Item not received</li>
          <li>Wrong item received</li>
        </ul>
        <p className="mt-2">
          Our team will review all disputes and make a fair determination. Refunds are issued at our
          discretion based on the evidence provided.
        </p>
      </ExpandableSection>

      <ExpandableSection title="6. Limitation of Liability">
        <p>
          ReUse is a marketplace platform that connects buyers and sellers. We are not responsible
          for:
        </p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>The quality, safety, or legality of items listed</li>
          <li>The accuracy of listings or user content</li>
          <li>The ability of sellers to complete transactions</li>
          <li>The ability of buyers to pay for items</li>
        </ul>
        <p className="mt-2">
          To the maximum extent permitted by law, ReUse shall not be liable for any indirect,
          incidental, special, or consequential damages arising from your use of our Services.
        </p>
      </ExpandableSection>

      <ShadedBox>
        {FAQ_LINK}
        <div className="mt-2">{CONTACT_DETAILS}</div>
      </ShadedBox>
    </>
  );
}

function PrivacyTab() {
  return (
    <>
      <ShadedBox>
        <LastUpdated />
        <p className="text-sm leading-6 text-gray-700">
          This Privacy Policy explains how we collect, use, and protect your personal information.
        </p>
      </ShadedBox>

      <div className="mt-3" />

      <ExpandableSection title="1. Information We Collect">
        <p className="font-semibold">Personal Information</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>Name, email address, and phone number</li>
          <li>Shipping and billing addresses</li>
          <li>Payment information (processed securely by our payment partners)</li>
          <li>Profile information and photos you choose to share</li>
        </ul>
        <p className="mt-3 font-semibold">Usage Information</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>Pages viewed and features used</li>
          <li>Search queries and browsing history</li>
          <li>Device information and IP address</li>
          <li>Cookies and similar tracking technologies</li>
        </ul>
      </ExpandableSection>

      <ExpandableSection title="2. How We Use Your Information">
        <p>We use your information to:</p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>Provide and improve our Services</li>
          <li>Process transactions and send notifications</li>
          <li>Personalize your experience and show relevant content</li>
          <li>Communicate with you about your account and transactions</li>
          <li>Detect and prevent fraud and abuse</li>
          <li>Comply with legal obligations</li>
          <li>Send marketing communications (with your consent)</li>
        </ul>
      </ExpandableSection>

      <ExpandableSection title="3. Information Sharing">
        <p>We share your information with:</p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>Other users (e.g., shipping address when you make a purchase)</li>
          <li>Service providers who help us operate our platform</li>
          <li>Payment processors for transaction processing</li>
          <li>Law enforcement when required by law</li>
        </ul>
        <p className="mt-2">We never sell your personal information to third parties.</p>
      </ExpandableSection>

      <ExpandableSection title="4. Data Security">
        <p>
          We implement industry-standard security measures to protect your information, including:
        </p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>Encryption of data in transit and at rest</li>
          <li>Secure payment processing through PCI-compliant providers</li>
          <li>Regular security audits and updates</li>
          <li>Access controls and authentication requirements</li>
        </ul>
        <p className="mt-2">
          However, no method of transmission over the internet is 100% secure, and we cannot
          guarantee absolute security.
        </p>
      </ExpandableSection>

      <ExpandableSection title="5. Your Rights">
        <p>You have the right to:</p>
        <ul className="mt-2 list-disc space-y-1 pl-5">
          <li>Access the personal information we hold about you</li>
          <li>Correct inaccurate or incomplete information</li>
          <li>Delete your account and personal information</li>
          <li>Object to or restrict certain processing of your information</li>
          <li>Export your data in a portable format</li>
          <li>Opt out of marketing communications</li>
        </ul>
        <p className="mt-2">
          To exercise these rights, please contact us at{" "}
          <a href="mailto:privacy@reuse.com" className="text-[#3d2e7c] hover:underline">
            privacy@reuse.com
          </a>{" "}
          or through your account settings.
        </p>
      </ExpandableSection>

      <ExpandableSection title="6. Cookies & Tracking">
        <p>
          We use cookies and similar technologies to enhance your experience, analyze usage, and
          deliver personalized content.
        </p>
        <p className="mt-2 font-semibold">Types of Cookies We Use:</p>
        <ul className="mt-1 list-disc space-y-1 pl-5">
          <li>
            <strong>Essential cookies:</strong> Required for the platform to function
          </li>
          <li>
            <strong>Analytics cookies:</strong> Help us understand how you use our Services
          </li>
          <li>
            <strong>Preference cookies:</strong> Remember your settings and preferences
          </li>
          <li>
            <strong>Advertising cookies:</strong> Deliver relevant ads (with your consent)
          </li>
        </ul>
        <p className="mt-2">
          You can control cookies through your browser settings, but disabling certain cookies may
          affect functionality.
        </p>
      </ExpandableSection>

      <ShadedBox>
        {FAQ_LINK}
        <div className="mt-2">{CONTACT_DETAILS}</div>
      </ShadedBox>
    </>
  );
}

export default function LegalPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const activeTab: Tab = searchParams.get("tab") === "privacy" ? "privacy" : "terms";

  return (
    <SystemPageShell
      eyebrow="Terms & Privacy"
      title="Terms and privacy policy"
      description="Review the basic terms for using ReUse and how the marketplace handles user data."
      icon={<FileText className="h-7 w-7" />}
    >
      <div className="bg-gray-100 p-1 rounded-xl w-full flex">
        {TABS.map(({ key, label }) => (
          <button
            key={key}
            type="button"
            onClick={() => setSearchParams({ tab: key })}
            className={`flex-1 py-2.5 rounded-lg font-semibold text-sm transition-all ${
              activeTab === key
                ? "bg-white shadow-sm text-gray-900"
                : "text-gray-600 hover:text-gray-900"
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="mt-6">{activeTab === "terms" ? <TermsTab /> : <PrivacyTab />}</div>
    </SystemPageShell>
  );
}
