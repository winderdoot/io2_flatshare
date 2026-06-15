const BANNED_USERS_KEY = "flatshare_moderation_banned_users";

function readIds(): string[] {
  try {
    const raw = sessionStorage.getItem(BANNED_USERS_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as unknown;
    return Array.isArray(parsed)
      ? parsed.filter((id): id is string => typeof id === "string")
      : [];
  } catch {
    return [];
  }
}

export function isUserBanned(userId: string | undefined): boolean {
  if (!userId) return false;
  return readIds().includes(userId);
}

export function markUserBanned(userId: string): void {
  const ids = new Set(readIds());
  ids.add(userId);
  sessionStorage.setItem(BANNED_USERS_KEY, JSON.stringify([...ids]));
}

export function readBannedUserIds(): string[] {
  return readIds();
}
