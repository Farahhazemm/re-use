import { AuthError } from "./authService";
import type { ApiError } from "./authService";

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface DeactivateAccountRequest {
  password: string;
  reason?: string;
}

export interface DeleteAccountRequest {
  password: string;
  confirmation: string;
  reason?: string;
}

async function handleEmptyResponse(res: Response): Promise<void> {
  if (res.ok) return;

  let payload: ApiError = { code: "UNKNOWN", message: "Request failed." };
  try {
    payload = await res.json();
  } catch {
    // ignore parse errors
  }
  throw new AuthError(res.status, payload);
}

/** PUT /api/me/password — change current user's password */
export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  const res = await fetch(`${BASE_URL}/me/password`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include",
  });
  await handleEmptyResponse(res);
}

/** PATCH /api/me/deactivation — temporarily deactivate the current user's account */
export async function deactivateAccount(request: DeactivateAccountRequest): Promise<void> {
  const res = await fetch(`${BASE_URL}/me/deactivation`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include",
  });
  await handleEmptyResponse(res);
}

/** DELETE /api/me — permanently delete the current user's account */
export async function deleteAccount(request: DeleteAccountRequest): Promise<void> {
  const res = await fetch(`${BASE_URL}/me`, {
    method: "DELETE",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include",
  });
  await handleEmptyResponse(res);
}
