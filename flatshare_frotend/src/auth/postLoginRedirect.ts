/** Ścieżka docelowa po zalogowaniu — unikamy /preferences dla właściciela itd. */
export function getPostLoginPath(
  fromPath: string | undefined,
  role: string | undefined
): string {
  const fallback = "/offer";
  if (!fromPath) return fallback;

  if (fromPath === "/preferences" || fromPath.startsWith("/preferences/")) {
    return role === "TENANT" ? fromPath : fallback;
  }
  if (fromPath === "/my-listings" || fromPath.startsWith("/my-listings/")) {
    return role === "LANDLORD" ? fromPath : fallback;
  }
  if (fromPath === "/admin" || fromPath.startsWith("/admin/")) {
    return role === "ADMIN" ? fromPath : fallback;
  }

  return fromPath;
}
